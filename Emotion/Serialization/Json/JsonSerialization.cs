#nullable enable

#pragma warning disable CS9080 // Use of variable in this context may expose referenced variables outside of their declaration scope

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector;
using System.Text;
using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Utility;

namespace Emotion.Serialization.JSON;

public struct JSONConfig
{
    public bool Pretty = true;
    public int Indentation = 2;

    public JSONConfig()
    {
    }
}

public static class JSONSerialization
{
    private static bool DEBUG = true;

    public static T? From<T>(string json)
    {
        return From<T>(json.AsSpan());
    }

    public static T? From<T>(ReadOnlySpan<char> jsonDataUtf16)
    {
        var reader = new ValueStringReader(jsonDataUtf16);
        return FromStateMachine<T>(ref reader);
    }

    public static T? From<T>(ReadOnlySpan<byte> jsonDataUtf8)
    {
        var reader = new ValueStringReader(jsonDataUtf8);
        return FromStateMachine<T>(ref reader);
    }

    private enum JSONReadState
    {
        None,
        DetermineNextRead,
        PostReadValue,

        StartObject,
        ReadObjectKeyValue,

        StartArray,

        ReadString,
        ReadNumber,

        ReadBoolean_True,
        ReadBoolean_False
    }

    private class JSONStackEntry
    {
        // It is possible for both the member and the object to not be defined in the
        // C# structure while being defined inthe JSON structure.
        // In this case we want to read past them still - so handle cases in which reading is done
        // implicitly (ie. by reflector methods).
        public object? CurrentObject;
        public object? ParentObject;
        public bool ParentIsList;
        public ComplexTypeHandlerMember? ObjectMember;
        public IGenericReflectorTypeHandler? TypeHandler;

        public void Reset()
        {
            CurrentObject = null;
            ParentObject = null;
            ParentIsList = false;
            ObjectMember = null;
            TypeHandler = null;
        }
    }

    private static char[] VALUE_SEPARATING_CHARACTERS = [']', '}', ','];

    private static ObjectPool<JSONStackEntry> _stackEntryPool = new ObjectPool<JSONStackEntry>((r) => r.Reset(), 10);

    public unsafe static T? FromStateMachine<T>(ref ValueStringReader reader)
    {
        ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        if (typeHandler == null) return default;

        ComplexTypeHandler<T>? complexHandler = typeHandler as ComplexTypeHandler<T>;
        if (complexHandler == null) return default;

        T? resultObj = complexHandler.CreateNewAsType();

        // Setup state machine
        Span<char> scratchMemory = stackalloc char[1024];

        JSONReadState state = JSONReadState.DetermineNextRead;
        JSONReadState lastState = JSONReadState.None;
        Stack<JSONStackEntry> stack = new Stack<JSONStackEntry>();

        JSONStackEntry newEntry = _stackEntryPool.Get();
        newEntry.CurrentObject = resultObj;
        newEntry.TypeHandler = complexHandler;
        stack.Push(newEntry);

        // Start state machine!
        while (state != JSONReadState.None)
        {
            JSONReadState stateAtStart = state;

            switch (state)
            {
                // Determine what value to read next based on the next non-whitespace character.
                case JSONReadState.DetermineNextRead:
                    {
                        char nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();
                        if (nextNonWhitespace == '{')
                            state = JSONReadState.StartObject;
                        else if (nextNonWhitespace == '[')
                            state = JSONReadState.StartArray;
                        else if (nextNonWhitespace == '\"')
                            state = JSONReadState.ReadString;
                        else if (nextNonWhitespace == 't') // true
                            state = JSONReadState.ReadBoolean_True;
                        else if (nextNonWhitespace == 'f') // false
                            state = JSONReadState.ReadBoolean_False;
                        else if ((nextNonWhitespace >= '0' && nextNonWhitespace <= '9') || nextNonWhitespace == '-')
                            state = JSONReadState.ReadNumber;
                        else
                            state = JSONReadState.None;
                    }
                    break;

                // Unwind the stack
                case JSONReadState.PostReadValue:
                    {
                        char nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace(true);

                        JSONStackEntry currentVal = stack.Peek();
                        if (currentVal.ParentIsList)
                        {
                            // This is an array - go to the next item (which should be of the same type)
                            if (nextNonWhitespace == ',')
                            {
                                // Shortcut to reading more of the same item
                                if (lastState == JSONReadState.ReadString)
                                {
                                    state = JSONReadState.ReadString;
                                }
                                else if (lastState == JSONReadState.ReadNumber)
                                {
                                    //reader.MoveCursorToNextOccuranceOfNotWhitespace();
                                    state = JSONReadState.ReadNumber;
                                }
                                else
                                {
                                    state = JSONReadState.DetermineNextRead;
                                }
                            }
                            else if (nextNonWhitespace == ']') // close array
                            {
                                // pop the generic item object
                                JSONStackEntry popped = stack.Pop(); 
                                _stackEntryPool.Return(popped);

                                // The object value being closed
                                JSONStackEntry current = stack.Peek();
                                IGenericEnumerableTypeHandler? handler = current.TypeHandler as IGenericEnumerableTypeHandler;
                                if (current.ParentObject != null && handler != null && current.CurrentObject is IList tempList)
                                {
                                    object? fromList = handler.CreateNewFromList(tempList);
                                    handler.ReturnTempListToPool(tempList);

                                    // Set the new object as the parent's member
                                    if (current.ParentIsList)
                                    {
                                        if (current.ParentObject is IList parentList)
                                            parentList.Add(fromList);
                                    }
                                    else
                                    {
                                        current.ObjectMember?.SetValueInComplexObject(current.ParentObject, fromList);
                                    }
                                }
                                state = JSONReadState.PostReadValue;
                            }
                            else
                            {
                                // This is a bug
                                Assert(false);
                                state = JSONReadState.None;
                            }
                        }
                        else // Object value
                        {
                            if (nextNonWhitespace == ',') // Read the next key value
                            {
                                JSONStackEntry popped = stack.Pop();
                                _stackEntryPool.Return(popped);

                                state = JSONReadState.ReadObjectKeyValue;
                            }
                            else if (nextNonWhitespace == '}') // close object
                            {
                                // pop the member value read last
                                // (todo: test for empty objects '{}')
                                JSONStackEntry popped = stack.Pop();
                                _stackEntryPool.Return(popped); 

                                // The object value being closed
                                JSONStackEntry current = stack.Peek();
                                object? closingObj = current.CurrentObject;
                                if (current.ParentObject != null && closingObj != null)
                                {
                                    // Set the new object as the parent's member
                                    if (current.ParentIsList)
                                    {
                                        if (current.ParentObject is IList parentList)
                                            parentList.Add(closingObj);
                                    }
                                    else
                                    {
                                        current.ObjectMember?.SetValueInComplexObject(current.ParentObject, closingObj);
                                    }
                                }

                                state = JSONReadState.PostReadValue;
                            }
                            else
                            {
                                // Final object (maybe?)
                                state = JSONReadState.None;
                            }
                        }
                    }
                    break;

                case JSONReadState.StartObject:
                    {
                        // This object was pushed into the stack by the key-value read or array read.
                        JSONStackEntry current = stack.Peek();
                        if (current.ParentObject != null && current.TypeHandler != null)
                        {
                            // Initialize object for the member object
                            IGenericReflectorComplexTypeHandler? handler = current.TypeHandler as IGenericReflectorComplexTypeHandler;
                            object? newObj = handler?.CreateNew();
                            current.CurrentObject = newObj;
                        }

                        // Start reading key value pairs.
                        state = JSONReadState.ReadObjectKeyValue;
                    }
                    break;

                case JSONReadState.ReadObjectKeyValue:
                    {
                        // Read next key (by reading up to the next value separator)
                        int charsWritten = reader.ReadUpToNextOccuranceOfChar(':', scratchMemory, true);
                        if (charsWritten == 0 || charsWritten >= scratchMemory.Length)
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        // Get current object data.
                        JSONStackEntry currentData = stack.Peek();
                        object? currentObject = currentData.CurrentObject;
                        IGenericReflectorComplexTypeHandler? currentTypeHandler = currentData.TypeHandler as IGenericReflectorComplexTypeHandler;

                        // Try to get the key handler.
                        ComplexTypeHandlerMember? memberHandler = null;
                        if (currentTypeHandler != null)
                        {
                            Span<char> writtenPart = scratchMemory.Slice(0, charsWritten);
                            int openingQuote = writtenPart.IndexOf('\"');
                            int closingQuote = writtenPart.LastIndexOf('\"');
                            if (openingQuote != -1 && openingQuote != closingQuote)
                            {
                                openingQuote++;
                                int length = closingQuote - openingQuote;
                                Span<char> nextKey = scratchMemory.Slice(openingQuote, length);
                                for (int i = 0; i < length; i++)
                                {
                                    // For JSON we enforce:
                                    // - Case insensitive keys since in C# they are usually capitalized but in JSON they aren't.
                                    // - We also enforce ASCII keys only.
                                    char c = nextKey[i];
                                    char lower = (c >= 'A' && c <= 'Z') ? (char)(c + 32) : c;
                                    nextKey[i] = lower;
                                }

                                memberHandler = currentTypeHandler.GetMemberByNameLowerCase(nextKey);
                            }
                        }

                        // Read value
                        JSONStackEntry valueReadEntry = _stackEntryPool.Get();
                        valueReadEntry.ParentObject = currentObject;
                        valueReadEntry.ObjectMember = memberHandler;
                        valueReadEntry.TypeHandler = memberHandler?.GetTypeHandler();
                        stack.Push(valueReadEntry);
                        state = JSONReadState.DetermineNextRead;
                    }
                    break;

                case JSONReadState.StartArray:
                    {
                        // Skip opening brace
                        reader.AdvancePosition(1);

                        // This object was pushed into the stack by the key-value read or array read.
                        JSONStackEntry current = stack.Peek();
                        IGenericEnumerableTypeHandler? handler = current.TypeHandler as IGenericEnumerableTypeHandler;
                        if (current.ParentObject != null && handler != null)
                        {
                            // Initialize temporary list to write in from a typed pool to prevent boxing
                            IList tempList = handler.CreateTempListFromPool();
                            current.CurrentObject = tempList;
                        }

                        // Push a generic array member as the stack value and start reading items
                        IGenericReflectorTypeHandler? elementHandler = null;
                        if (handler?.ItemType != null)
                            elementHandler = ReflectorEngine.GetTypeHandler(handler.ItemType);

                        // Read value
                        JSONStackEntry genericItemEntry = _stackEntryPool.Get();
                        genericItemEntry.ParentObject = current.CurrentObject;
                        genericItemEntry.ParentIsList = true;
                        genericItemEntry.TypeHandler = elementHandler;
                        stack.Push(genericItemEntry);
                        state = JSONReadState.DetermineNextRead;
                    }
                    break;

                case JSONReadState.ReadString:
                    {
                        // todo: strings larger than the scratch memory will not be read...
                        int charsWritten = reader.ReadNextQuotedString(scratchMemory, true);
                        if (charsWritten == 0 || charsWritten >= scratchMemory.Length)
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        // Set value in parent (if any).
                        JSONStackEntry currentVal = stack.Peek();
                        if (currentVal.ParentObject != null && currentVal.TypeHandler != null)
                        {
                            // Allocate string
                            Span<char> stringContent = scratchMemory.Slice(0, charsWritten);
                            var val = new string(stringContent);

                            if (currentVal.ParentIsList && currentVal.ParentObject is IList parentList)
                                parentList.Add(val);
                            else
                                currentVal.ObjectMember!.SetValueInComplexObject(currentVal.ParentObject, val);
                        }

                        state = JSONReadState.PostReadValue;
                    }
                    break;

                case JSONReadState.ReadNumber:
                    {
                        int charsRead = reader.ReadUpToNextOccuranceOfAnyOfChars(VALUE_SEPARATING_CHARACTERS, scratchMemory, false);
                        if (charsRead == 0 || charsRead >= scratchMemory.Length)
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        JSONStackEntry current = stack.Peek();
                        object? parentObj = current.ParentObject;
                        IGenericReflectorTypeHandler? handler = current.TypeHandler;
                        if (parentObj != null && handler != null)
                        {
                            Span<char> readData = scratchMemory.Slice(0, charsRead);

                            if (current.ParentIsList)
                                handler.ReadValueFromStringIntoList(readData, parentObj);
                            else if (current.ObjectMember != null)
                                handler.ReadValueFromStringIntoObjectMember(readData, parentObj, current.ObjectMember);
                        }

                        state = JSONReadState.PostReadValue;
                    }
                    break;

                case JSONReadState.ReadBoolean_True:
                case JSONReadState.ReadBoolean_False:
                    {
                        bool value;
                        if (state == JSONReadState.ReadBoolean_True)
                        {
                            reader.AdvancePosition(4); // t r u e
                            value = true;
                        }
                        else
                        {
                            reader.AdvancePosition(5); // f a l s e
                            value = false;
                        }

                        JSONStackEntry current = stack.Peek();
                        object? parentObj = current.ParentObject;
                        if (parentObj != null)
                        {
                            if (current.ParentIsList && parentObj is IList parentList)
                                parentList.Add(value);
                            else if (current.ObjectMember != null)
                                current.ObjectMember?.SetValueInComplexObject(parentObj, value);
                        }

                        state = JSONReadState.PostReadValue;
                    }
                    break;
            }

            lastState = stateAtStart;
        }

        while (stack.TryPop(out JSONStackEntry? storedEntry))
        {
            _stackEntryPool.Return(storedEntry);
        }

        return resultObj;
    }

    public static string To<T>(T obj)
    {
        return To<T>(obj, new JSONConfig());
    }

    public static string To<T>(T obj, JSONConfig config)
    {
        var builder = new StringBuilder();
        var reader = new ValueStringWriter(builder);
        if (To(obj, config, ref reader) == -1) return string.Empty;
        return builder.ToString();
    }

    public static int To<T>(T obj, JSONConfig config, Span<char> jsonDataUtf16)
    {
        var reader = new ValueStringWriter(jsonDataUtf16);
        return To(obj, config, ref reader);
    }

    public static int To<T>(T obj, JSONConfig config, Span<byte> jsonDataUtf8)
    {
        var reader = new ValueStringWriter(jsonDataUtf8);
        return To(obj, config, ref reader);
    }

    public static int To<T>(T obj, JSONConfig config, ref ValueStringWriter writer)
    {
        return 0;
    }
}

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
        ReadBoolean
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
                        else if (nextNonWhitespace == 't' || nextNonWhitespace == 'f') // true or false
                            state = JSONReadState.ReadBoolean;
                        else if ((nextNonWhitespace >= '0' && nextNonWhitespace <= '9') || nextNonWhitespace == '-')
                            state = JSONReadState.ReadNumber;
                        else
                            state = JSONReadState.None;
                    }
                    break;

                // Unwind the stack
                case JSONReadState.PostReadValue:
                    {
                        char nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();

                        JSONStackEntry currentVal = stack.Peek();
                        if (currentVal.ParentIsList)
                        {
                            // This is an array - go to the next item (which should be of the same type)
                            if (nextNonWhitespace == ',')
                            {
                                reader.ReadNextChar();
                                nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();

                                // Shortcut to reading more of the same item
                                if (lastState == JSONReadState.ReadNumber || lastState == JSONReadState.ReadNumber)
                                    state = lastState;
                                else
                                    state = JSONReadState.DetermineNextRead;
                            }
                            else if (nextNonWhitespace == ']')
                            {
                                // close array

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
                                reader.ReadNextChar(); // Skip closing bracket
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
                            // Read the next key value
                            if (nextNonWhitespace == ',')
                            {
                                JSONStackEntry popped = stack.Pop();
                                _stackEntryPool.Return(popped);

                                state = JSONReadState.ReadObjectKeyValue;
                                reader.ReadNextChar(); // comma
                            }
                            else if (nextNonWhitespace == '}')
                            {
                                // close object

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
                                reader.ReadNextChar(); // bracket
                            }
                            else
                            {
                                // Final object
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
                        // Find next key open
                        if (!reader.MoveCursorToNextOccuranceOfChar('\"'))
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        // Read key
                        reader.ReadNextChar();
                        int charsWritten = reader.ReadToNextOccuranceofChar('\"', scratchMemory);
                        if (charsWritten == 0)
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
                            Span<char> nextKey = scratchMemory.Slice(0, charsWritten);

                            // For JSON we enforce case insensitive keys since in C# they are
                            // usually capitalized but in JSON they aren't.
                            for (int i = 0; i < nextKey.Length; i++)
                            {
                                nextKey[i] = char.ToLowerInvariant(nextKey[i]);
                            }

                            int tagNameHash = nextKey.GetStableHashCode();
                            memberHandler = currentTypeHandler.GetMemberByNameCaseInsensitive(tagNameHash);
                        }

                        // Find the value separator.
                        if (!reader.MoveCursorToNextOccuranceOfChar(':'))
                        {
                            state = JSONReadState.None;
                            break;
                        }
                        reader.ReadNextChar();

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
                        // This object was pushed into the stack by the key-value read or array read.
                        JSONStackEntry current = stack.Peek();
                        IGenericEnumerableTypeHandler? handler = current.TypeHandler as IGenericEnumerableTypeHandler;
                        if (current.ParentObject != null && handler != null)
                        {
                            // Initialize temporary list to write in from a typed pool to prevent boxing
                            IList tempList = handler.CreateTempListFromPool();
                            current.CurrentObject = tempList;
                        }

                        // Skip opening brace
                        reader.ReadNextChar();

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
                        char openingChar = reader.ReadNextChar();
                        if (openingChar != '\"')
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        int stringChars = reader.ReadToNextOccuranceofChar('\"', scratchMemory);
                        if (stringChars == 0)
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        // todo: big strings will fuck us up :/
                        Span<char> stringContent = scratchMemory.Slice(0, stringChars);

                        char closing = reader.ReadNextChar();
                        if (closing != '\"')
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        // Allocate string
                        var val = new string(stringContent);

                        // Set value in parent (if any).
                        JSONStackEntry currentVal = stack.Peek();
                        if (currentVal.ParentObject != null && currentVal.TypeHandler != null)
                        {
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
                        JSONStackEntry current = stack.Peek();
                        object? parentObj = current.ParentObject;
                        IGenericReflectorTypeHandler? handler = current.TypeHandler;
                        if (parentObj != null && handler != null)
                        {
                            if (current.ParentIsList)
                                handler.ReadValueFromStringIntoList(ref reader, parentObj);
                            else if (current.ObjectMember != null)
                                handler.ReadValueFromStringIntoObjectMember(ref reader, parentObj, current.ObjectMember);
                            else
                                reader.ReadNumber(scratchMemory);
                        }
                        else
                        {
                            reader.ReadNumber(scratchMemory);
                        }

                        state = JSONReadState.PostReadValue;
                    }
                    break;

                case JSONReadState.ReadBoolean:
                    {
                        reader.ReadToNextOccuranceofChar('e', scratchMemory);
                        reader.ReadNextChar(); // next

                        bool value = scratchMemory[0] == 't';

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

    public unsafe static T? From<T>(ref ValueStringReader reader)
    {
        ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        if (typeHandler == null) return default;

        IGenericReflectorComplexTypeHandler? complexHandler = typeHandler as IGenericReflectorComplexTypeHandler;
        if (complexHandler == null) return default;

        Type requestedType = typeof(T);
        Span<char> readMemory = stackalloc char[1024];
        if (!reader.MoveCursorToNextOccuranceOfChar('{')) return default;

        object? result = ReadObject(ref reader, readMemory, complexHandler);
        return (T?)result;
    }

    private static object? ReadObject(
        ref ValueStringReader reader,
        Span<char> scratchMemory,
        IGenericReflectorComplexTypeHandler? objHandler
    )
    {
        char c = reader.ReadNextChar();
        if (c != '{') return null;

        object? obj = null;
        if (objHandler != null)
            obj = objHandler.CreateNew();

        bool firstLoop = true;

        // Read key-value pairs until closing of object.
        while (true)
        {
            // Go to next key-value (if any)
            char nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();
            if (nextNonWhitespace == '}')
            {
                reader.ReadNextChar();
                break;
            }
            else if (firstLoop && nextNonWhitespace == '\"')
            {
                // Thats fine - it's the first tag
            }
            else if (nextNonWhitespace != ',')
            {
                return null;
            }
            firstLoop = false;

            // Start reading tag
            if (!reader.MoveCursorToNextOccuranceOfChar('\"')) return null;
            reader.ReadNextChar();
            int charsWritten = reader.ReadToNextOccuranceofChar('\"', scratchMemory);
            if (charsWritten == 0) return null;

            Span<char> nextTag = scratchMemory.Slice(0, charsWritten);
            ComplexTypeHandlerMember? member = null;

            if (objHandler != null)
            {
                // For JSON we enforce case insensitive keys since in C# they are
                // usually capitalized but in JSON they aren't.
                for (int i = 0; i < nextTag.Length; i++)
                {
                    nextTag[i] = char.ToLowerInvariant(nextTag[i]);
                }

                int tagNameHash = nextTag.GetStableHashCode();
                member = objHandler.GetMemberByNameCaseInsensitive(tagNameHash);
            }

            // Go to value delim
            if (!reader.MoveCursorToNextOccuranceOfChar(':')) return null;
            reader.ReadNextChar();

            IGenericReflectorTypeHandler? memberHandler = member?.GetTypeHandler();
            ReadJSONValue(ref reader, scratchMemory, memberHandler, member, obj);
        }

        return obj;
    }

    private static void ReadArray(
        ref ValueStringReader reader,
        Span<char> scratchMemory,
        IGenericEnumerableTypeHandler? typeHandler,
        ComplexTypeHandlerMember? parentObjectMember,
        object? parentObject
    )
    {
        char c = reader.ReadNextChar();
        if (c != '[') return;

        IList? list = null;
        IGenericReflectorTypeHandler? itemTypeHandler = null;
        if (typeHandler != null)
        {
            Type itemType = typeHandler.ItemType;
            itemTypeHandler = ReflectorEngine.GetTypeHandler(itemType);
            list = typeHandler.CreateTempListFromPool();
        }

        bool firstLoop = true;

        while (true)
        {
            char nextNonWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();
            if (nextNonWhitespace == ']')
            {
                reader.ReadNextChar();
                break;
            }
            else if (firstLoop)
            {
                // That's ok - it's the first one so no need for a comma
            }
            else if (nextNonWhitespace == ',')
            {
                reader.ReadNextChar(); // Skip comma (todo: trailing comma?)
            }
            else
            {
                return;
            }
            firstLoop = false;

            ReadJSONValue(ref reader, scratchMemory, itemTypeHandler, null, list);
        }

        if (list != null)
        {
            AssertNotNull(typeHandler);
            object? arrayRead = typeHandler.CreateNewFromList(list);
            AssertNotNull(arrayRead);

            typeHandler.ReturnTempListToPool(list);

            if (parentObject != null)
            {
                if (parentObject is IList parentList)
                    parentList.Add(arrayRead);
                else
                    parentObjectMember?.SetValueInComplexObject(parentObject, arrayRead);
            }
        }
    }

    private static void ReadJSONValue(
        ref ValueStringReader reader,
        Span<char> scratchMemory,
        IGenericReflectorTypeHandler? typeHandlerOfHolder,
        ComplexTypeHandlerMember? parentObjectMember,
        object? parentObject
    )
    {
        // Peak the next character that isnt whitespace
        char charAfterWhitespace = reader.MoveCursorToNextOccuranceOfNotWhitespace();

        // Object value opened
        if (charAfterWhitespace == '{')
        {
            IGenericReflectorComplexTypeHandler? handler = typeHandlerOfHolder as IGenericReflectorComplexTypeHandler;
            object? objRead = ReadObject(ref reader, scratchMemory, handler);

            if (parentObject != null && typeHandlerOfHolder != null)
            {
                if (parentObject is IList parentList)
                    parentList.Add(objRead);
                else if (parentObjectMember != null)
                    parentObjectMember.SetValueInComplexObject(parentObject, objRead);
            }
        }
        // Array value opened
        else if (charAfterWhitespace == '[')
        {
            IGenericEnumerableTypeHandler? handler = typeHandlerOfHolder as IGenericEnumerableTypeHandler;
            ReadArray(ref reader, scratchMemory, handler, parentObjectMember, parentObject);
        }
        // String value opened
        else if (charAfterWhitespace == '\"')
        {
            reader.ReadNextChar();
            int charsWritten = reader.ReadToNextOccuranceofChar('\"', scratchMemory);
            if (charsWritten == 0) return;

            // todo: big strings will fuck us up :/
            Span<char> stringContent = scratchMemory.Slice(0, charsWritten);

            char closing = reader.ReadNextChar();
            if (closing != '\"') return;

            string val = new string(stringContent);

            if (parentObject != null && typeHandlerOfHolder != null)
            {
                if (parentObject is IList parentList)
                    parentList.Add(val);
                else if (parentObjectMember != null)
                    parentObjectMember.SetValueInComplexObject(parentObject, val);
            }
        }
        else if (char.IsNumber(charAfterWhitespace) || charAfterWhitespace == '-')
        {
            if (parentObject != null && typeHandlerOfHolder != null)
            {
                if (parentObjectMember == null)
                    typeHandlerOfHolder.ReadValueFromStringIntoList(ref reader, parentObject);
                else
                    typeHandlerOfHolder.ReadValueFromStringIntoObjectMember(ref reader, parentObject, parentObjectMember);
            }
            else
            {
                reader.ReadNumber(scratchMemory);
            }
        }
        else if (charAfterWhitespace == 't' || charAfterWhitespace == 'f') // true or false
        {
            reader.ReadToNextOccuranceofChar('e', scratchMemory);
            reader.ReadNextChar(); // next

            bool value = charAfterWhitespace == 't';

            if (parentObject != null && typeHandlerOfHolder != null)
            {
                if (parentObject is IList parentList)
                    parentList.Add(value);
                else if (parentObjectMember != null)
                    parentObjectMember.SetValueInComplexObject(parentObject, value);
            }
        }
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

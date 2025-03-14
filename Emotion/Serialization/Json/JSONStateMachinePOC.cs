#if PROTOTYPE

#nullable enable

using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using System.Buffers;

namespace Emotion.Serialization.Json;

/// <summary>
/// This is an imperfect but kind of functional proof of concept JSON reader.
/// It is unused as the one provided by System.Text.JSON is faster and safer.
/// </summary>
public static class JSONStateMachine
{
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
        ReadBoolean_False,
    }

    private static char[] VALUE_SEPARATING_CHARACTERS = [']', '}', ','];
    private static char[] WHITESPACE = [' ', '\n', '\t', '\r'];
    private static char[] ARRAY_OPEN_CLOSE = ['[', ']'];

    private struct JSONStackEntry
    {
        // It is possible for both the member and the object to not be defined in the
        // C# structure while being defined inthe JSON structure.
        // In this case we want to read past them still - so handle cases in which reading is done
        // implicitly (ie. by reflector methods).
        public object? CurrentObject;
        public object? ParentObject;
        public IList? ParentList;
        public ComplexTypeHandlerMember? ObjectMember;
        public IGenericReflectorTypeHandler? TypeHandler;

        // Read state
        public bool ParentIsList;

        public void Reset()
        {
            CurrentObject = null;
            ParentObject = null;
            ParentList = null;
            ObjectMember = null;
            TypeHandler = null;

            ParentIsList = false;
        }
    }

    public unsafe static T? FromStateMachine<T>(ref JSONStateMachineValueStringReader reader)
    {
        ReflectorTypeHandlerBase<T>? typeHandler = ReflectorEngine.GetTypeHandler<T>();
        if (typeHandler == null) return default;

        ComplexTypeHandler<T>? complexHandler = typeHandler as ComplexTypeHandler<T>;
        if (complexHandler == null) return default;

        T? resultObj = complexHandler.CreateNewAsType();

        // Setup state machine
        Span<Range> splitScratchMemory = stackalloc Range[256];

        JSONReadState state = JSONReadState.DetermineNextRead;
        JSONReadState lastState = JSONReadState.None;

        JSONStackEntry[] stack = ArrayPool<JSONStackEntry>.Shared.Rent(128);
        int currentStackPointer = -1;

        // Push current in the stack.
        {
            currentStackPointer++;
            ref JSONStackEntry newHead = ref stack[currentStackPointer];
            newHead.Reset();
            newHead.CurrentObject = resultObj;
            newHead.TypeHandler = complexHandler;
        }

        // Start state machine!
        while (state != JSONReadState.None)
        {
            JSONReadState stateAtStart = state;

            switch (state)
            {
                // Determine what value to read next based on the next non-whitespace character.
                case JSONReadState.DetermineNextRead:
                    {
                        char nextNonWhitespace = reader.ReadToNextOccuranceOfAnyExcept(WHITESPACE);

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
                        char nextNonWhitespace = reader.ReadToNextOccuranceOfAnyExcept(WHITESPACE, true);

                        ref JSONStackEntry currentStackEntry = ref stack[currentStackPointer];
                        if (currentStackEntry.ParentIsList)
                        {
                            // This is an array - go to the next item (which should be of the same type)
                            if (nextNonWhitespace == ',')
                            {
                                // Shortcut to reading more of the same item
                                if (lastState == JSONReadState.ReadString || lastState == JSONReadState.ReadNumber)
                                    state = lastState;
                                else
                                    state = JSONReadState.DetermineNextRead;
                            }
                            else if (nextNonWhitespace == ']') // close array
                            {
                                // Get the temp list from the element handler (which should be current)
                                IList? tempList = currentStackEntry.ParentList;

                                // pop the generic item object
                                currentStackPointer--;
                                if (currentStackPointer < 0)
                                {
                                    state = JSONReadState.None;
                                    break;
                                }

                                // The object value being closed
                                currentStackEntry = ref stack[currentStackPointer];
                                IGenericEnumerableTypeHandler? handler = currentStackEntry.TypeHandler as IGenericEnumerableTypeHandler;
                                if (handler != null && tempList != null)
                                {
                                    object fromList = handler.CreateNewFromList(tempList);
                                    handler.ReturnTempListToPool(tempList);

                                    // Set the new object as the parent's member
                                    if (currentStackEntry.ParentList != null)
                                        currentStackEntry.ParentList.Add(fromList);
                                    else if (currentStackEntry.ParentObject != null)
                                        currentStackEntry.ObjectMember!.SetValueInComplexObject(currentStackEntry.ParentObject, fromList);
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
                                // Pop the current key
                                currentStackPointer--;
                                if (currentStackPointer < 0)
                                {
                                    state = JSONReadState.None;
                                    break;
                                }

                                state = JSONReadState.ReadObjectKeyValue;
                            }
                            else if (nextNonWhitespace == '}') // close object
                            {
                                // pop the member value read last
                                // (todo: test for empty objects '{}')
                                currentStackPointer--;
                                if (currentStackPointer < 0)
                                {
                                    state = JSONReadState.None;
                                    break;
                                }

                                // The object value being closed
                                currentStackEntry = ref stack[currentStackPointer];
                                object? closingObj = currentStackEntry.CurrentObject;
                                if (currentStackEntry.TypeHandler != null && closingObj != null)
                                {
                                    // Set the new object as the parent's member
                                    if (currentStackEntry.ParentList != null)
                                        currentStackEntry.ParentList.Add(closingObj);
                                    else if (currentStackEntry.ParentObject != null)
                                        currentStackEntry.ObjectMember!.SetValueInComplexObject(currentStackEntry.ParentObject, closingObj);
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
                        // CurrentObject is already initialized only for the root object case.
                        ref JSONStackEntry currentStackEntry = ref stack[currentStackPointer];
                        if (currentStackPointer != 0 && currentStackEntry.TypeHandler != null)
                        {
                            // Initialize object for the member object
                            IGenericReflectorComplexTypeHandler? handler = currentStackEntry.TypeHandler as IGenericReflectorComplexTypeHandler;
                            object? newObj = handler?.CreateNew();
                            currentStackEntry.CurrentObject = newObj;
                        }

                        // Start reading key value pairs.
                        state = JSONReadState.ReadObjectKeyValue;
                    }
                    break;

                case JSONReadState.ReadObjectKeyValue:
                    {
                        // Read next key (by reading up to the next value separator)
                        SimpleSpanRange readRange = reader.ReadToNextOccuranceOf(':', true);
                        if (readRange.IsInvalid)
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        // Get current object data.
                        ref JSONStackEntry currentStackEntry = ref stack[currentStackPointer];
                        IGenericReflectorComplexTypeHandler? currentTypeHandler = currentStackEntry.TypeHandler as IGenericReflectorComplexTypeHandler;

                        // Try to get the key handler.
                        ComplexTypeHandlerMember? memberHandler = null;
                        IGenericReflectorTypeHandler? memberTypeHandler = null;
                        if (currentTypeHandler != null)
                        {
                            ReadOnlySpan<char> writtenPart = reader.GetSpanSlice(readRange);
                            int openingQuote = writtenPart.IndexOf('\"');
                            int closingQuote = writtenPart.LastIndexOf('\"');
                            if (openingQuote != -1 && openingQuote != closingQuote)
                            {
                                openingQuote++;
                                int length = closingQuote - openingQuote;
                                writtenPart = writtenPart.Slice(openingQuote, length);

                                memberHandler = currentTypeHandler.GetMemberByNameLowerCase(writtenPart);
                                memberTypeHandler = memberHandler?.GetTypeHandler();
                                if (memberTypeHandler == null) memberHandler = null;
                            }
                        }

                        // Push key read value
                        {
                            currentStackPointer++;
                            if (currentStackPointer >= stack.Length)
                            {
                                state = JSONReadState.None;
                                break;
                            }

                            ref JSONStackEntry newHead = ref stack[currentStackPointer];
                            newHead.Reset();

                            if (memberHandler != null)
                            {
                                newHead.ParentObject = currentStackEntry.CurrentObject;
                                newHead.ObjectMember = memberHandler;
                                newHead.TypeHandler = memberTypeHandler;
                            }
                        }
                        state = JSONReadState.DetermineNextRead;
                    }
                    break;

                case JSONReadState.StartArray:
                    {
                        // Skip opening brace
                        reader.AdvancePosition(1);

                        // This object was pushed into the stack by the key-value read or array read.
                        ref JSONStackEntry currentStackEntry = ref stack[currentStackPointer];
                        IGenericEnumerableTypeHandler? handler = currentStackEntry.TypeHandler as IGenericEnumerableTypeHandler;

                        // Push a generic array member as the stack value and start reading items
                        IGenericReflectorTypeHandler? elementHandler = null;
                        if (handler?.ItemType != null)
                            elementHandler = ReflectorEngine.GetTypeHandler(handler.ItemType);

                        // ============================
                        #region OptimizedShortcut_PrimitiveArray
                        // We can guess the type using our member knowledge... but if the JSON wants to screw us up - it will.
                        if (currentStackPointer != 0 && elementHandler != null && elementHandler.CanGetOrParseValueAsString)
                        {
                            int checkpoint = reader.GetCurrentPosition();

                            SimpleSpanRange readRange = reader.ReadToNextOccuranceOfAnyOf(ARRAY_OPEN_CLOSE, true);
                            if (readRange.IsInvalid)
                            {
                                state = JSONReadState.None;
                                break;
                            }

                            ReadOnlySpan<char> span = reader.GetSpanSlice(readRange);
                            if (span[0] != '[')
                            {
                                int elements = span.Split(splitScratchMemory, ',');
                                if (elements < splitScratchMemory.Length)
                                {
                                    object array = handler.CreateNewOfSize(elements);
                                    currentStackEntry.CurrentObject = array;

                                    for (int i = 0; i < elements; i++)
                                    {
                                        ref Range currentSplit = ref splitScratchMemory[i];
                                        ReadOnlySpan<char> splitSpan = span[currentSplit];
                                        elementHandler.ReadValueFromStringIntoArray(splitSpan, array, i);
                                    }

                                    // Set the new object as the parent's member
                                    if (currentStackEntry.ParentList != null)
                                        currentStackEntry.ParentList.Add(array);
                                    else if (currentStackEntry.ParentObject != null)
                                        currentStackEntry.ObjectMember!.SetValueInComplexObject(currentStackEntry.ParentObject, array);

                                    state = JSONReadState.PostReadValue;
                                    break;
                                }
                                else
                                {
                                    // Yikes - too many elements, restore checkpoint
                                    reader.SetPosition(checkpoint);
                                }
                            }
                            else
                            {
                                // We failed at the optimization, and this is actually an array of arrays - :(
                                // Restore checkpoint and read again - however keep in mind that this means the current handler is incorrect.
                                handler = null;
                                elementHandler = null;
                                reader.SetPosition(checkpoint);
                            }
                        }
                        #endregion
                        // ============================

                        IList? tempList = null;
                        if (handler != null)
                        {
                            // Initialize temporary list to write in from a typed pool to prevent boxing
                            tempList = handler.CreateTempListFromPool();
                            currentStackEntry.CurrentObject = tempList;
                        }

                        // Push array read value
                        {
                            currentStackPointer++;
                            if (currentStackPointer >= stack.Length)
                            {
                                state = JSONReadState.None;
                                break;
                            }

                            ref JSONStackEntry newHead = ref stack[currentStackPointer];
                            newHead.Reset();
                            newHead.ParentIsList = true;

                            if (elementHandler != null)
                            {
                                newHead.ParentList = tempList;
                                newHead.TypeHandler = elementHandler;
                            }
                        }
                        state = JSONReadState.DetermineNextRead;
                    }
                    break;

                case JSONReadState.ReadString:
                    {
                        SimpleSpanRange readRange = reader.ReadNextQuotedString(true);
                        if (readRange.IsInvalid)
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        // Set value in parent (if any).
                        ref JSONStackEntry currentStackEntry = ref stack[currentStackPointer];
                        if (currentStackEntry.TypeHandler != null)
                        {
                            // Allocate string
                            ReadOnlySpan<char> stringContent = reader.GetSpanSlice(readRange);
                            var val = new string(stringContent);

                            if (currentStackEntry.ParentList != null)
                                currentStackEntry.ParentList.Add(val);
                            else if (currentStackEntry.ParentObject != null)
                                currentStackEntry.ObjectMember!.SetValueInComplexObject(currentStackEntry.ParentObject, val);
                        }

                        state = JSONReadState.PostReadValue;
                    }
                    break;

                case JSONReadState.ReadNumber:
                    {
                        SimpleSpanRange readRange = reader.ReadToNextOccuranceOfAnyOf(VALUE_SEPARATING_CHARACTERS, false);
                        if (readRange.IsInvalid)
                        {
                            state = JSONReadState.None;
                            break;
                        }

                        ref JSONStackEntry currentStackEntry = ref stack[currentStackPointer];
                        IGenericReflectorTypeHandler? handler = currentStackEntry.TypeHandler;
                        if (handler != null)
                        {
                            ReadOnlySpan<char> readData = reader.GetSpanSlice(readRange);
                            if (currentStackEntry.ParentList != null)
                                handler.ReadValueFromStringIntoList(readData, currentStackEntry.ParentList);
                            else if (currentStackEntry.ParentObject != null)
                                handler.ReadValueFromStringIntoObjectMember(readData, currentStackEntry.ParentObject, currentStackEntry.ObjectMember!);
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

                        ref JSONStackEntry currentStackEntry = ref stack[currentStackPointer];
                        if (currentStackEntry.TypeHandler != null)
                        {
                            if (currentStackEntry.ParentList != null)
                                currentStackEntry.ParentList.Add(value);
                            else if (currentStackEntry.ParentObject != null)
                                currentStackEntry.ObjectMember?.SetValueInComplexObject(currentStackEntry.ParentObject, value);
                        }

                        state = JSONReadState.PostReadValue;
                    }
                    break;
            }

            lastState = stateAtStart;
        }

        ArrayPool<JSONStackEntry>.Shared.Return(stack);

        return resultObj;
    }
}
#endif
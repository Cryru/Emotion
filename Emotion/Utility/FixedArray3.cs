﻿#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using Emotion.Common.Serialization;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// A fixed size struct array of size 3.
    /// </summary>
    public struct FixedArray3<T> : IEnumerable<T>
    {
        public T Value0, Value1, Value2;

        public int Length
        {
            get => 3;
        }

        [DontSerialize]
        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Value0;
                    case 1:
                        return Value1;
                    case 2:
                        return Value2;
                    default:
                        throw new IndexOutOfRangeException(nameof(index));
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Value0 = value;
                        break;
                    case 1:
                        Value1 = value;
                        break;
                    case 2:
                        Value2 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException(nameof(index));
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T value)
        {
            for (var i = 0; i < 3; i++)
            {
                if (this[i].Equals(value))
                    return i;
            }

            return -1;
        }

        public void Clear()
        {
            Value0 = Value1 = Value2 = default;
        }

        private IEnumerable<T> Enumerate()
        {
            yield return Value0;
            yield return Value1;
            yield return Value2;
        }
    }
}
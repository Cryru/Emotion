#region Using

using System;

#endregion

#nullable enable

namespace Emotion.Game.AnimationGraph
{
    public struct AnimationGraphVariableInt : IAnimationGraphVariable
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public IAnimationGraphVariable Clone()
        {
            return new AnimationGraphVariableInt {Value = Value};
        }

        public bool SetValue(object value)
        {
            var valueAsInt = (int) value;
            if (Value == valueAsInt) return false;
            Value = valueAsInt;
            return true;
        }

        public static bool operator ==(AnimationGraphVariableInt a, AnimationGraphVariableInt b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(AnimationGraphVariableInt a, AnimationGraphVariableInt b)
        {
            return a.Value != b.Value;
        }

        public bool Equals(AnimationGraphVariableInt other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is AnimationGraphVariableInt other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
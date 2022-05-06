#region Using

using System;

#endregion

#nullable enable

namespace Emotion.Game.AnimationGraph
{
    public struct AnimationGraphVariableBool : IAnimationGraphVariable
    {
        public string Name { get; set; }
        public bool Value { get; set; }

        public IAnimationGraphVariable Clone()
        {
            return new AnimationGraphVariableBool {Value = Value};
        }

        public bool SetValue(object value)
        {
            var valueAsBool = (bool) value;
            if (Value == valueAsBool) return false;
            Value = valueAsBool;
            return true;
        }

        public static bool operator ==(AnimationGraphVariableBool a, AnimationGraphVariableBool b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(AnimationGraphVariableBool a, AnimationGraphVariableBool b)
        {
            return a.Value != b.Value;
        }

        public bool Equals(AnimationGraphVariableBool other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is AnimationGraphVariableBool other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
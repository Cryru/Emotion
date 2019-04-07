#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Adfectus.Game.Time
{
    internal class Tween : TimerInstance
    {
        public static float Bounciness = 1.70158f;

        private float _timePassed;

        private float _duration;
        private object _objectTarget;
        private object _objectTween;
        private TweenMethod _method;
        private TweenType _type;
        private Action _after;

        /// <summary>
        /// Members which exist in both the target and tween objects.
        /// </summary>
        private Dictionary<string, float> _crossover;

        /// <summary>
        /// The interpolation function of the tween.
        /// </summary>
        private Func<float, float> _func;

        /// <summary>
        /// The value of the interpolated value from the last update.
        /// </summary>
        private float _prevS;

        internal Tween(float duration, ref object objectTarget, ref object objectTween, TweenType type, TweenMethod method, Action after = null)
        {
            _duration = duration;
            _objectTarget = objectTarget;
            _objectTween = objectTween;
            _type = type;
            _method = method;
            _after = after;

            _crossover = new Dictionary<string, float>();

            MemberInfo[] tweenMembers = _objectTween.GetType().GetMembers();
            MemberInfo[] targetMembers = _objectTarget.GetType().GetMembers();

            // Find matches.
            foreach (MemberInfo tweenMember in tweenMembers)
            {
                // Filter everything that isn't a field or property.
                if (tweenMember.MemberType != MemberTypes.Field && tweenMember.MemberType != MemberTypes.Property) continue;
                Type tweenMemberType = TypeOfMember(tweenMember);

                // Filter not double convertible types.
                if (tweenMemberType != typeof(float) && tweenMemberType != typeof(double) && tweenMemberType != typeof(decimal)) continue;

                // Check if name and type match. Add it and calculate the delta.
                if ((from targetMember in targetMembers where tweenMember.Name == targetMember.Name select TypeOfMember(targetMember)).Any(targetMemberType => tweenMemberType == targetMemberType))
                    _crossover.Add(tweenMember.Name, (GetFloatValueFromObjectMember(_objectTween, tweenMember.Name) - GetFloatValueFromObjectMember(_objectTarget, tweenMember.Name)) * 1);
            }

            // Get the type/method combination function.
            _func = (Func<float, float>) GetType().GetMethod(type.ToString(), BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(this, new object[] {method});
        }

        #region Inheritance

        internal override void TimerLogic(float timePassed)
        {
            _timePassed += timePassed;

            // Get interpolation.
            float s = _func(Math.Min(1, _timePassed / _duration));
            float ds = s - _prevS;
            _prevS = s;

            // Apply interpolation to all cross members.
            foreach (KeyValuePair<string, float> crossMember in _crossover)
            {
                SetFloatValueOnObjectMember(_objectTarget, crossMember.Key, GetFloatValueFromObjectMember(_objectTarget, crossMember.Key) + crossMember.Value * ds);
            }

            // Check if over.
            if (!(_timePassed >= _duration)) return;
            // Invoke ending logic.
            _after?.Invoke();
            Kill();
        }

        protected override void End()
        {
            _objectTarget = null;
            _objectTween = null;
            _after = null;
        }

        #endregion

        #region Reflection Helpers

        /// <summary>
        /// Returns the type of a member field or property.
        /// </summary>
        /// <param name="memberInfo">The member info of the member whose type to return.</param>
        /// <returns>The type of the member.</returns>
        private static Type TypeOfMember(MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                {
                    FieldInfo temp = (FieldInfo) memberInfo;
                    return temp.FieldType;
                }
                case MemberTypes.Property:
                {
                    PropertyInfo temp = (PropertyInfo) memberInfo;
                    return temp.PropertyType;
                }
                default:
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the value of an object as a float.
        /// </summary>
        /// <param name="Object">The object's member to get.</param>
        /// <param name="name">The name of the member.</param>
        /// <returns>An object member's value as a float.</returns>
        private static float GetFloatValueFromObjectMember(object Object, string name)
        {
            MemberInfo memberInfo = Object.GetType().GetMember(name)[0];

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                {
                    return (float) Convert.ToDouble(Object.GetType().GetField(memberInfo.Name).GetValue(Object));
                }
                case MemberTypes.Property:
                {
                    PropertyInfo property = Object.GetType().GetProperty(memberInfo.Name);
                    if (property == null) throw new Exception("Missing property on target.");
                    return (float) Convert.ToDouble(property.GetValue(Object));
                }
                default:
                {
                    throw new Exception("Invalid member type.");
                }
            }
        }

        /// <summary>
        /// Sets the value of an object member to a float.
        /// </summary>
        /// <param name="Object">The object's member to set.</param>
        /// <param name="name">The name of the member.</param>
        /// <param name="value">The value to set.</param>
        private static void SetFloatValueOnObjectMember(object Object, string name, float value)
        {
            MemberInfo memberInfo = Object.GetType().GetMember(name)[0];

            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                {
                    FieldInfo field = Object.GetType().GetField(memberInfo.Name);
                    field.SetValue(Object, Convert.ChangeType(value, field.FieldType));
                    break;
                }
                case MemberTypes.Property:
                {
                    PropertyInfo property = Object.GetType().GetProperty(memberInfo.Name);
                    if (property == null) throw new Exception("Missing property on target.");
                    property.SetValue(Object, Convert.ChangeType(value, property.PropertyType));
                    break;
                }
                default:
                {
                    throw new Exception("Invalid member type.");
                }
            }
        }

        #endregion

        #region Types

        private Func<float, float> In(TweenMethod method)
        {
            // Call the method function straight.
            return (Func<float, float>) Delegate.CreateDelegate(typeof(Func<float, float>), this,
                GetType().GetMethod(method.ToString(), BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception($"Tween method [{method}] was not found."));
        }

        private Func<float, float> Out(TweenMethod method)
        {
            // Reverse input and output.
            return s =>
            {
                Func<float, float> methodFunc = In(method);

                return 1f - methodFunc(1 - s);
            };
        }

        private Func<float, float> InOut(TweenMethod method)
        {
            return s =>
            {
                Func<float, float> methodFuncIn = In(method);
                Func<float, float> methodFuncOut = Out(method);

                if (s < 0.5f)
                    return methodFuncIn(2 * s) * 0.5f;
                return 1 + methodFuncOut(2 * s - 1) * 0.5f;
            };
        }

        private Func<float, float> OutIn(TweenMethod method)
        {
            return s =>
            {
                Func<float, float> methodFuncIn = In(method);
                Func<float, float> methodFuncOut = Out(method);

                if (s < 0.5f)
                    return 1 + methodFuncOut(2 * s - 1) * 0.5f;
                return methodFuncIn(2 * s) * 0.5f;
            };
        }

        #endregion

        #region Tweening

        private float Linear(float s)
        {
            return s;
        }

        private float Quad(float s)
        {
            return s * s;
        }

        private float Cubic(float s)
        {
            return s * s * s;
        }

        private float Quart(float s)
        {
            return s * s * s * s;
        }

        private float Quint(float s)
        {
            return s * s * s * s * s;
        }

        private float Sine(float s)
        {
            return (float) Math.Sin(s);
        }

        private float Expo(float s)
        {
            return (float) Math.Pow(2, 10 * (s - 1));
        }

        private float Circ(float s)
        {
            return (float) (1f - Math.Sqrt(1 - s * s));
        }

        private float Back(float s)
        {
            return s * s * ((Bounciness + 1) * s - Bounciness);
        }

        private float Bounce(float s)
        {
            float a = 7.5625f;
            float b = 1 / 2.75f;

            float res1 = (float) Math.Min(a * Math.Pow(s, 2), a * Math.Pow(s - 1.5 * b, 2) + 0.75);
            float res2 = (float) Math.Min(a * Math.Pow(s - 2.25 * b, 2) + 0.9375, a * Math.Pow(s - 2.625 * b, 2));

            return Math.Min(res1, res2) + 0.984375f;
        }

        #endregion
    }

    public enum TweenType
    {
        In,
        Out,
        InOut,
        OutIn
    }

    public enum TweenMethod
    {
        Linear,
        Quad,
        Cubic,
        Quart,
        Quint,
        Sine,
        Expo,
        Circ,
        Back,
        Bounce
    }
}
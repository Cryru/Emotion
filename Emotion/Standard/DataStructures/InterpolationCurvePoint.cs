#nullable enable

namespace Emotion.Standard.DataStructures;


[DontSerialize] // Inheriting List breaks Reflector currently
public class InterpolationCurve<TValue> : List<InterpolationCurve<TValue>.InterpolationCurvePoint>
    where TValue : unmanaged
{
    public struct InterpolationCurvePoint(float time, TValue value)
    {
        public float Time = time;
        public TValue Value = value;
    }

    private class TimeComparer : IComparer<InterpolationCurve<TValue>.InterpolationCurvePoint>
    {
        public int Compare(InterpolationCurve<TValue>.InterpolationCurvePoint x, InterpolationCurve<TValue>.InterpolationCurvePoint y)
            => x.Time.CompareTo(y.Time);
    }
    private static readonly TimeComparer _timeComparer = new();

    private Func<TValue, TValue, float, TValue> _lerpFunc;

    public InterpolationCurve(Func<TValue, TValue, float, TValue> lerp)
    {
        _lerpFunc = lerp;
    }

    public TValue GetValueAtTime(float time)
    {
        if (Count == 0)
            return default;
        if (Count == 1)
            return this[0].Value;
        if (time <= this[0].Time)
            return this[0].Value;
        if (time >= this[^1].Time)
            return this[^1].Value;

        int found = BinarySearch(new InterpolationCurve<TValue>.InterpolationCurvePoint(time, default), _timeComparer);
        if (found >= 0)
        {
            // Exact match
            return this[found].Value;
        }

        int nextIndex = ~found;
        int prevIndex = nextIndex - 1;
        var prev = this[prevIndex];
        var next = this[nextIndex];

        float t = (time - prev.Time) / (next.Time - prev.Time);
        return _lerpFunc(prev.Value, next.Value, t);
    }
}

public class InterpolationCurveFloat : InterpolationCurve<float>
{
    public InterpolationCurveFloat() : base(Maths.Lerp)
    {
    }
}

public class InterpolationCurveVec2 : InterpolationCurve<Vector2>
{
    public InterpolationCurveVec2() : base(Vector2.Lerp)
    {
    }
}

public class InterpolationCurveVec3 : InterpolationCurve<Vector3>
{
    public InterpolationCurveVec3() : base(Vector3.Lerp)
    {
    }
}

public class InterpolationCurveQuaternion : InterpolationCurve<Quaternion>
{
    public InterpolationCurveQuaternion() : base(Quaternion.Lerp)
    {
    }
}

public class InterpolationCurveColor : InterpolationCurve<Color>
{
    public InterpolationCurveColor() : base(Color.Lerp)
    {
    }
}
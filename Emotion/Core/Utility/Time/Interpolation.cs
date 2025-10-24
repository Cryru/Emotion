#nullable enable

namespace Emotion.Core.Utility.Time;

public static class Interpolation
{
    // Delta is in seconds (so ms should be delta / 1000f)

    public static float SmoothLerp(float current, float target, int speed, float deltaMs)
    {
        float delta = deltaMs / 1000f;
        return target + (current - target) * MathF.Exp(-speed * delta);
    }

    public static float SmoothLerpAngle(float current, float target, int speed, float delta)
    {
        float num = Maths.Repeat(current - target, 360f);
        if (num > 180f)
            num -= 360f;

        return target + num * MathF.Exp(-speed * delta);
    }

    public static Vector2 SmoothLerp(Vector2 current, Vector2 target, int speed, float deltaMs)
    {
        float delta = deltaMs / 1000f;
        return target + (current - target) * MathF.Exp(-speed * delta);
    }

    public static Vector3 SmoothLerp(Vector3 current, Vector3 target, int speed, float deltaMs)
    {
        float delta = deltaMs / 1000f;
        return target + (current - target) * MathF.Exp(-speed * delta);
    }
}

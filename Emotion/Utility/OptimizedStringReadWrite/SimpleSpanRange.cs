namespace Emotion.Utility.OptimizedStringReadWrite;

public struct SimpleSpanRange
{
    public static SimpleSpanRange Invalid = new SimpleSpanRange(0, 0);

    public int Start;
    public int Length;
    public bool IsInvalid => Length == 0;

    public SimpleSpanRange(int start, int length)
    {
        Start = start;
        Length = length;
    }
}

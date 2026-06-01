namespace Emotion.Testing;

public sealed class EmotionTestResult
{
    public EmotionTestDescription Description { get; init; }
    public bool Passed { get; set; }
    public DateTimeOffset StartTime { get; init; }

    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public DateTimeOffset EndTime { get; set; }

    public TimeSpan Duration => EndTime - StartTime;

    public EmotionTestResult(EmotionTestDescription description)
    {
        Description = description;
        StartTime = DateTimeOffset.UtcNow;
    }

    public void SetFailed(string errorMessage, Exception? ex = null)
    {
        Passed = false;
        ErrorMessage = errorMessage;
        Exception = ex;
        EndTime = DateTimeOffset.UtcNow;
    }

    public void SetSuccess()
    {
        Passed = true;
        EndTime = DateTimeOffset.UtcNow;
    }
}
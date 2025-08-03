namespace Emotion.Platform.Implementation.Android;

public class AndroidLogger : LoggingProvider
{
    public override void Log(MessageType type, string source, string message)
    {
        global::Android.Util.Log.Info(source, message);
    }

    public override void Dispose()
    {
    }
}
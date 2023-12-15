#region Using

using Emotion.Standard.Logging;

#endregion

namespace Emotion.Droid
{
    public class AndroidLogger : LoggingProvider
    {
        public override void Log(MessageType type, string source, string message)
        {
            Android.Util.Log.Info(source, message);
        }

        public override void Dispose()
        {
        }
    }
}
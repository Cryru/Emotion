#region Using

#endregion

namespace Emotion.Common
{
    public interface IPlugin : IDisposable
    {
        void Initialize();
    }
}
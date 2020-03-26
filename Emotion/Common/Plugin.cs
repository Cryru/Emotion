using System;

namespace Emotion.Common
{
    public interface IPlugin : IDisposable
    {
        void Initialize();
        void Update();
    }
}
using System;

namespace Emotion.Platform
{
    public interface IPlatform : IDisposable
    {
        void Initialize();
    }
}
#region Using

using System;

#endregion

namespace Emotion.Common
{
    public interface IPlugin : IDisposable
    {
        void Initialize();
    }
}
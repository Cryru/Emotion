#region Using

using System;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Input;
using Adfectus.IO;
using Adfectus.Sound;

#endregion

namespace Adfectus.Implementation
{
    public interface IPlatform : IDisposable
    {
        void Initialize();
        AssetLoader CreateAssetLoader(EngineBuilder builder);
        IHost CreateHost(EngineBuilder builder);
        IInputManager CreateInputManager(EngineBuilder builder);
        GraphicsManager CreateGraphicsManager(EngineBuilder builder);
        SoundManager CreateSoundManager(EngineBuilder builder);
    }
}
using Adfectus.Common;
using Adfectus.Common.Hosting;
using Adfectus.Graphics;
using Adfectus.Implementation;
using Adfectus.Implementation.GLFW;
using Adfectus.Input;
using Adfectus.IO;
using Adfectus.Sound;
using Emotion.Platform.DesktopGL.Assets;
using Emotion.Platform.DesktopGL.Native;
using Emotion.Platform.DesktopGL.Sound;

namespace Emotion.Platform.DesktopGL
{
    public sealed class DesktopPlatform : IPlatform
    {
        private AssetLoaderDesktop _assetLoader { get; set; }
        private GlfwHost _host { get; set; }
        private GlfwInputManager _inputManager { get; set; }
        private GlfwGraphicsManager _graphicsManager { get; set; }
        private ALSoundManager _soundManager { get; set; }

        public void Initialize()
        {
            NativeLoader.Setup();
        }

        public AssetLoader CreateAssetLoader(EngineBuilder builder)
        {
            return _assetLoader ?? (_assetLoader = new AssetLoaderDesktop(builder));
        }

        public IHost CreateHost(EngineBuilder builder)
        {
            return _host ?? (_host = new GlfwHost(builder));
        }

        public IInputManager CreateInputManager(EngineBuilder builder)
        {
            return _inputManager ?? (_inputManager = new GlfwInputManager(_host._win));
        }

        public GraphicsManager CreateGraphicsManager(EngineBuilder builder)
        {
            return _graphicsManager ?? (_graphicsManager = new GlfwGraphicsManager());
        }

        public SoundManager CreateSoundManager(EngineBuilder builder)
        {
            return _soundManager ?? (_soundManager = new ALSoundManager(builder));
        }

        public void Dispose()
        {
        }
    }
}
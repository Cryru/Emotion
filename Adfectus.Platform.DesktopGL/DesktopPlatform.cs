#region Using

using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Implementation;
using Adfectus.Input;
using Adfectus.IO;
using Adfectus.Platform.DesktopGL.Assets;
using Adfectus.Platform.DesktopGL.Native;
using Adfectus.Platform.DesktopGL.Sound;
using Adfectus.Sound;

#endregion

namespace Adfectus.Platform.DesktopGL
{
    public sealed class DesktopPlatform : IPlatform
    {
        private DesktopAssetLoader _desktopAssetLoader { get; set; }
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
            return _desktopAssetLoader ??= new DesktopAssetLoader(builder);
        }

        public IHost CreateHost(EngineBuilder builder)
        {
            return _host ??= new GlfwHost(builder);
        }

        public IInputManager CreateInputManager(EngineBuilder builder)
        {
            return _inputManager ??= new GlfwInputManager(_host._win);
        }

        public GraphicsManager CreateGraphicsManager(EngineBuilder builder)
        {
            return _graphicsManager ??= new GlfwGraphicsManager();
        }

        public SoundManager CreateSoundManager(EngineBuilder builder)
        {
            return _soundManager ??= new ALSoundManager(builder);
        }

        public void Dispose()
        {
        }
    }
}
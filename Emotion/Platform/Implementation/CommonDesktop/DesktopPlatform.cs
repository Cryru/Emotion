#region Using

using Emotion.Common;

#endregion

namespace Emotion.Platform.Implementation.CommonDesktop
{
    public abstract class DesktopPlatform : PlatformBase
    {
        internal override void Setup(Configurator config)
        {
            base.Setup(config);

            if (Engine.AssetLoader == null) return;
            Engine.AssetLoader.AddSource(new FileAssetSource("Assets"));
            Engine.AssetLoader.AddStore(new FileAssetStore("Player"));
        }
    }
}
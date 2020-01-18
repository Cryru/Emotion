# Assets and Content

To draw textures, play sounds, or interact with content in any way you need to load the files as assets first. This is done through the [AssetLoader]([CodeRoot]IO/AssetLoader) module accessible at `Engine.AssetLoader`. When the module is initialized asset "sources" are loaded into it, and additional sources can be added later. These sources describe the existing assets and handle loading them.

The default sources include the "Assets" folder and any assets embedded into the Emotion assembly, calling assembly, and executing assembly.

All clases relating to assets are found under the [Emotion.IO]([CodeRoot]IO) namespace.

## Types of Assets

Any class which inherits the `Asset` class can be managed by the Assetloader. By default these include:

- TextureAsset

    Drawable images. Uses all formats supported by Emotion.Standard.Image (BMP/PNG)
- ShaderAsset

    A shader description file. For more information refer to the shader documentation here - [Shaders].
- FontAsset

    Fonts to draw text. Uses all formats supported by Emotion.Standard.Text
- SoundAsset

    Sounds to play using the SoundManager. For more information refer to the audio documentation here - [Audio].
- TextAsset

    When loading plain text files. You can access the contents using the `MyTextAsset.Content` property.
- OtherAsset

    Load the asset as raw bytes. You can access the contents using the `MyOtherAsset.Content` property.

## Loading and Unloading Assets

To load an asset use the `Engine.AssetLoader.Get<AssetType>(Path)` function. In place of `AssetType` use the name of the class you want to load the file using, and in place of `Path` use a path relative to a loaded source. Paths are case insensitive, and the direction of the slashes doesn't matter. If the file is not found the function will return null.

The function will return a loaded asset of the specified class which you are free to use. Once the asset is loaded, subsequent calls to `AssetLoader.Get` will return a cached version of it instead of loading it again, meaning you don't have to assign and manage them, but can rather call `Get` every time.

To clear the asset from the cache (for example to get a fresh copy), or to free memory use the `Engine.AssetLoader.Destroy(Path)` function.

Additionally you can check the existance of an asset using the `Engine.AssetLoader.Exists(Path)` function.

Note: Asset loading should be async as most of the time they are loaded from other threads, for instance when loading scenes. To access the draw thread (for example for uploading textures) use `GLThread.ExecuteGLThread()`. You can refer to how this is done in [TextureAsset]([CodeRoot]IO/TextureAsset)

## Sources

The AssetLoader uses the `AssetSource` class and its derivatives (`FileAssetSource`, `EmbeddedAssetSource`) to load assets. This allows you to provide files from anywhere - be it embedded in the assembly, the internet, or the file system. When a source is added it generates a "manifest" of all known assets under it, which can later be requested. This means that all assets a source can load have to be known when it is initialized.

If two sources have the same asset path in their manifests then the source which was added first will be used to load the asset.

To add embedded assets to be used by `EmbeddedAssetSource`s, add the files to the project and select `EmbeddedResource` under the build action. This allows you to have a separate dll for assets - which you shouldn't abuse as the whole thing is loaded into memory at once.
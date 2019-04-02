# AssetLoader (Adfectus.IO.AssetLoader)

_Last Updated: Version 0.0.11_

The asset loader is an Adfectus context module accessible globally through `Engine.AssetLoader`. It provides you with the ability load and unload files. Most of its classes can be found under the `Adfectus.IO` namespace.

## Types of Assets

Any class which inherits the `Asset` class can be managed by the Assetloader. By default these include:

- Texture

    Images to draw, loaded by FreeImage ex. BMP/PNG/JPEG/GIF
- ShaderAsset

    An Emotion xml shader description file. For more information refer to the shader documentation here - [Shaders](./Documents/Shaders).
- Font

    Fonts to draw text. Loaded by FreeType ex. TTF/OTF
- SoundFile

    Sounds to play using the SoundManager. For more information refer to the audio documentation here - [SoundManager](./Documents/SoundManager).
- TextFile

    When loading plain text files. You can access the contents using the `MyTextFile.Content` property.
- OtherAsset

    Load the asset as a byte array. You can access the contents using the `MyOtherAsset.Content` property.

## Loading and Unloading Assets

To load an asset use the `Engine.AssetLoader.Get<AssetType>(Path)` function. In place of `AssetType` use the name of the class you want to load the file using, and in place of `Path` use a path relative to the `Assets` folder - configurable using the `Engine.Flags.AssetRootDirectory` property. Paths are case insensitive, and the direction of the slashes doesn't matter. If the file is not found the function will return null.

The function will then return a loaded asset of the specified class which you are free to use. Once the asset is loaded, subsequent calls to `AssetLoader.Get` will return a cached version of it instead of loading it again, meaning you don't have to assign and manage them, but can rather call `Get` every time.

To clear the asset from the cache (for example for getting a fresh copy), or to free memory use the `Engine.AssetLoader.Destroy(Path)` function.

Additionally you can check the existance of an asset using the `Engine.AssetLoader.Exists(Path)` function.

## IO, Filesystem, and Embedding

The AssetLoader uses the `AssetSource` class and its derivatives (`FileAssetSource`, `EmbeddedAssetSource`) to load assets. This allows you to provide files from anywhere - be it embedded in the assembly, the internet, or the actual file system. When a source is added it generates a "manifest" of all known assets under it, which can later be requested. This means that all assets a source can load have to be known when it is initialized.

By default one `FileAssetSource` is loaded a `EmbeddedAssetSource` for the calling, entry, and calling assemblies. You can configure the default FileAssetSource's folder and add additional embedded source assemblies using the EngineBuilder's SetupAssets function. Additionally you can load a source at runtime by using the `Engine.AssetLoader.AddSource` function.

If two sources have the same asset path in their manifests then the source which was added first will be used to load the asset.

To add embedded assets to be used by `EmbeddedAssetSource`s, add the files to the project and select `EmbeddedResource` under the build action. This allows you to have a separate dll for assets - which you shouldn't abuse as the whole thing is loaded into memory at once.

## References

- [Asset Tests](https://github.com/Cryru/Emotion/blob/master/Adfectus.Tests/Assets.cs)
- [Adfectus.IO Namespace](https://github.com/Cryru/Emotion/tree/master/Adfectus/IO)
- [AssetLoader.cs](https://github.com/Cryru/Emotion/blob/master/Adfectus/IO/AssetLoader.cs)
- [Asset.cs](https://github.com/Cryru/Emotion/blob/master/Adfectus/IO/Asset.cs)

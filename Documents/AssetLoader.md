# AssetLoader (Emotion.IO.AssetLoader)

_Last Updated: Build 285_

The asset loader is an Emotion context module accessible globally through `Context.AssetLoader`. It provides you with the ability load and unload files. Most of its classes can be found under the `Emotion.IO` namespace.

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

To load an asset use the `Context.AssetLoader.Get<AssetType>(Path)` function. In place of `AssetType` use the name of the class you want to load the file using, and in place of `Path` use a path relative to the `Assets` folder - configurable using the `Context.Flags.AssetRootDirectory` property. Paths are case insensitive, and the direction of the slashes doesn't matter. If the file is not found the function will return null.

The function will then return a loaded asset of the specified class which you are free to use. Once the asset is loaded, subsequent calls to `AssetLoader.Get` will return a cached version of it instead of loading it again, meaning you don't have to assign and manage them, but can rather call `Get` every time.

To clear the asset from the cache (for example for getting a fresh copy), or to free memory use the `Context.AssetLoader.Destroy(Path)` function.

Additionally you can check the existance of an asset using the `Context.AssetLoader.Exists(Path)` function.

## IO, Filesystem, and Embedding

The AssetLoader uses the `AssetSource` class and its derivatives (`FileAssetSource`, `EmbeddedAssetSource`) to load assets from the file system. This allows you to provide files from anywhere - be it embedded in the assembly, the internet, or the actual file system. However, currently there is no way to add custom sources to the main AssetLoader. By default it is loaded with one `FileAssetSource` set to the `Context.Flags.AssetRootDirectory` property, a `EmbeddedAssetSource` for the calling, entry, and calling assemblies and a `EmbeddedAssetSource` for each assembly in `Context.Flags.AdditionalAssetAssemblies`. This allows you to have a separate dll for assets - which you shouldn't abuse as the whole thing is loaded into memory at once.

To add embedded assets, add the files to the project and select `EmbeddedResource` under the build action. If the same file is present in multiple sources it will be loaded from the source that was loaded first.

## References

- [Asset Tests](https://github.com/Cryru/Emotion/blob/master/Emotion.Tests/src/Tests/Assets.cs)
- [Emotion.IO Namespace](https://github.com/Cryru/Emotion/tree/master/EmotionCore/src/IO)
- [AssetLoader.cs](https://github.com/Cryru/Emotion/blob/master/EmotionCore/src/IO/AssetLoader.cs)
- [Asset.cs](https://github.com/Cryru/Emotion/blob/master/EmotionCore/src/IO/Asset.cs)

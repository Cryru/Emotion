# Emotion
<img src="EmotionLogo.png" width="128px" />

![CI-Windows](https://github.com/Cryru/Emotion/actions/workflows/buildWindows.yml/badge.svg?branch=master)

## What is it?

Emotion is a cross-platform game engine written in C# with minimal dependencies, for me to make games in. The engine is more targeted towards programmers and is alternative to stuff like MonoGame, FNA, and Love rather than Unity.

## Games

### [Electric Sleep](https://store.steampowered.com/app/1011620/Electric_Sleep/)

A visual novel for Windows, Linux, and x64 Mac.

The version this game was built on is kept in the "adfectus-backup" branch. 

### [Escape the Arcana](https://store.steampowered.com/app/2953200/Escape_the_Arcana/)

A roguelike deckbuilder for Windows and Linux, with an Android version built but not published.

The version this game was built on is kept in the "EtA" branch.

## Target Hardware:

- OpenGL 3.0 or higher supported hardware
  - Or DirectX 11 if ANGLE is enabled
  - Or a multi-core CPU if the Mesa software renderer is enabled
  - WebGL 2.0 on Web
- Be able to run the Net 9 runtime.
	- If older than Windows 10 you'll need the [C++ Redistributable 2015 Update 3](https://www.microsoft.com/en-us/download/details.aspx?id=52685)
	- For Windows 7 you'll also need a specific security update [KB3063858](https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60#dependencies)
- A supported platform:
	- Windows 32 and 64 bit
 	- Linux 64 bit (Steam Deck works too)
	- Android API 23+
 	- Or implement your own via PlatformBase :)

That's it.

## Developing and Building

It is recommended you develop with a cloned version of the Emotion repo linking 'Emotion.csproj' and 'Emotion.SourceGeneration' in your solution. This way you have the most control over your code.

## Credits and Dependencies

By default Emotion has no external dependencies, any third party code is copy-pasted and statically compiled inside the project.
Note: All third-party code used is also modified in some way, so updating it to newer versions isnt very straight forward.

Include the "LICENSE THIRD-PARTY" file from the repository in your exe when distributing.

### Dependencies

| Library | License | Used For | Inclusion |
| -- | -- | -- | -- |
| .Net Core | MIT | Runtime | Nuget
| System.Numerics | MIT | Data structures and hardware intrinsics | Nuget
| [xxhash (pure C# implementation)](https://github.com/uranium62/xxHash) | MIT | Hashing data | ThirdParty/xxhash
| [Icaria Noise](https://github.com/krubbles/Icaria-Noise) | MIT | Perlin Noise | ThirdParty/IcariaNoise
| [Fast Noise Lite](https://github.com/Auburn/FastNoiseLite) | MIT | Simplex Noise | ThirdParty/FastNoiseLite
| [WinApi](https://github.com/prasannavl/WinApi) | Apache | Windows API Interop Headers | @ Emotion.Core.Platform.Implementation.Win32.Native
| [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net) | MIT | OpenGL API | ThirdParty/OpenGL
| [StbTrueType](https://github.com/nothings/stb/blob/master/stb_truetype.h) & [StbTrueTypeSharp](https://github.com/zwcloud/StbTruetypeSharp) | MIT & GPL3 | Text Rasterization | Slighly optimized and uses an arena allocator @ ThirdParty/StbTrueType
| [TiledSharp](https://github.com/marshallward/TiledSharp) | Apache 2.0 | .TMX Support | Uses custom XML and engine integration @ Emotion.Standard.Parsers.TMX

### Optional Dependencies

| Library | License | Used For | Inclusion |
| -- | -- | -- | -- |
| [Roslyn/Microsoft.CodeAnalysis.CSharp](https://github.com/dotnet/roslyn) | MIT | Runtime C# Script Compilation | Emotion.Plugins.CSharpScripting
| [ANGLE](https://github.com/google/angle) | Google License | Compatibility | Precompiled for Win32 and Win64, Add "ANGLE" symbol
| [llvmpipe / Gallium / Mesa](https://mesa3d.org/) | MIT | Compatibility via Software Renderer | Precompiled for Win32 and Win64
| [Glfw](https://github.com/glfw/glfw) & [Glfw.Net](https://github.com/Chman/Glfw.Net) | Zlib | Mac and Linux Window Creation | Precompiled for Mac64, Linux64, Win32, and Win64, Add "GLFW" symbol
| [OpenAL-Soft](https://github.com/kcat/openal-soft) & [OpenAL.NetCore](https://github.com/nsglover/OpenAL.NETCore) | LGPL & MIT | Mac and Linux Audio | Precompiled for Mac64, Linux64, Win64, Add "OpenAL" symbol
| [Assimp](https://github.com/assimp/assimp) & [AssimpNet](https://github.com/assimp/assimp-net) | Modified BSD | Reading FBX files | The Emotion build tool
| [Assimp (Engine)](https://github.com/assimp/assimp) & [Silk.NET.Assimp](https://github.com/dotnet/Silk.NET/pkgs/nuget/Silk.NET.Assimp) | Modified BSD & MIT | 3D Model preview and conversion | Add "ASSIMP" symbol
| [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) | Six Labors Split License | Developer loading and conversion of unsupported image types such as "jpg" | Add "MORE_IMAGE_TYPES" symbol

### Used as References and Research

| Library | License | Used For | Inclusion |
| -- | -- | -- | -- |
| [McGill Engineering](http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/Samples.html) | X | Hardening WAV and Audio Implementation | None
| [PNGSuite](http://www.schaik.com/pngsuite/) | X | Hardening PNG Implementation | None
| [OpenType.JS](https://opentype.js.org/) | X | Font Parsing Reference | None
| [Nine.Imagine](https://github.com/yufeih/Nine.Imaging) | X | Image Parsing Comparison | None
| [ImageSharp](https://github.com/SixLabors/ImageSharp) | X | Quirky Image Format Reference | None
| [OpenAL-Soft](https://github.com/kcat/openal-soft/) | X | Audio Code Reference | None
| [NAudio](https://github.com/naudio/NAudio) | X | Audio Code Reference | None
| [Audacity](https://github.com/audacity) | X | Audio Code Reference | None

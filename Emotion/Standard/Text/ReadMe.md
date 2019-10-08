# Emotion.Standard.Text

This part of the library deals with parsing fonts and rendering font atlases.

The Emotion font parser was developed by using https://opentype.js.org/ and https://github.com/nothings/stb/blob/master/stb_truetype.h as a reference.
Some comments and code is directly taken from there.

The font allows the user to pick their own rasterizer. Possible options are:

- Emotion
	The default renderer.
	Based on the Stb renderer, but has some differences.
	If everything is well it should produce at least the same results as the Stb renderer.
	Fastest
- StbTrueType
	A more mature rasterizer to be used if the Emotion rasterizer produces bugs/unwanted results.
	Is sort of a fallback.
	Fast
- FreeType
	The most mature and advanced renderer, but it isn't portable as it is a native library.
	The native libraries for MacOS64, Windows64 and Linux64 are included, but this isn't really recommended. It will net you the best looking results, but you can't scale at runtime.
	
	Additionally this rasterizer requires the font to have been parsed by FreeType as well, which is the second parameter in the constructor (on by default)
	as the information parsed from Emotion is not transferable.
	
	The rasterizer isn't slow itself, but FreeType is not thread safe and due to how everything is implemented this is the slowest option.

	FreeType support is thanks to https://github.com/Robmaister/SharpFont and requires Emotion to have been compiled with the "FreeType" constant.
	Additionally the interopability plugin can be found in the "ZZZ_Plugins" folder.
# Emotion.Platform.SDL2

An emotion platform based on SDL2.

## Renderer

The SDL2 renderer is an abstraction over a SDL2 renderer object.

### Scaling and Rendering

The Emotion engine defines two sizes. A virtual width and height, called the RenderWidth and RenderHeight within the settings, and a window width and height. When rendering to a resolution with a different aspect ratio the view will be either letterboxed or pillarboxed depending on the difference. The virtual resolution tries to fit the window resolution, which means that if you are rendering at 1920x1080 and your window has the size of 960x540 everything will be downscaled twice. 

To achieve this effect I've used ``SDL_RenderSetLogicalSize``, which does the letter and pillarboxing automatically, because access to matrices within SDL is a whole other beast. However it does have its problems. For instance when rendering to a logical size bigger than the window the scaling produces artifacts, while the other way around produces none.

### Text Rendering

Usually when rendering text the preferred way is to render each glyph individually to a texture and then cache and reuse it. SDL2_TTF exposes a couple text rendering functions - ``TTF_RenderText`` which are somewhat performing. They return a surface with the text rendered on it, presumably on the CPU. I have opted in using this function for now as it does what I need, although doing things right is something to look into.

### Text Rendering At Top Left Origin

Glyph metrics are defined by an imaginary line on which the letters lay, however when rendering most of the times a top-left origin is used. Glyph metrics are defined as observed here;

<img src="https://www.freetype.org/freetype2/docs/glyphs/metrics.png" width="50%" height="50%" />

To position each glyph you will need to do the following transformations to your x and y offsets:

- Add minX to your x offset.
- Subtract your maxY from your font ascent and add the result to your y offset.
- Render your glyph.
- Add the horizontal advance to your x offset.
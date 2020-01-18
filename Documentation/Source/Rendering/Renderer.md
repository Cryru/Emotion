# Renderer

The renderer is an engine module accessed through "Engine.Renderer". It handles rendering and general graphics.

- [Renderer.cs](Emotion/Graphics/Renderer.cs)

## Frame

The frame lifecycle must all execute on the graphics thread.

At the start of the frame the "StartFrame" function is called and a "RenderComposer" object is obtained. This object is then passed
to user code where it is filled with the render commands to execute. For more information refer to the "Composing" documentation.

At the end of the frame the "EndFrame" function is called and executing of the render composer commands begins. Afterwards it is expected for the
platform to swap buffers and display whatever was rendered.

The lifecycle of the frame is automatically perfomed by the "RunFrame" part of the game loop. No function calls or modifications are expected to be performed on the Renderer module directly.
By default the frame is rendered to an internal framebuffer whose size is defined by the configuration, and then copied on to the screen buffer. This, in addition to
letterboxing, pillarboxing, (optionally) integer scaling, and other techniques are done to ensure correct scaling of pixels to different resolutions.

## Flags

The module will detect various features of the graphics device which can help it improve performance. You can find out whether they are in use by checking the Renderer members.
Some of these flags are editable and represent runtime options.

- SoftwareRenderer (Uneditable)
    Whether the detected renderer is the Mesa3D software renderer.
    Experimental - enabled on Windows when WGL context creation fails.
- DSA (Uneditable) Disabled in compatibility mode.
    Direct State Access (https://www.khronos.org/opengl/wiki/Direct_State_Access) allows OpenGL objects to be accessed without being bound first.
    OpenGL 4.5+
- CircleDetail (Editable)
    The number of vertices to use when rendering circles. 30 by default. The larger the circle the more detail you'd need for it to look smooth.
- TextureArrayLimit (Uneditable)
    The maximum number of textures that can be bound at once.
    Currently hardcoded to 16, which is the limit on MacOS. In the future it can be dynamically detected.
- MaxIndices (Uneditable)
    The maximum number of indices within the default index buffers.
    Currently hardcoded to ushort.MaxValue as the default index buffers use ushorts.
    This changes how many sprites/vertices can be rendered in a single batch.

## VertexData Model

You can specify any struct, when creating a Vertex Array Object, to carry your information to the shader. By default though the "VertexData" structure is used
when drawing sprites and vertices.

- [VertexData.cs](Emotion/Graphics/Data/VertexData.cs)
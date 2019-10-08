# OpenGL Objects

In the Emotion.Graphics.Objects namespace you can find OOP representations of OpenGL objects. You generally don't
need to deal with them unless you know what you are doing.

Note: These objects are not thread safe and should only be used on the graphics thread.

- [OpenGL Objects Namespace]([CodeRoot]/Graphics/Objects)

## DataBuffer

Can hold an array of any type.
You can either upload the data directly, or "map" it by using "CreateMapper".

- [VertexBuffer (vbo)]([CodeRoot]/Graphics/Objects/VertexBuffer.cs)

A DataBuffer object meant for storing vertices. The format of the vertex data is described in the VertexArrayObject.

- [IndexBuffer (ibo)]([CodeRoot]/Graphics/Objects/IndexBuffer.cs)

A DataBuffer object meant for storing indices. There are two index buffers which are automatically created. One is
the "QuadIbo" which is meant for drawing quads using two triangles and is a repeating pattern of "012230". The other is the
"SequentialIbo" which just holds the numbers in a sequence one after another.

- [VertexArrayObject (vao)]([CodeRoot]/Graphics/Objects/VertexArrayObject.cs)

Defines the format of the data within the VertexBuffer. Automatically binds the associated vbo and optionally an associated ibo (the latter being a feature of Emotion rather than OpenGL).
Note: On some platforms the vbo fails to bind automatically when the vao is bound, this is handled.

- [FrameBuffer (fbo)]([CodeRoot]/Graphics/Objects/FrameBuffer.cs)

An object you can draw to, associated with a texture.
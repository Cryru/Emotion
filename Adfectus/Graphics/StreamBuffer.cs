#region Using

using System.Numerics;
using Adfectus.Primitives;

#endregion

namespace Adfectus.Graphics
{
    /// <inheritdoc />
    /// <summary>
    /// A buffer used for stream drawing.
    /// </summary>
    public abstract class StreamBuffer : IRenderable
    {
        #region Properties

        /// <summary>
        /// The size of an object in vertices.
        /// </summary>
        public uint ObjectSize { get; protected set; }

        /// <summary>
        /// The size of the buffer in vertices.
        /// </summary>
        public uint Size { get; protected set; }

        /// <summary>
        /// The number of indices per object.
        /// </summary>
        public uint IndicesPerObject { get; protected set; }

        /// <summary>
        /// The size of the buffer in objects.
        /// </summary>
        public uint SizeInObjects
        {
            get => Size / ObjectSize;
        }

        #endregion

        #region Map State

        /// <summary>
        /// Whether the buffer is currently mapping.
        /// </summary>
        public abstract bool Mapping { get; }

        /// <summary>
        /// The number of mapped vertices.
        /// </summary>
        public uint MappedVertices { get; protected set; }

        /// <summary>
        /// The number of currently mapped objects.
        /// </summary>
        public uint MappedObjects
        {
            get => MappedVertices / ObjectSize;
        }

        /// <summary>
        /// Whether anything is mapped in the buffer.
        /// </summary>
        public bool AnythingMapped
        {
            get => MappedVertices != 0;
        }

        #endregion

        #region Objects

        /// <summary>
        /// The vbo holding the buffer data.
        /// </summary>
        public uint Vbo { get; protected set; }

        /// <summary>
        /// The vao binding the buffer.
        /// </summary>
        public uint Vao { get; protected set; }

        /// <summary>
        /// The ibo holding the vbo indices.
        /// </summary>
        public uint Ibo { get; protected set; }

        #endregion

        #region Mapping

        /// <summary>
        /// Unsafely map a vertex, and move the index by one.
        /// </summary>
        /// <param name="color">The color of the vertex as a uint.</param>
        /// <param name="tid">The texture id of the vertex texture or -1 if none.</param>
        /// <param name="uv">The normalized uv of the texture.</param>
        /// <param name="vertex">The vertex to map.</param>
        public abstract void UnsafeMapVertex(uint color, float tid, Vector2 uv, Vector3 vertex);

        /// <summary>
        /// Increment the vertex pointer.
        /// </summary>
        /// <param name="amount">The amount to increment the pointer.</param>
        public abstract void UnsafeIncrementPointer(int amount);

        /// <summary>
        /// Move the vertex pointer to a specific index.
        /// </summary>
        /// <param name="index">The index to move the pointer to.</param>
        public abstract void UnsafeMovePointerToVertex(uint index);

        /// <summary>
        /// Return the vertex pointer offset.
        /// </summary>
        /// <returns>The offset of the pointer from the start of the buffer.</returns>
        public abstract uint GetVertexPointer();

        /// <summary>
        /// Set the mapped vertices to a specific amount.
        /// Used if filling the VBO externally.
        /// </summary>
        /// <param name="mapped">The number of mapped vertices.</param>
        public void UnsafeSetMappedVertices(uint mapped)
        {
            MappedVertices = mapped;
        }

        #endregion

        #region Friendly Mapping

        /// <summary>
        /// Map the next vertex.
        /// </summary>
        /// <param name="vertex">The vertex to map.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex.</param>
        /// <param name="uv">The uv of the texture.</param>
        public abstract void MapNextVertex(Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null);

        /// <summary>
        /// Map a vertex at a specific index.
        /// </summary>
        /// <param name="index">The index to map at.</param>
        /// <param name="vertex">The vertex to map.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex.</param>
        /// <param name="uv">The uv of the texture.</param>
        public abstract void MapVertexAt(uint index, Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null);

        /// <summary>
        /// Maps the current quad and advances the current index by one quad.
        /// </summary>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad.</param>
        /// <param name="textureArea">The texture area (UV) of the quad.</param>
        public abstract void MapNextQuad(Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null);

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        /// <param name="index">The index of the quad to map.</param>
        /// <param name="position">The position of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad.</param>
        /// <param name="textureArea">The texture area (UV) of the quad.</param>
        public abstract void MapQuadAt(uint index, Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null);

        /// <summary>
        /// Maps the current line and advances the pointer.
        /// </summary>
        /// <param name="pointOne">The position of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public abstract void MapNextLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1);

        /// <summary>
        /// Moves the pointer to the specified quad index and maps the quad.
        /// </summary>
        /// <param name="index">The index of the vertex to map.</param>
        /// <param name="pointOne">The position of the first point.</param>
        /// <param name="pointTwo">The size of the second point.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public abstract void MapLineAt(uint index, Vector3 pointOne, Vector3 pointTwo, Color color, int thickness = 1);

        #endregion

        /// <summary>
        /// Get the id a texture within the buffer, or -1 if it doesn't exist in it.
        /// </summary>
        /// <param name="texture">The id of a texture within the buffer.</param>
        /// <returns>The id of the texture within the buffer.</returns>
        public abstract int GetTid(Texture texture);

        /// <summary>
        /// Reset the buffer, and everything mapped.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Set the render range for the buffer in objects.
        /// </summary>
        /// <param name="startIndex">The index of the object to start drawing from. Inclusive.</param>
        /// <param name="endIndex">The index of the object to stop drawing at. If null will draw to MappedObjects. Exclusive</param>
        public abstract void SetRenderRange(uint startIndex = 0, uint? endIndex = null);

        /// <summary>
        /// Set the render range for the buffer in indices.
        /// </summary>
        /// <param name="startIndex">The indices offset to start drawing from. Inclusive.</param>
        /// <param name="endIndex">The indices to stop drawing at. If null will draw to MappedObjects. Exclusive.</param>
        public abstract void SetRenderRangeIndices(uint startIndex = 0, uint? endIndex = null);

        /// <summary>
        /// Set the base vertex, essentially a vertex number to be added to all indices.
        /// </summary>
        /// <param name="baseVertex"></param>
        public abstract void SetBaseVertex(uint baseVertex);

        /// <summary>
        /// Render the stream buffer.
        /// </summary>
        public abstract void Render();

        /// <summary>
        /// Delete the buffer freeing memory.
        /// </summary>
        public abstract void Delete();
    }
}
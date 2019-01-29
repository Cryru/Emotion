// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <inheritdoc />
    /// <summary>
    /// A buffer used for stream drawing.
    /// </summary>
    public interface IStreamBuffer : IRenderable
    {
        #region Properties

        /// <summary>
        /// The size of an object in vertices.
        /// </summary>
        int ObjectSize { get; }

        /// <summary>
        /// The size of the buffer in bytes.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// The size of the buffer in objects.
        /// </summary>
        int SizeInObjects { get; }

        /// <summary>
        /// The number of indices per object.
        /// </summary>
        int IndicesPerObject { get; }

        #endregion

        #region Map State

        /// <summary>
        /// Whether anything is mapped in the buffer.
        /// </summary>
        bool AnythingMapped { get; }

        /// <summary>
        /// The number of currently mapped objects.
        /// </summary>
        int MappedObjects { get; }

        /// <summary>
        /// The number of mapped vertices.
        /// </summary>
        int MappedVertices { get; }

        /// <summary>
        /// Whether the buffer is currently mapping.
        /// </summary>
        bool Mapping { get; }

        #endregion

        #region Object State

        /// <summary>
        /// The vbo holding the buffer data.
        /// </summary>
        int Vbo { get; set; }

        /// <summary>
        /// The vao binding the buffer.
        /// </summary>
        int Vao { get; set; }

        /// <summary>
        /// The ibo holding the vbo indices.
        /// </summary>
        int Ibo { get; set; }

        #endregion

        /// <summary>
        /// Delete the buffer freeing memory.
        /// </summary>
        void Delete();

        /// <summary>
        /// Get the id a texture within the buffer, or -1 if it doesn't exist in it.
        /// </summary>
        /// <param name="texture">The id of a texture within the buffer.</param>
        /// <returns>The id of the texture within the buffer.</returns>
        int GetTid(Texture texture);

        /// <summary>
        /// Map the next vertex.
        /// </summary>
        /// <param name="vertex">The vertex to map.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex.</param>
        /// <param name="uv">The uv of the texture.</param>
        void MapNextVertex(Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null);

        /// <summary>
        /// Map a vertex at a specific index.
        /// </summary>
        /// <param name="index">The index to map at.</param>
        /// <param name="vertex">The vertex to map.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex.</param>
        /// <param name="uv">The uv of the texture.</param>
        void MapVertexAt(int index, Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null);

        /// <summary>
        /// Unsafely map a vertex, and move the index by one.
        /// </summary>
        /// <param name="color">The color of the vertex as a uint.</param>
        /// <param name="tid">The texture id of the vertex texture or -1 if none.</param>
        /// <param name="uv">The normalized uv of the texture.</param>
        /// <param name="vertex">The vertex to map.</param>
        void UnsafeMapVertex(uint color, float tid, Vector2 uv, Vector3 vertex);

        /// <summary>
        /// Increment the vertex pointer.
        /// </summary>
        /// <param name="amount">The amount to increment the pointer.</param>
        void UnsafeIncrementPointer(int amount);

        /// <summary>
        /// Move the vertex pointer to a specific index.
        /// </summary>
        /// <param name="index">The index to move the pointer to.</param>
        void UnsafeMovePointerToVertex(int index);

        /// <summary>
        /// Return the vertex pointer offset.
        /// </summary>
        /// <returns>The offset of the pointer from the start of the buffer.</returns>
        int GetVertexPointer();

        /// <summary>
        /// Reset the buffer, and everything mapped.
        /// </summary>
        void Reset();

        /// <summary>
        /// Set the render range for the buffer in objects.
        /// </summary>
        /// <param name="startIndex">The index of the object to start drawing from.</param>
        /// <param name="endIndex">The index of the object to stop drawing at. If -1 will draw to MappedObjects.</param>
        void SetRenderRange(int startIndex = 0, int endIndex = -1);

        /// <summary>
        /// Set the render range for the buffer in vertices.
        /// </summary>
        /// <param name="startIndex">The index of the vertex to start drawing from.</param>
        /// <param name="endIndex">The index of the vertex to stop drawing at. If -1 will draw to MappedVertices.</param>
        void SetRenderRangeVertices(int startIndex = 0, int endIndex = -1);
    }
}
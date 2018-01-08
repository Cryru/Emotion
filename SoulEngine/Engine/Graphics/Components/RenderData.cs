// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using Breath.Graphics;
using Breath.Objects;
using OpenTK;
using Soul.Engine.ECS;

#endregion

namespace Soul.Engine.Graphics.Components
{
    public partial class RenderData : ComponentBase
    {
        /// <summary>
        /// The color of the drawable.
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set
            {
                HasUpdated = true;
                _color = value;
            }
        }

        private Color _color = Color.White;

        /// <summary>
        /// The drawing priority of the object. The higher - the more on top it will be drawn.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
            set
            {
                HasUpdated = true;
                _priority = value;
            }
        }

        private int _priority = 0;

        #region VBOs

        public VBO VerticesVBO = new VBO();
        public VBO ColorVBO = new VBO();

        #endregion

        #region Objects

        /// <summary>
        /// The model matrix.
        /// </summary>
        public Matrix4 ModelMatrix = Matrix4.Identity;

        internal Vector2[] _vertices = { };

        #endregion

        #region Vertices Functions

        /// <summary>
        /// Sets the render data to a vertices array.
        /// </summary>
        /// <param name="vertices">The array of vertices to set this render data to.</param>
        public void SetVertices(Vector2[] vertices)
        {
            SetPointCount(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                SetPoint(i, vertices[i]);
            }
        }

        /// <summary>
        /// Returns the number of points in the polygon.
        /// </summary>
        /// <returns>The number of points in the polygon.</returns>
        public int GetPointCount()
        {
            return _vertices.Length;
        }

        /// <summary>
        /// Sets the number of points in the polygon.
        /// </summary>
        /// <param name="size">The number of points the polygon should have.</param>
        public void SetPointCount(int size)
        {
            Array.Resize(ref _vertices, size);
        }

        /// <summary>
        /// Returns the point in the polygon with the specified index.
        /// </summary>
        /// <param name="index">The index of the point to return.</param>
        /// <returns>The point with the specified index.</returns>
        public Vector2 GetPoint(int index)
        {
            if (_vertices.Length - 1 < index || index < 0)
                return Vector2.Zero;

            return _vertices[index];
        }

        /// <summary>
        /// Sets a point within the polygon.
        /// </summary>
        /// <param name="index">The index of the point to change.</param>
        /// <param name="point">The point to change it with.</param>
        public void SetPoint(int index, Vector2 point)
        {
            if (_vertices.Length - 1 < index || index < 0)
                return;

            _vertices[index] = point;
            HasUpdated = true;
        }

        /// <summary>
        /// Uploads the vertices to the vbo.
        /// </summary>
        public void UpdateVertices()
        {
            if (_vertices.Length == 0) return;

            VerticesVBO.Upload(_vertices);
            UpdateColor();

            HasUpdated = false;
        }

        #endregion

        #region Color

        /// <summary>
        /// Updates the color array.
        /// </summary>
        private void UpdateColor()
        {
            ColorVBO.Upload(_color.ToVertexArray(_vertices.Length));
        }

        #endregion

    }
}
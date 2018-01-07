// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

#endregion

#region Using

using System;
using Breath.Objects;
using OpenTK;

#endregion

namespace Soul.Engine.Graphics.Components
{
    public partial class RenderData
    {
        #region Templates

        /// <summary>
        /// Transform the render data to a rectangle.
        /// </summary>
        public void ApplyTemplate_Rectangle()
        {
            // Dump old VBO.
            VerticesVBO?.Destroy();

            _vertices = _rectangleVertices;
            VerticesVBO = RectangleVBO;

            _hasUpdated = true;
        }

        /// <summary>
        /// Transform the render data to a triangle.
        /// </summary>
        public void ApplyTemplate_Triangle()
        {           
            // Dump old VBO.
            VerticesVBO.Destroy();

            _vertices = _triangleVertices;
            VerticesVBO = TriangleVBO;

            _hasUpdated = true;
        }

        /// <summary>
        /// Transform the render data to a circle.
        /// </summary>
        public void ApplyTemplate_Circle()
        {
            // Dump old VBO.
            VerticesVBO.Destroy();

            _vertices = _circleVertices;
            VerticesVBO = CircleVBO;

            _hasUpdated = true;
        }

        #endregion

        #region Rectangle

        private static VBO RectangleVBO
        {
            get
            {
                if (_rectangleVBOHolder == null)
                {
                    _rectangleVBOHolder = new VBO();
                    _rectangleVBOHolder.Upload(_rectangleVertices);
                }

                return _rectangleVBOHolder;
            }
        }

        private static VBO _rectangleVBOHolder;

        private static Vector2[] _rectangleVertices =
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        #endregion

        #region Triangle

        private static VBO TriangleVBO
        {
            get
            {
                if (_triangleVBOHolder == null)
                {
                    _triangleVBOHolder = new VBO();
                    _triangleVBOHolder.Upload(_triangleVertices);
                }

                return _triangleVBOHolder;
            }
        }

        private static VBO _triangleVBOHolder;

        private static Vector2[] _triangleVertices =
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 1)
        };

        #endregion

        #region Circle

        private static VBO CircleVBO
        {
            get
            {
                if (_circleVBOHolder == null)
                {
                    _circleVBOHolder = new VBO();
                    _circleVBOHolder.Upload(_circleVertices);
                }

                return _circleVBOHolder;
            }
        }

        private static VBO _circleVBOHolder;

        private static Vector2[] _circleVertices
        {
            get
            {
                if (_circleVerticesHolder == null)
                {
                    float radius = 0.5f;
                    int detail = 30;

                    Array.Resize(ref _circleVerticesHolder, detail);

                    // Generate points.
                    for (uint i = 0; i < detail; i++)
                    {
                        float angle = (float) (i * 2 * Math.PI / detail - Math.PI / 2);
                        float x = (float) Math.Cos(angle) * radius;
                        float y = (float) Math.Sin(angle) * radius;

                        _circleVerticesHolder[i] = new Vector2(radius + x, radius + y);
                    }
                }

                return _circleVerticesHolder;
            }
        }

        private static Vector2[] _circleVerticesHolder;

        #endregion
    }
}
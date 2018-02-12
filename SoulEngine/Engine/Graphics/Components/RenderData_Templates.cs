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
            Vertices = RectangleVertices;
            BreathDrawable.VerticesVBO = RectangleVBO;

            HasUpdated = true;
        }

        /// <summary>
        /// Transform the render data to a triangle.
        /// </summary>
        public void ApplyTemplate_Triangle()
        {           
            Vertices = TriangleVertices;
            BreathDrawable.VerticesVBO = TriangleVBO;

            HasUpdated = true;
        }

        /// <summary>
        /// Transform the render data to a circle.
        /// </summary>
        public void ApplyTemplate_Circle()
        {
            Vertices = CircleVertices;
            BreathDrawable.VerticesVBO = CircleVBO;

            HasUpdated = true;
        }

        #endregion

        #region Rectangle

        internal static VBO RectangleVBO
        {
            get
            {
                if (_rectangleVBOHolder == null)
                {
                    _rectangleVBOHolder = new VBO();
                    _rectangleVBOHolder.Upload(RectangleVertices);
                }

                return _rectangleVBOHolder;
            }
        }

        private static VBO _rectangleVBOHolder;

        internal static Vector2[] RectangleVertices =
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        #endregion

        #region Triangle

        internal static VBO TriangleVBO
        {
            get
            {
                if (_triangleVBOHolder == null)
                {
                    _triangleVBOHolder = new VBO();
                    _triangleVBOHolder.Upload(TriangleVertices);
                }

                return _triangleVBOHolder;
            }
        }

        private static VBO _triangleVBOHolder;

        internal static Vector2[] TriangleVertices =
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
                    _circleVBOHolder.Upload(CircleVertices);
                }

                return _circleVBOHolder;
            }
        }

        private static VBO _circleVBOHolder;

        internal static Vector2[] CircleVertices
        {
            get
            {
                if (_circleVerticesHolder == null)
                {
                    float radius = 0.5f;
                    int detail = 20;

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
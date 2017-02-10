using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Objects.Components;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A camera object.
    /// </summary>
    public class Camera : GameObject
    {
        #region "Declarations"
        /// <summary>
        /// Private holder for the zoom level.
        /// </summary>
        private float _zoom = 1;
        /// <summary>
        /// The camera's zoom level.
        /// </summary>
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (value < 0)
                    value = 0;

                _zoom = value;
            }
        }
        /// <summary>
        /// The transformation matrix to render through.
        /// </summary>
        public Matrix View
        {
            get
            {
                return ViewParallax(Vector2.One);
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new camera object.
        /// </summary>
        public Camera()
        {
            //Add a transform to the camera.
            AddComponent(new Transform(new Vector2(0,0), new Vector2(Settings.Width, Settings.Height)));

            lockComponentAdding = true;
            lockComponentRemoving = true;
        }


        #region "Functions"
        /// <summary>
        /// Converts a camera coordinate to a screen coordinate.
        /// </summary>
        /// <param name="x">The X coordinate of the camera coordinate to convert.</param>
        /// <param name="y">The Y coordinate of the camera coordinate to convert.</param>
        /// <returns>The screen coordinate that corresponds to the provided world coordinate.</returns>
        public Vector2 WorldToScreen(float x, float y)
        {
            return WorldToScreen(new Vector2(x, y));
        }
        /// <summary>
        /// Converts a camera coordinate to a screen coordinate.
        /// </summary>
        /// <param name="worldPosition">The camera coordinate to convert.</param>
        /// <returns>The screen coordinate that corresponds to the provided world coordinate.</returns>
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            var viewport = Context.Graphics.Viewport;
            return Vector2.Transform(worldPosition + new Vector2(viewport.X, viewport.Y), View);
        }

        /// <summary>
        /// Converts a screen coordinate to a world coordinate.
        /// </summary>
        /// <param name="x">The X coordinate of the screen coordinate to convert.</param>
        /// <param name="y">The Y coordinate of the screen coordinate to convert.</param>
        /// <returns>The world coordinate that corresponds to the provided screen coordinate.</returns>
        public Vector2 ScreenToWorld(float x, float y)
        {
            return ScreenToWorld(new Vector2(x, y));
        }
        /// <summary>
        /// Converts a screen coordinate to a world coordinate.
        /// </summary>
        /// <param name="screenPosition">The screen coordinate to convert.</param>
        /// <returns>The world coordinate that corresponds to the provided screen coordinate.</returns>
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            var viewport = Context.Graphics.Viewport;
            return Vector2.Transform(screenPosition - new Vector2(viewport.X, viewport.Y), Matrix.Invert(View));
        }

        /// <summary>
        /// The transformation matrix to render through with an added parallax factor.
        /// </summary>
        /// <param name="parallaxFactor">The parallax factor.</param>
        /// <returns>The transformation matrix to render through.</returns>
        public Matrix ViewParallax(Vector2 parallaxFactor)
        {
            return GetVirtualViewMatrix(parallaxFactor) * Context.Screen.View;
        }
        #endregion

        #region "Internal Functions"
        /// <summary>
        /// Constructs the matrix.
        /// </summary>
        private Matrix GetVirtualViewMatrix(Vector2 parallaxFactor)
        {
            return
                Matrix.CreateTranslation(new Vector3(-Component<Transform>().Position * parallaxFactor, 0.0f)) * //Position is modified by parallax.
                Matrix.CreateTranslation(new Vector3(-Component<Transform>().Center, 0.0f)) * //Center is origin.
                Matrix.CreateRotationZ(Component<Transform>().Rotation) * //Rotation is based on rotation.
                Matrix.CreateScale(Zoom, Zoom, 1) * //Scale is based on zoom level.
                Matrix.CreateTranslation(new Vector3(Component<Transform>().Center, 0.0f)); //Center is origin.
        }
        #endregion
    }
}

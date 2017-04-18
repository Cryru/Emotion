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
    public class Camera : ViewObject
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
        public override Matrix View
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
            //Camera bounds.
            X = 0;
            Y = 0;
            Width = Settings.Width;
            Height = Settings.Height;

            lockComponentAdding = true;
            lockComponentRemoving = true;
        }

        #region "Functions"
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
                Matrix.CreateTranslation(new Vector3(-Position * parallaxFactor, 0.0f)) * //Position is modified by parallax.
                Matrix.CreateTranslation(new Vector3(-Center, 0.0f)) * //Center is origin.
                Matrix.CreateRotationZ(Rotation) * //Rotation is based on rotation.
                Matrix.CreateScale(Zoom, Zoom, 1) * //Scale is based on zoom level.
                Matrix.CreateTranslation(new Vector3(Center, 0.0f)); //Center is origin.
        }
        #endregion
    }
}

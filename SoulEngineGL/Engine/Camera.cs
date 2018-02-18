// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Microsoft.Xna.Framework;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine
{
    /// <summary>
    /// A 2D camera.
    /// </summary>
    public class Camera
    {
        #region Properties

        /// <summary>
        /// The camera's zoom level.
        /// </summary>
        public float Zoom = 1;

        /// <summary>
        /// The rotation of the camera.
        /// </summary>
        public float Rotation = 0;

        /// <summary>
        /// The camera bounds.
        /// </summary>
        public Rectangle Bounds;

        #endregion

        /// <summary>
        /// Create a new camera object.
        /// </summary>
        public Camera()
        {
            Bounds = new Rectangle(0, 0, Settings.Width, Settings.Height);
        }

        /// <summary>
        /// Returns the camera's transformation matrix.
        /// </summary>
        /// <returns>The camera's transformation matrix.</returns>
        public Matrix GetMatrix()
        {
            Matrix cameraMatrix =
                Matrix.CreateTranslation(new Vector3(-Bounds.Location.ToVector2(), 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Bounds.Center.ToVector2(), 0.0f)) *
                Matrix.CreateRotationZ(Rotation) * // Apply rotation.
                Matrix.CreateScale(Zoom, Zoom, 1) * // Apply zoom.
                Matrix.CreateTranslation(new Vector3(Bounds.Center.ToVector2(), 0.0f)); // Apply the center as origin of the rotation and zoom.

            // Multiply by the screen matrix.
            return cameraMatrix * WindowManager.GetScreenMatrix();
        }
    }
}
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame, CraftworkGames                //
    //                                                                          //
    // A camera object.                                                         //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    public class Camera
    {
        #region "Variables"
        private readonly Viewport viewport; //The viewport of the camera.
        public Vector2 Position; //The position of the camera.
        public float Rotation; //The rotation of the camera.
        public float Zoom; //The zoom of the camera.
        public Vector2 Origin; //The center of the starting location of the camera.
        private float _minimumZoom; //The minimum zoom.
        public float MinimumZoom
        {
            get { return _minimumZoom; }
            set
            {
                if (value < 0)
                    value = 0;

                if (Zoom < value)
                    Zoom = MinimumZoom;

                _minimumZoom = value;
            }
        } //The accessor of the minimum zoom variable.
        private float _maximumZoom = float.MaxValue; //The maximum zoom.
        public float MaximumZoom
        {
            get { return _maximumZoom; }
            set
            {
                if (value < 0)
                    value = 0;

                if (Zoom > value)
                    Zoom = value;

                _maximumZoom = value;
            }
        } //The accessor of the maximum zoom variable.
        #endregion

        //Initialization
        public Camera(Viewport _viewport)
        {
            //Set the viewport.
            viewport = _viewport;
            //Default values.
            Rotation = 0;
            Zoom = 1;
            Position = Vector2.Zero;
            //Find the origin.
            Origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f); 
        }

        //Move the camera.
        public void Move(Vector2 direction)
        {
            Position += Vector2.Transform(direction, Matrix.CreateRotationZ(-Rotation));
        }

        //Rotate the camera.
        public void Rotate(float deltaRadians)
        {
            Rotation += deltaRadians;
        }

        //Zoom the camera in.
        public void ZoomIn(float deltaZoom)
        {
            ClampZoom(Zoom + deltaZoom);
        }

        //Zoom the camera out.
        public void ZoomOut(float deltaZoom)
        {
            ClampZoom(Zoom - deltaZoom);
        }

        //The actual zooming function.
        private void ClampZoom(float value)
        {
            if (value < MinimumZoom)
                Zoom = MinimumZoom;
            else if (value > MaximumZoom)
                Zoom = MaximumZoom;
            else
                Zoom = value;
        }

        //Find a point in the world from a point on the screen by a float position.
        public Vector2 WorldToScreen(float x, float y)
        {
            return WorldToScreen(new Vector2(x, y));
        }

        //Find a point in the world from a point on the screen by a Vector2 position.
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition + new Vector2(viewport.X, viewport.Y), GetViewMatrix());
        }

        //Find a point on the screen from a point in the world by a float position.
        public Vector2 ScreenToWorld(float x, float y)
        {
            return ScreenToWorld(new Vector2(x, y));
        }

        //Find a point on the screen from a point in the world by a Vector2 position.
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition - new Vector2(viewport.X, viewport.Y), Matrix.Invert(GetViewMatrix()));
        }

        //Return a transformation matrix used for rendering.
        public Matrix GetViewMatrix()
        {
            return
                Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }
    }
}
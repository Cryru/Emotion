using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoulEngine
{
    public enum BoxingMode
    {
        None, Letterbox, Pillarbox
    }

    public class ViewAdapter
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private readonly GameWindow _window;

        public int VirtualWidth { get; }
        public int VirtualHeight { get; }
        public int ViewportWidth => GraphicsDevice.Viewport.Width;
        public int ViewportHeight => GraphicsDevice.Viewport.Height;
        public GraphicsDevice GraphicsDevice { get; }

        public Viewport Viewport => GraphicsDevice.Viewport;

        public Rectangle BoundingRectangle => new Rectangle(0, 0, VirtualWidth, VirtualHeight);
        public Point Center => BoundingRectangle.Center;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="graphicsDevice"></param>
        /// <param name="virtualWidth"></param>
        /// <param name="virtualHeight"></param>
        public ViewAdapter(GameWindow window, GraphicsDevice graphicsDevice, int virtualWidth, int virtualHeight)
        {
            _window = window;
            VirtualWidth = virtualWidth;
            VirtualHeight = virtualHeight;
            GraphicsDevice = graphicsDevice;
        }

        public Matrix GetScaleMatrix()
        {
            var scaleX = (float)ViewportWidth / VirtualWidth;
            var scaleY = (float)ViewportHeight / VirtualHeight;
            return Matrix.CreateScale(scaleX, scaleY, 1.0f);
        }

        public BoxingMode BoxingMode { get; private set; }

        public void Update()
        {
            var viewport = GraphicsDevice.Viewport;

            var worldScaleX = (float)viewport.Width / VirtualWidth;
            var worldScaleY = (float)viewport.Height / VirtualHeight;

            var safeScaleX = (float)viewport.Width / (VirtualWidth);
            var safeScaleY = (float)viewport.Height / (VirtualHeight);

            float worldScale = MathHelper.Max(worldScaleX, worldScaleY);
            float safeScale = MathHelper.Min(safeScaleX, safeScaleY);
            float scale = MathHelper.Min(worldScale, safeScale);

            var width = (int)(scale * VirtualWidth + 0.5f);
            var height = (int)(scale * VirtualHeight + 0.5f);

            if (height >= viewport.Height && width < viewport.Width)
            {
                BoxingMode = BoxingMode.Pillarbox;
            }
            else if (width >= viewport.Height && height < viewport.Height)
            {
                BoxingMode = BoxingMode.Letterbox;
            }
            else
            {
                BoxingMode = BoxingMode.None;
            }

            var x = (viewport.Width / 2) - (width / 2);
            var y = (viewport.Height / 2) - (height / 2);
            GraphicsDevice.Viewport = new Viewport(x, y, width, height);

            // Needed for a DirectX bug in MonoGame 3.4. Hopefully it will be fixed in future versions
            // see http://gamedev.stackexchange.com/questions/68914/issue-with-monogame-resizing
            if (_graphicsDeviceManager != null &&
                    (_graphicsDeviceManager.PreferredBackBufferWidth != _window.ClientBounds.Width ||
                    _graphicsDeviceManager.PreferredBackBufferHeight != _window.ClientBounds.Height))
            {
                _graphicsDeviceManager.PreferredBackBufferWidth = _window.ClientBounds.Width;
                _graphicsDeviceManager.PreferredBackBufferHeight = _window.ClientBounds.Height;
                _graphicsDeviceManager.ApplyChanges();
            }
        }

        public Point PointToScreen(int x, int y)
        {
            var viewport = GraphicsDevice.Viewport;
            return PointToScreenHelper(x - viewport.X, y - viewport.Y);
        }

        public Point PointToScreenHelper(int x, int y)
        {
            var scaleMatrix = GetScaleMatrix();
            var invertedMatrix = Matrix.Invert(scaleMatrix);
            return Vector2.Transform(new Vector2(x, y), invertedMatrix).ToPoint();
        }
    }
}
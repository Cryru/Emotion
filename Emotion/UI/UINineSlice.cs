#region Using

using System;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UINineSlice : UITexture
    {
        public Rectangle Frame;

        private Rectangle _topLeftUV;
        private Vector3 _topLeftDraw;
        private Vector2 _topLeftDrawSize;

        private Rectangle _topRightUV;
        private Vector3 _topRightDraw;
        private Vector2 _topRightDrawSize;

        private Rectangle _bottomLeftUV;
        private Vector3 _bottomLeftDraw;
        private Vector2 _bottomLeftDrawSize;

        private Rectangle _bottomRightUV;
        private Vector3 _bottomRightDraw;
        private Vector2 _bottomRightDrawSize;

        private Rectangle _centerUV;
        private Vector3 _centerDraw;
        private Vector2 _centerDrawSize;

        private Rectangle _verticalBarLeftUV;
        private Vector3 _verticalBarLeftDraw;
        private Vector2 _verticalBarLeftDrawSize;
        private Vector2 _verticalBarLeftTileArea;

        private Rectangle _verticalBarRightUV;
        private Vector3 _verticalBarRightDraw;
        private Vector2 _verticalBarRightDrawSize;
        private Vector2 _verticalBarRightTileArea;

        private Rectangle _horizontalBarTopUV;
        private Vector3 _horizontalBarTopDraw;
        private Vector2 _horizontalBarTopDrawSize;
        private Vector2 _horizontalBarTopTileArea;

        private Rectangle _horizontalBarBottomUV;
        private Vector3 _horizontalBarBottomDraw;
        private Vector2 _horizontalBarBottomDrawSize;
        private Vector2 _horizontalBarBottomTileArea;

        protected override void AfterLayout()
        {
            base.AfterLayout();
            if (TextureAsset == null) return;

            Vector2 imageScale = ImageScale ?? Vector2.One;
            Vector2 textureSize = TextureAsset.Texture.Size;
            _topLeftUV = new Rectangle(Vector2.Zero, Frame.Position);
            _topLeftDrawSize = _topLeftUV.Size * imageScale;
            _topLeftDraw = Position;

            _topRightUV = new Rectangle(textureSize.X - Frame.Width, 0, Frame.Width, Frame.Y);
            _topRightDrawSize = _topRightUV.Size * imageScale;
            _topRightDraw = new Vector3(X + Width - _topRightDrawSize.X, Y, Z);

            _bottomLeftUV = new Rectangle(0, textureSize.Y - Frame.Height, Frame.X, Frame.Height);
            _bottomLeftDrawSize = _bottomLeftUV.Size * imageScale;
            _bottomLeftDraw = new Vector3(X, Y + Height - _bottomLeftDrawSize.Y, Z);

            _bottomRightUV = new Rectangle(textureSize.X - Frame.Width, textureSize.Y - Frame.Height, Frame.Size);
            _bottomRightDrawSize = _bottomRightUV.Size * imageScale;
            _bottomRightDraw = new Vector3(X + Width - _bottomRightDrawSize.X, Y + Height - _bottomRightDrawSize.Y, Z);

            _centerUV = new Rectangle(Frame.X, Frame.Y, textureSize.X - Frame.Width - Frame.X, textureSize.Y - Frame.Height - Frame.Y);
            _centerDraw = new Vector3(_topLeftDraw.X + _topLeftDrawSize.X, _topLeftDraw.Y + _topLeftDrawSize.Y, Z);
            _centerDrawSize = new Vector2(Width - _topLeftDrawSize.X - _bottomRightDrawSize.X, Height - _topLeftDrawSize.Y - _bottomRightDrawSize.Y);

            _verticalBarLeftUV = new Rectangle(0, Frame.Y, Frame.X, textureSize.Y - Frame.Height - Frame.Y);
            _verticalBarLeftDrawSize = _verticalBarLeftUV.Size * imageScale;
            _verticalBarLeftDraw = new Vector3(_topLeftDraw.X, _topLeftDraw.Y + _topLeftDrawSize.Y, Z);
            _verticalBarLeftTileArea = new Vector2(0, Height - _topLeftDrawSize.Y - _bottomLeftDrawSize.Y);

            _verticalBarRightUV = new Rectangle(textureSize.X - Frame.Width, Frame.Y, Frame.Width, textureSize.Y - Frame.Height - Frame.Y);
            _verticalBarRightDrawSize = _verticalBarRightUV.Size * imageScale;
            _verticalBarRightDraw = new Vector3(_topRightDraw.X, _topRightDraw.Y + _topRightDrawSize.Y, Z);
            _verticalBarRightTileArea = new Vector2(0, Height - _topRightDrawSize.Y - _bottomRightDrawSize.Y);

            _horizontalBarTopUV = new Rectangle(Frame.X, 0, textureSize.X - Frame.X - Frame.Width, Frame.Y);
            _horizontalBarTopDrawSize = _horizontalBarTopUV.Size * imageScale;
            _horizontalBarTopDraw = new Vector3(_topLeftDraw.X + _topLeftDrawSize.X, Y, Z);
            _horizontalBarTopTileArea = new Vector2(Width - _topLeftDrawSize.X - _topRightDrawSize.X, 0);

            _horizontalBarBottomUV = new Rectangle(Frame.Width, textureSize.Y - Frame.Height, textureSize.X - Frame.X - Frame.Width, Frame.Height);
            _horizontalBarBottomDrawSize = _horizontalBarBottomUV.Size * imageScale;
            _horizontalBarBottomDraw = new Vector3(_bottomLeftDraw.X + _bottomLeftDrawSize.X, _bottomLeftDraw.Y, Z);
            _horizontalBarBottomTileArea = new Vector2(Width - _topLeftDrawSize.X - _topRightDrawSize.X, 0);

            base.AfterLayout();
        }

        private void DrawTileVertical(RenderComposer composer, Vector3 startPos, Vector2 tileSize, Vector2 tileArea, Rectangle uv)
        {
            if (tileSize.Y == 0) return;

            float bottomY = startPos.Y + tileArea.Y;
            for (float y = startPos.Y; y < bottomY; y += tileSize.Y)
            {
                float heightLeft = MathF.Min(bottomY - y, tileSize.Y);
                Rectangle tileUv = uv;
                if (heightLeft < tileSize.Y) tileUv.Height = tileUv.Height * heightLeft / tileSize.Y;

                composer.RenderSprite(new Vector3(startPos.X, y, Z), new Vector2(tileSize.X, heightLeft), _calculatedColor, TextureAsset.Texture, tileUv);
            }
        }

        private void DrawTileHorizontal(RenderComposer composer, Vector3 startPos, Vector2 tileSize, Vector2 tileArea, Rectangle uv)
        {
            if (tileSize.X == 0) return;

            float rightX = startPos.X + tileArea.X;
            for (float x = startPos.X; x < rightX; x += tileSize.X)
            {
                float widthLeft = MathF.Min(rightX - x, tileSize.X);
                Rectangle tileUv = uv;
                if (widthLeft < tileSize.X) tileUv.Width = tileUv.Width * widthLeft / tileSize.X;

                composer.RenderSprite(new Vector3(x, startPos.Y, Z), new Vector2(widthLeft, tileSize.Y), _calculatedColor, TextureAsset.Texture, tileUv);
            }
        }

        protected override Vector2 InternalMeasure(Vector2 space)
        {
            return space;
        }

        protected override bool RenderInternal(RenderComposer composer)
        {
            if (TextureAsset == null) return false;

            // Corners
            composer.RenderSprite(_topLeftDraw, _topLeftDrawSize, _calculatedColor, TextureAsset.Texture, _topLeftUV);
            composer.RenderSprite(_topRightDraw, _topRightDrawSize, _calculatedColor, TextureAsset.Texture, _topRightUV);
            composer.RenderSprite(_bottomLeftDraw, _bottomLeftDrawSize, _calculatedColor, TextureAsset.Texture, _bottomLeftUV);
            composer.RenderSprite(_bottomRightDraw, _bottomRightDrawSize, _calculatedColor, TextureAsset.Texture, _bottomRightUV);

            // Center
            composer.RenderSprite(_centerDraw, _centerDrawSize, _calculatedColor, TextureAsset.Texture, _centerUV);

            // Vertical bars
            DrawTileVertical(composer, _verticalBarLeftDraw, _verticalBarLeftDrawSize, _verticalBarLeftTileArea, _verticalBarLeftUV);
            DrawTileVertical(composer, _verticalBarRightDraw, _verticalBarRightDrawSize, _verticalBarRightTileArea, _verticalBarRightUV);

            // Horizontal bars
            DrawTileHorizontal(composer, _horizontalBarTopDraw, _horizontalBarTopDrawSize, _horizontalBarTopTileArea, _horizontalBarTopUV);
            DrawTileHorizontal(composer, _horizontalBarBottomDraw, _horizontalBarBottomDrawSize, _horizontalBarBottomTileArea, _horizontalBarBottomUV);

            return true;
        }
    }
}
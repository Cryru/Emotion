namespace Emotion.Game.Animation2D
{
    public class SpriteGridFrameSource : SpriteAnimationFrameSource
    {
        public Vector2 TextureSize;
        public Vector2 FrameSize;
        public Vector2 Spacing;

        public SpriteGridFrameSource(Vector2 textureSize, Vector2 frameSize)
        {
            TextureSize = textureSize;
            FrameSize = frameSize;
        }

        // serialization constructor
        protected SpriteGridFrameSource()
        {
        }

        public override int GetFrameCount()
        {
            if (FrameSize == Vector2.Zero) return 0;

            // todo: take spacing into consideration
            var columns = (int) MathF.Floor(TextureSize.X / FrameSize.X);
            var rows = (int) MathF.Floor(TextureSize.Y / FrameSize.Y);

            return columns * rows;
        }

        public override Rectangle GetFrameUV(int i)
        {
            if (FrameSize == Vector2.Zero) return Rectangle.Empty;

            // Legacy class can static method that can compute this for us.
            // In the future this function should be moved here.
            return Animation2DHelpers.GetGridFrameBounds(TextureSize, FrameSize, Spacing, i);
        }
    }
}
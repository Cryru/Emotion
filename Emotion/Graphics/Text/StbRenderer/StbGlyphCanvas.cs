namespace Emotion.Graphics.Text.StbRenderer
{
    public class StbGlyphCanvas
    {
        public byte[] Data;
        public int Width;
        public int Height;
        public int Stride { get; private set; }

        public StbGlyphCanvas(int width, int height)
        {
            Data = new byte[width * height];
            Width = width;
            Height = height;
            Stride = Width;
        }
    }
}
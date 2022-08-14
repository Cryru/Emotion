namespace Emotion.Graphics.Text.StbRenderer
{
    public class StbRendererActiveEdge
    {
        public StbRendererActiveEdge Next;
        public float Fx;
        public float Fdx;
        public float Fdy;
        public float Direction;
        public float Sy;
        public float Ey;

        public StbRendererActiveEdge(StbRendererGlyphEdge e, int offsetX, float start)
        {
            float dxDy = (e.X1 - e.X) / (e.Y1 - e.Y);
            Fdx = dxDy;
            Fdy = dxDy != 0.0f ? 1.0f / dxDy : 0.0f;
            Fx = e.X + dxDy * (start - e.Y);
            Fx -= offsetX;
            Direction = e.Invert ? 1.0f : -1.0f;
            Sy = e.Y;
            Ey = e.Y1;
        }
    }
}
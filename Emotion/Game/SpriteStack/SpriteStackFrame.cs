#region Using

using Emotion.Graphics.Data;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.SpriteStack
{
    public class SpriteStackFrame
    {
        public Color[] Pixels;
        public int FilledPixels;
        public VertexData[] Vertices;
        public ushort[] Indices;

        public SpriteStackFrame(int w, int h)
        {
            Pixels = new Color[w * h];
            for (var i = 0; i < Pixels.Length; i++)
            {
                Pixels[i].A = 0;
            }
        }

        public void SetPixel(int idx, Color c)
        {
            Pixels[idx] = c;
            FilledPixels++;
        }
    }
}
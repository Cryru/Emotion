#region Using

using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.SpriteStack
{
    public class SpriteStackFrame : Mesh
    {
        public Color[] Pixels;
        public int FilledPixels;

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
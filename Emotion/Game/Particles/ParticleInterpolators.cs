using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Game.Particles
{
    public class ColorAtTime
    {
        public float PercentOfLifetime;
        public Color Color;

        public ColorAtTime(float percent, Color color)
        {
            PercentOfLifetime = percent;
            Color = color;
        }
    }

    public class SizeAtTime
    {
        public float PercentOfLifetime;
        public Vector2 Size;

        public SizeAtTime(float percent, Vector2 size)
        {
            PercentOfLifetime = percent;
            Size = size;
        }
    }
}

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Game.Effects
{
    public class Palette
    {
        public string Name { get; set; }
        public Color[] Colors { get; set; }

        public Palette()
        {
        }

        public Palette(string name, Color[] colors)
        {
            Name = name;
            Colors = colors;
        }
    }
}
// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public abstract class Renderable : Transform
    {
        public abstract void Draw(Renderer renderer);
    }
}
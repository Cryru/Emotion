// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Graphics.Objects
{
    public interface IGLObject
    {
        void Bind();
        void Unbind();
        void Delete();
    }
}
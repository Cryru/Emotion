// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Graphics.GLES
{
    public interface IGLObject
    {
        void Bind();
        void Unbind();
        void Delete();
    }
}
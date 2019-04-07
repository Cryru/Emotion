#region Using

using Adfectus.Graphics;

#endregion

namespace Emotion.Engine.Scenography
{
    public abstract class Scene : Adfectus.Scenography.Scene
    {
        public override void Update()
        {
            Update(Adfectus.Common.Engine.FrameTime);
        }

        public override void Draw()
        {
            Draw(Adfectus.Common.Engine.Renderer);
        }

        public abstract void Update(float dt);
        public abstract void Draw(Renderer renderer);
    }
}
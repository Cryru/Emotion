using System;
using System.Collections.Generic;
using System.Text;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Scenography;

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

// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Graphics;

#endregion

namespace Emotion.Tests.Layers
{
    /// <summary>
    /// Layer which allows for external calls to its functions.
    /// </summary>
    public class ExternalLayer : LayerLoading
    {
        public Action ExtLoad = null;
        public Action ExtUpdate = null;
        public Action ExtDraw = null;
        public Action ExtUnload = null;

        public override void Load()
        {
            ExtLoad?.Invoke();
            base.Load();
        }

        public override void Update(float frameTime)
        {
            ExtUpdate?.Invoke();
            base.Update(frameTime);
        }

        public override void Draw(Renderer renderer)
        {
            ExtDraw?.Invoke();
            base.Draw(renderer);
        }

        public override void Unload()
        {
            ExtUnload?.Invoke();
            base.Unload();
        }
    }
}
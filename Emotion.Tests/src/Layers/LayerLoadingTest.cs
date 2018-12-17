// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Threading;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Layers
{
    public class LayerLoadingTest : Layer
    {
        public bool LoadCalled;
        public bool UpdateCalled;
        public bool DrawCalled;
        public bool UnloadCalled;

        public override void Load()
        {
            // Check if also not called on the GL Thread.
            if (Thread.CurrentThread.Name != "GL Thread") LoadCalled = true;
        }

        public override void Update(float frameTime)
        {
            UpdateCalled = true;
        }

        public override void Draw(Renderer renderer)
        {
            Assert.AreEqual(Context.Renderer, renderer);

            DrawCalled = true;
        }

        public override void Unload()
        {
            UnloadCalled = true;
        }
    }
}
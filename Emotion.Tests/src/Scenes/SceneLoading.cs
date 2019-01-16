// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Threading;
using Emotion.Engine;
using Emotion.Engine.Scenography;
using Emotion.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Scenes
{
    /// <summary>
    /// Scene used for testing loading and unloading of scenes.
    /// </summary>
    public class SceneLoading : Scene
    {
        public bool LoadCalled;
        public bool SyncLoadCalled;
        public bool UpdateCalled;
        public int FocusLossCalled;
        public bool DrawCalled;
        public bool UnloadCalled;
        public bool SyncUnloadCalled;

        public override void Load()
        {
            // Check if also not called on the GL Thread.
            if (Thread.CurrentThread.Name != "GL Thread")
                LoadCalled = true;
            else
                SyncLoadCalled = true;
        }

        public override void FocusLoss()
        {
            FocusLossCalled++;
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
            // Check if also not called on the GL Thread.
            if (Thread.CurrentThread.Name != "GL Thread")
                UnloadCalled = true;
            else
                SyncUnloadCalled = true;
        }
    }
}
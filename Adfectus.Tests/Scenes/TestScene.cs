#region Using

using System;
using Adfectus.Common;
using Adfectus.Common.Threading;
using Adfectus.Scenography;

#endregion

namespace Adfectus.Tests.Scenes
{
    /// <summary>
    /// Scene used for testing.
    /// </summary>
    public class TestScene : Scene
    {
        public bool LoadCalled;
        public bool UpdateCalled;
        public int FocusLossCalled;
        public bool Focused;
        public bool DrawCalled;
        public bool UnloadCalled;

        private int _cycles = -1;
        private int _waitCycles;
        private AwAction _waitToken;

        public Action ExtLoad = null;
        public Action ExtUpdate = null;
        public Action ExtDraw = null;
        public Action ExtUnload = null;

        public override void Load()
        {
            ExtLoad?.Invoke();
            LoadCalled = true;
        }

        public override void NoFocusUpdate()
        {
            FocusLossCalled++;
            Focused = !Engine.IsUnfocused;
        }

        public override void Update()
        {
            ExtUpdate?.Invoke();
            UpdateCalled = true;
        }

        public override void Draw()
        {
            ExtDraw?.Invoke();
            DrawCalled = true;

            // Check if waiting.
            if (_waitToken != null)
            {
                _cycles++;

                if (_cycles > _waitCycles)
                {
                    _waitToken.Run();
                    _waitToken = null;
                }
            }
        }

        public override void Unload()
        {
            ExtUnload?.Invoke();
            UnloadCalled = true;
        }

        public AwAction WaitFrames(int cycles)
        {
            _cycles = 0;
            _waitCycles = cycles;
            _waitToken = new AwAction();

            return _waitToken;
        }
    }
}
#region Using

using System;
using Adfectus.Common;

#endregion

namespace Rationale.Interop
{
    public class RationalePlugin : Plugin
    {
        private float _curFps;
        private int _frameCounter;
        private DateTime _lastSec = DateTime.Now;

        private DateTime _lastSecTickTracker = DateTime.Now;
        private int _tickTracker;
        private float _curTps;

        private Communicator _debugCommunicator = new Communicator();

        public override void Initialize()
        {
            DebugLogger logger = (DebugLogger) Engine.Log;
            logger.AttachCommunicator(_debugCommunicator);
        }

        public override void Update()
        {
            // Tick counter.
            if (_lastSecTickTracker.Second < DateTime.Now.Second)
            {
                _lastSecTickTracker = DateTime.Now;
                _curTps = _tickTracker;
                _tickTracker = 0;

                _debugCommunicator.SendMessage(new DebugMessage {Type = MessageType.CurrentTPS, Data = _curTps});
            }

            _tickTracker++;
        }

        public void Draw()
        {
            // Fps counter.
            if (_lastSec.Second < DateTime.Now.Second)
            {
                _lastSec = DateTime.Now;
                _curFps = _frameCounter;
                _frameCounter = 0;
                _debugCommunicator.SendMessage(new DebugMessage {Type = MessageType.CurrentFPS, Data = _curFps});
            }

            _frameCounter++;
        }

        public override void Dispose()
        {
        }
    }
}
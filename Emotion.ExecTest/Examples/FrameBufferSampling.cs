#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;

#endregion

namespace Emotion.ExecTest.Examples
{
    internal class FrameBufferSampling : IScene
    {
        private static byte _r;
        private Color _lastColorResult;
        private FrameBufferSampleRequest _sampleReq;
        private bool _unsych = true;
        private static FrameBuffer _fbo;

        public void Load()
        {
            GLThread.ExecuteGLThread(() => { _fbo = new FrameBuffer(new Vector2(100, 100)).WithColor(); });
        }

        public void Update()
        {
            if (Engine.InputManager.IsKeyDown(Key.U))
            {
                _unsych = !_unsych;
                Engine.Log.Warning($"Unsynch: {_unsych}", "Other");
            }

            _r++;
        }

        public void Draw(RenderComposer composer)
        {
            composer.RenderToAndClear(_fbo);
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(100, 100), new Color(_r, (byte) 50, (byte) 50));
            composer.RenderTo(null);
            composer.SetUseViewMatrix(true);

            if (_unsych)
            {
                if (_sampleReq == null || _sampleReq.Finished)
                {
                    if (_sampleReq != null && _sampleReq.Finished) _lastColorResult = new Color(_sampleReq.Data[2], _sampleReq.Data[1], _sampleReq.Data[0], _sampleReq.Data[3]);
                    _sampleReq = _fbo.SampleUnsynch(new Rectangle(0, 0, 1, 1));
                }
            }
            else
            {
                byte[] sampleReq = _fbo.Sample(new Rectangle(0, 0, 1, 1));
                _lastColorResult = new Color(sampleReq[2], sampleReq[1], sampleReq[0], sampleReq[3]);
            }


            composer.RenderSprite(new Vector3(0, 0, 10), new Vector2(100, 100), _lastColorResult);
        }

        public void Unload()
        {
            GLThread.ExecuteGLThread(() => { _fbo.Dispose(); });
        }
    }
}
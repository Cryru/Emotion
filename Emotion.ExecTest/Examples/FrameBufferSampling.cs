#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Core.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using OpenGL;

#endregion

namespace Emotion.ExecTest.Examples
{
    internal class FrameBufferSampling : IScene
    {
        private static byte _currentRedValue;
        private Color _lastColorResult;
        private FrameBufferSampleRequest _sampleReq;
        private bool _asyncSample = true;
        private static FrameBuffer _fbo;

        public void Load()
        {
            GLThread.ExecuteGLThread(() => { _fbo = new FrameBuffer(new Vector2(100, 100)).WithColor(); });
        }

        public void Update()
        {
            if (Engine.Host.IsKeyDown(Key.U))
            {
                _asyncSample = !_asyncSample;
                Engine.Log.Warning($"Current Mode: {(_asyncSample ? "Async" : "Sync")}", "Other");
            }

            _currentRedValue++;
        }

        public void Draw(RenderComposer composer)
        {
            composer.RenderToAndClear(_fbo);
            composer.SetUseViewMatrix(false);
            composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(100, 100), new Color(_currentRedValue, (byte) 50, (byte) 50));
            composer.RenderTo(null);
            composer.SetUseViewMatrix(true);
            composer.RenderSprite(new Vector3(10, -50, 0), new Vector2(100, 100), new Color(_currentRedValue, (byte) 50, (byte) 50));

            if (_asyncSample)
            {
                if (_sampleReq == null || _sampleReq.Finished)
                {
                    if (_sampleReq != null && _sampleReq.Finished) _lastColorResult = new Color(_sampleReq.Data[0], _sampleReq.Data[1], _sampleReq.Data[2], _sampleReq.Data[3]);
                    _sampleReq = _fbo.SampleAsync(new Rectangle(0, 0, 1, 1), PixelFormat.Rgba);
                }
            }
            else
            {
                byte[] sampleReq = _fbo.Sample(new Rectangle(0, 0, 1, 1), PixelFormat.Rgba);
                _lastColorResult = new Color(sampleReq[0], sampleReq[1], sampleReq[2], sampleReq[3]);
            }

            composer.RenderSprite(new Vector3(-100, -50, 10), new Vector2(100, 100), _lastColorResult);
        }

        public void Unload()
        {
            GLThread.ExecuteGLThread(() => { _fbo.Dispose(); });
        }
    }
}
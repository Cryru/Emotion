#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.WebGL;
using Emotion.Common;
using Emotion.Web.Platform;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web
{
    public class WebGLComponent : BECanvasComponent
    {
        public WebGLContext GL;

        // ReSharper disable once UnassignedField.Global
        protected BECanvasComponent _canvasReference;

        private Action _tickAction;
        private Action _drawAction;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                GL = await _canvasReference.CreateWebGLAsync(new WebGLContextAttributes
                {
                    PowerPreference = WebGLContextAttributes.POWER_PREFERENCE_HIGH_PERFORMANCE,
                });

                await WebGLCache.PopulateCache(GL);

                // This host is initialized prior to the engine.
                Engine.Setup(new Configurator
                {
                    LoopFactory = (tick, draw) =>
                    {
                        _tickAction = tick;
                        _drawAction = draw;
                    },
                    PlatformOverride = new WebHost
                    {
                        ContextSetter = new WebGL(GL)
                    },
                    Logger = new WebLogger(),
                });
            }
        }

        [JSInvokable]
        public void RunLoop(float timeStamp)
        {
            //await _gl.ClearColorAsync(_test ? 1 : 0, 0, 0, 1);
            //await _gl.ClearAsync(BufferBits.COLOR_BUFFER_BIT);
            //_test = !_test;

            _tickAction?.Invoke();
            _drawAction?.Invoke();
        }

        [JSInvokable]
        public void HostResized(int width, int height)
        {
            if (Engine.Host != null)
            {
                Engine.Log.Info($"Size set! {width}x{height}", "");
                Engine.Host.Size = new Vector2(width, height);
                Engine.Host.OnResize.Invoke(Engine.Host.Size);
            }
        }
    }
}
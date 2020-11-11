#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Web.Platform;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web
{
    public class WebGLComponent : BECanvasComponent
    {
        // ReSharper disable once UnassignedField.Global
        protected BECanvasComponent _canvasReference;

        private Action _tickAction;
        private Action _drawAction;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return Task.CompletedTask;

            Engine.Setup(new Configurator
            {
                LoopFactory = (tick, draw) =>
                {
                    _tickAction = tick;
                    _drawAction = draw;
                },
                PlatformOverride = new WebHost
                {
                    ContextSetter = new WebGL(JSRuntime)
                },
                Logger = new WebLogger(),
            });

            return Task.CompletedTask;
        }

        [JSInvokable]
        public void RunLoop(float timeStamp)
        {
            _tickAction?.Invoke();
            _drawAction?.Invoke();
        }

        [JSInvokable]
        public void HostResized(int width, int height)
        {
            if (Engine.Host == null) return;
            Engine.Log.Info($"Size set! {width}x{height}", "");
            Engine.Host.Size = new Vector2(width, height);
            Engine.Host.OnResize.Invoke(Engine.Host.Size);
        }
    }
}
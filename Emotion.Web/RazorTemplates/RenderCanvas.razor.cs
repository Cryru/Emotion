#region Using

using System;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Web.Platform;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web.RazorTemplates
{
    public partial class RenderCanvas : ComponentBase
    {
        [Inject]
        protected EmotionSetupService SetupService { get; set; }

        // https://github.com/mono/mono/blob/b6ef72c244bd33623d231ff05bc3d120ad36b4e9/sdks/wasm/src/binding_support.js
        // https://www.meziantou.net/generating-and-downloading-a-file-in-a-blazor-webassembly-application.htm
        [Inject]
        public IJSUnmarshalledRuntime JsRuntime { get; set; }

        [Inject]
        protected IJSInProcessRuntime _jsRuntimeMarshalled { get; set; }

        private Action _tickAction;
        private Action _drawAction;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return Task.CompletedTask;

            // Initiate Javascript
            _jsRuntimeMarshalled.InvokeVoid("InitJavascript", DotNetObjectReference.Create(this));
            int[] hostSizeInitial = _jsRuntimeMarshalled.Invoke<int[]>("GetHostSize");

            Configurator webConfig = new Configurator
            {
                LoopFactory = (tick, draw) =>
                {
                    _tickAction = tick;
                    _drawAction = draw;
                },
                PlatformOverride = new WebHost(this)
                {
                    Size = new Vector2(hostSizeInitial[0], hostSizeInitial[1])
                },
                Logger = new WebLogger()
            };
            SetupService.SetupEngine(webConfig);
            return Task.CompletedTask;
        }

        [JSInvokable]
        public void RunLoop(float timeStamp)
        {
            if (Engine.Status != EngineStatus.Running) return;
            _tickAction?.Invoke();
            _drawAction?.Invoke();
        }

        [JSInvokable]
        public void HostResized(int width, int height)
        {
            if (Engine.Host == null) return;
            Engine.Host.Size = new Vector2(width, height);
        }
    }
}
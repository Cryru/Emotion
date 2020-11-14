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
    public partial class RenderCanvas
    {
        // This service will be provided a config and it needs to setup the engine.
        [Inject]
        protected EmotionSetupService _setupService { get; set; }

        // https://github.com/mono/mono/blob/b6ef72c244bd33623d231ff05bc3d120ad36b4e9/sdks/wasm/src/binding_support.js
        // https://www.meziantou.net/generating-and-downloading-a-file-in-a-blazor-webassembly-application.htm
        [Inject]
        public IJSUnmarshalledRuntime JsRuntime { get; set; }

        [Inject]
        public IJSInProcessRuntime JsRuntimeMarshalled { get; set; }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return Task.CompletedTask;

            // Initiate Javascript
            var platformHost = new WebHost(this);
            var webConfig = new Configurator
            {
                LoopFactory = (tick, draw) =>
                {
                    platformHost.TickAction = tick;
                    platformHost.DrawAction = draw;
                },
                PlatformOverride = platformHost,
                Logger = new WebLogger()
            };
            _setupService.SetupEngine(webConfig);
            return Task.CompletedTask;
        }
    }
}
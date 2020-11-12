#region Using

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web
{
    public class CanvasBase : ComponentBase
    {
        [Parameter]
        public long Height { get; set; }

        [Parameter]
        public long Width { get; set; }

        protected readonly string Id = Guid.NewGuid().ToString();
        protected ElementReference _canvasRef;

        protected ElementReference CanvasReference
        {
            get => _canvasRef;
        }

        // https://github.com/mono/mono/blob/b6ef72c244bd33623d231ff05bc3d120ad36b4e9/sdks/wasm/src/binding_support.js
        // https://www.meziantou.net/generating-and-downloading-a-file-in-a-blazor-webassembly-application.htm
        [Inject]
        public IJSUnmarshalledRuntime JSRuntime { get; set; }

        [Inject]
        protected IJSInProcessRuntime JSRuntimeMarshalled { get; set; }
    }
}
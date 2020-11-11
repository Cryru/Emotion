#region Using

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web
{
    public class BECanvasComponent : ComponentBase
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

        [Inject]
        protected IJSInProcessRuntime JSRuntime { get; set; }
    }
}
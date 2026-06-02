#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Emotion.Core.Platform.Implementation.Web.Razor;

[DontSerialize]
public partial class RenderCanvas
{
    [Inject]
    protected EmotionWebService SetupService { get; set; } = null!;

    [Inject]
    public IJSInProcessRuntime JsRuntime { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || SetupService == null) return;
        WebPlatform.ActiveHost = new WebPlatform(this);
        SetupService.InitCode();
    }
}

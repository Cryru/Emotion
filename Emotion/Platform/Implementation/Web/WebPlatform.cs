#region Using

using Emotion.Common.Serialization;
using Emotion.Platform.Implementation.Web.Razor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Net.Http;

#endregion

#nullable enable

namespace Emotion.Platform.Implementation.Web;

[DontSerialize]
public class WebPlatform : PlatformBase
{
    public override WindowState WindowState
    {
        get => Emotion.Platform.WindowState.Normal;
        set
        {
        }
    }

    protected override void SetupInternal(Configurator config)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(config.GetExecutionArguments());
        builder.RootComponents.Add<RenderCanvas>("#app");
        builder.Services.AddSingleton(services => (IJSInProcessRuntime)services.GetRequiredService<IJSRuntime>());
        //builder.Services.AddSingleton(services => (IJSUnmarshalledRuntime)services.GetRequiredService<IJSRuntime>());
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton(services => new EmotionWebService());
        builder.Build().RunAsync();
    }

    protected override bool UpdatePlatform()
    {
        return true;
    }

    protected override Vector2 GetPosition()
    {
        return Vector2.Zero;
    }

    protected override Vector2 GetSize()
    {
        return Vector2.One;
    }

    public override void DisplayMessageBox(string message)
    {
        Console.WriteLine(message);
    }

    #region Libraries (NOP)

    public override nint GetLibrarySymbolPtr(nint library, string symbolName)
    {
        return 0;
    }

    public override nint LoadLibrary(string path)
    {
        return 0;
    }

    #endregion

    #region Window (NOP)

    protected override void SetPosition(Vector2 position)
    {

    }

    protected override void SetSize(Vector2 size)
    {

    }

    protected override void UpdateDisplayMode()
    {

    }

    #endregion
}
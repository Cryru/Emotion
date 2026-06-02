#nullable enable

using Emotion.Core.Platform.Implementation.Web.Razor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Threading.Tasks;

namespace Emotion.Core.Platform.Implementation.Web;

[DontSerialize]
public static class WebBootstrap
{
    public static async Task RunAsync(string[] args, EmotionWebService setup)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<RenderCanvas>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        builder.Services.AddSingleton(sp => (IJSInProcessRuntime)sp.GetRequiredService<IJSRuntime>());
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton(setup);
        await builder.Build().RunAsync();
    }
}

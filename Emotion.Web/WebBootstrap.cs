#region Using

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Emotion.Web.RazorTemplates;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web
{
    public static class LibraryBootstrap
    {
        public static async Task MainLibrary(string[] args, EmotionSetupService setup)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<RenderCanvas>("#app");
            builder.Services.AddSingleton(services => (IJSInProcessRuntime) services.GetRequiredService<IJSRuntime>());
            builder.Services.AddSingleton(services => (IJSUnmarshalledRuntime) services.GetRequiredService<IJSRuntime>());
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton(services => setup);
            await builder.Build().RunAsync();
        }
    }
}
#region Using

using System.Threading.Tasks;
using Emotion.Web.RazorTemplates;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

#endregion

namespace Emotion.Web.Internal
{
    public class WebBootstrap
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<RenderCanvas>("#app");

            builder.Services.AddSingleton(services => (IJSInProcessRuntime) services.GetRequiredService<IJSRuntime>());
            builder.Services.AddSingleton(services => (IJSUnmarshalledRuntime) services.GetRequiredService<IJSRuntime>());
            await builder.Build().RunAsync();
        }
    }
}
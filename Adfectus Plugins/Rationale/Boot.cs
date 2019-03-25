#region Using

using System;
using System.Reflection;
using Adfectus.Common;
using Adfectus.ImGuiNet;
using Rationale.Interop;

#endregion

namespace Rationale
{
    public class Boot
    {
        public static Action Main(EngineBuilder builder, Assembly gameAssembly)
        {
            RationalePlugin pluginHook = new RationalePlugin();
            builder.AddGenericPlugin(pluginHook);

            // Add dependency plugin.
            builder.AddGenericPlugin(new ImGuiNetPlugin("SourceSans.ttf", 15, 15));
            return pluginHook.Draw;
        }
    }
}
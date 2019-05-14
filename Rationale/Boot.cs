#region Using

using System;
using Adfectus.Common;
using Adfectus.ImGuiNet;
using Adfectus.Platform.DesktopGL;
using Rationale.Interop;
using Rationale.Windows;

#endregion

namespace Rationale
{
    public class Boot
    {
        /// <summary>
        /// Entry point for the debugger app.
        /// </summary>
        public static void Main()
        {
            EngineBuilder builder = new EngineBuilder();
            ImGuiNetPlugin imGuiPlugin = new ImGuiNetPlugin();
            builder.AddGenericPlugin(imGuiPlugin);
            Engine.Flags.PauseOnFocusLoss = false;
            builder.SetLogger<DebugLogger>();

            Engine.Setup<DesktopPlatform>(builder);
            Engine.SceneManager.SetScene(new RationaleScene(imGuiPlugin));
            Engine.Run();
        }

        /// <summary>
        /// Entry point for the debugger injection.
        /// </summary>
        /// <param name="builder">The builder the engine will be built with.</param>
        /// <returns>A draw hook for the injector, as plugins can't draw by themselves.</returns>
        public static Action Inject(EngineBuilder builder)
        {
            // Substitute logger with debug logger.
            builder.SetLogger<DebugLogger>();
            Engine.Flags.PauseOnFocusLoss = false;
            RationalePlugin pluginHook = new RationalePlugin();
            builder.AddGenericPlugin(pluginHook);
            return pluginHook.Draw;
        }
    }
}
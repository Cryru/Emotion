#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion.Common;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Tools.DevUI;
using Emotion.UI;

#endregion

namespace Emotion.Tools
{
    public static class ToolsManager
    {
        public static Dictionary<string, Action<UIBaseWindow>> CustomToolsFactory = new();

        public static void ConfigureForTools(Configurator config)
        {
            if (config.Plugins.Any(x => x.GetType() == typeof(ImGuiNetPlugin))) return;
            config.AddPlugin(new ImGuiNetPlugin());
        }

        public static ToolsWindow AddToolBoxWindow(UIController controller)
        {
            var toolWin = new ToolsWindow();
            controller.AddChild(toolWin);
            return toolWin;
        }

        public static void AddGameSpecificTool(string tool, Action<UIBaseWindow> factory)
        {
            if (CustomToolsFactory.ContainsKey(tool)) return;

            CustomToolsFactory.Add(tool, factory);
        }

        public static void AddGameSpecificTool(string tool, Action<WindowManager> factory)
        {
            if (CustomToolsFactory.ContainsKey(tool)) return;

            CustomToolsFactory.Add(tool, toolManager =>
            {
                var manager = (ToolsWindow) toolManager;
                factory(manager.LegacyWindowManager);
            });
        }
    }
}
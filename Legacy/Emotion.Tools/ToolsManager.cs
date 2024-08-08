#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Tools.DevUI;
using Emotion.UI;

#endregion

namespace Emotion.Tools
{
    public static class ToolsManager
    {
        /// <summary>
        /// Whether the tools menu is currently open, sets the visibility of all tool windows in all controllers.
        /// The tools menu can be toggled using the "~" key.
        /// </summary>
        public static bool ToolsOpen
        {
            get => _toolsOpen;
            set
            {
                _toolsOpen = value;
                ApplyVisibility();
            }
        }

        private static bool _toolsOpen = true;

        public static Dictionary<string, Action<UIBaseWindow>> CustomToolsFactory = new();
        private static List<UIBaseWindow> _toolsWindows = new List<UIBaseWindow>();

        public static void ConfigureForTools(Configurator config)
        {
            if (config.Plugins.All(x => x.GetType() != typeof(ImGuiNetPlugin))) config.AddPlugin(new ImGuiNetPlugin());
            // Coroutines will be run after the engine is setup.
            Engine.CoroutineManager.StartCoroutine(AttachKeyListener());
        }

        private static IEnumerator AttachKeyListener()
        {
            Engine.Host.OnKey.AddListener(ToolsToggleKeyListener, KeyListenerType.System);
            yield break;
        }

        private static bool ToolsToggleKeyListener(Key key, KeyStatus status)
        {
            if (key == Key.GraveAccent && status == KeyStatus.Down)
            {
                ToolsOpen = !ToolsOpen;
                return false;
            }

            return true;
        }

        public static ToolsWindow AddToolBoxWindow(UIController controller)
        {
            var toolWin = new ToolsWindow
            {
                Visible = ToolsOpen
            };
            _toolsWindows.Add(toolWin);
            controller.AddChild(toolWin);
            return toolWin;
        }

        private static void ApplyVisibility()
        {
            for (int i = _toolsWindows.Count - 1; i >= 0; i--)
            {
                _toolsWindows[i].Visible = ToolsOpen;
                if (_toolsWindows[i].Controller == null) _toolsWindows.RemoveAt(i);
            }
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
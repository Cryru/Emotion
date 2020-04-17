﻿#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Tools.Windows.Art;
using Emotion.Tools.Windows.Audio;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public static class ToolsMenu
    {
        // ReSharper disable once CollectionNeverUpdated.Global
        public static Dictionary<string, Action<WindowManager>> CustomTools = new Dictionary<string, Action<WindowManager>>();
        public static WindowManager ToolsWindowManager;

        public static void RenderToolsMenu(this RenderComposer composer, WindowManager manager)
        {
            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("Audio"))
            {
                if (ImGui.MenuItem("Audio Mixer")) manager.AddWindow(new AudioMixer());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Art"))
            {
                if (ImGui.MenuItem("Animation Editor")) manager.AddWindow(new AnimationEditor());
                if (ImGui.MenuItem("Rogue Alpha Remover")) manager.AddWindow(new RogueAlphaRemoval());
                if (ImGui.MenuItem("Palette Editor")) manager.AddWindow(new PaletteEditor());
                if (ImGui.MenuItem("Font Preview")) manager.AddWindow(new FontEditor());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Gameplay"))
            {
                if (ImGui.MenuItem("Map Editor")) manager.AddWindow(new MapEditor());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Engine"))
            {
                if (ImGui.MenuItem("Performance Monitor")) manager.AddWindow(new PerformanceMonitor());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Game"))
            {
                if (ImGui.MenuItem("Open Folder")) Process.Start("explorer.exe", ".");

                foreach (KeyValuePair<string, Action<WindowManager>> tool in CustomTools.Where(tool => ImGui.MenuItem(tool.Key)))
                {
                    tool.Value(manager);
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        public static void RenderToolsMenu(this RenderComposer composer)
        {
            if (ToolsWindowManager == null) ToolsWindowManager = new WindowManager();

            ToolsWindowManager.Update();

            ImGui.NewFrame();
            RenderToolsMenu(composer, ToolsWindowManager);
            ToolsWindowManager.Render(composer);
            composer.RenderUI();
        }
    }
}
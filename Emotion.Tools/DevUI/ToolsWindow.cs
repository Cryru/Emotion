#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Tools.Editors;
using Emotion.Tools.Editors.Animation2D;
using Emotion.Tools.Editors.MapEditor;
using Emotion.Tools.Editors.UIEditor;
using Emotion.Tools.Windows;
using Emotion.Tools.Windows.Art;
using Emotion.Tools.Windows.Audio;
using Emotion.UI;
using ImGuiNET;

#endregion

namespace Emotion.Tools.DevUI
{
    public class ToolsWindow : ImGuiBaseWindow
    {
        public WindowManager LegacyWindowManager;

        private bool _captureMouse;
        private bool _captureKeyboard;
        private bool _captureText; // todo

        public ToolsWindow() : base("Tools Window")
        {
            _windowFlags |= ImGuiWindowFlags.MenuBar;
            Id = "ToolsRoot";
            LegacyWindowManager = new WindowManager();
            ZOffset = 99;
        }

        protected override void RenderImGui()
        {
            // The tools window overrides the entire render internal and doesn't need this.
            throw new NotImplementedException();
        }

        public override UIBaseWindow FindMouseInput(Vector2 pos)
        {
            if (_captureMouse)
                return this;
            return null;
        }

        public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
            if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd && _captureMouse) return false;
            if (_captureKeyboard) return false;
            return base.OnKey(key, status, mousePos);
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.SetUseViewMatrix(false);
            c.SetDepthTest(false);

            ImGui.NewFrame();
            ImGui.BeginMainMenuBar();
            GetImGuiMetrics();

            ImGuiIOPtr io = ImGui.GetIO();
            _captureMouse = io.WantCaptureMouse;
            _captureKeyboard = io.WantCaptureKeyboard;
            _captureText = io.WantTextInput;
            InputTransparent = !_captureMouse && !_captureKeyboard;

            if (ImGui.BeginMenu("Audio"))
            {
                if (ImGui.MenuItem("Audio Mixer")) AddLegacyWindow(new AudioMixer());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Art"))
            {
                if (ImGui.MenuItem("Animation Editor (Old)")) AddLegacyWindow(new AnimationEditor());
                if (ImGui.MenuItem("Animation Editor")) AddChild(new Animation2DEditor());
                if (ImGui.MenuItem("Rogue Alpha Remover")) AddLegacyWindow(new RogueAlphaRemoval());
                if (ImGui.MenuItem("Palette Editor")) AddLegacyWindow(new PaletteEditor());
                if (ImGui.MenuItem("Font Preview")) AddLegacyWindow(new FontPreview());
                if (ImGui.MenuItem("PNG Exporter")) AddLegacyWindow(new PngExporter());
                if (ImGui.MenuItem("3D Object Viewer")) AddLegacyWindow(new Viewer3D());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Gameplay"))
            {
                if (ImGui.MenuItem("Map Viewer")) AddChild(new MapEditor());
#if DEBUG
                if (ImGui.MenuItem("Collision Viewer")) AddLegacyWindow(new CollisionViewer());
#else
                ImGui.MenuItem("Collision Viewer [Requires DEBUG]");
#endif
                if (ImGui.MenuItem("UI Editor")) AddChild(new UIEditorWindow());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Engine"))
            {
                if (ImGui.MenuItem("Performance Monitor")) AddChild(new PerformanceMonitor());
                if (ImGui.MenuItem("Memory Viewer")) AddLegacyWindow(new MemoryViewer());
#if DEBUG
                if (ImGui.MenuItem("Coroutine Viewer")) AddLegacyWindow(new CoroutineViewer());
#else
                ImGui.MenuItem("Coroutine Viewer [Requires DEBUG]");
#endif
                if (ImGui.MenuItem("Gpu Texture Viewer")) AddLegacyWindow(new GpuTextureViewer());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Game"))
            {
                if (ImGui.MenuItem("Open Folder")) Process.Start("explorer.exe", ".");

                foreach (KeyValuePair<string, Action<UIBaseWindow>> tool in ToolsManager.CustomToolsFactory)
                {
                    if (ImGui.MenuItem(tool.Key)) tool.Value(this);
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();

            LegacyWindowManager.Update();
            LegacyWindowManager.Render(c);
            if (Children == null) AfterRenderChildren(c);
            return true;
        }

        protected override void AfterRenderChildren(RenderComposer c)
        {
            ImGuiNetPlugin.RenderUI(c);
        }

        public void AddLegacyWindow(ImGuiWindow oldWin)
        {
            LegacyWindowManager.AddWindow(oldWin);
        }
    }
}
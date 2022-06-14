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
            return base.FindMouseInput(pos);
        }

        public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
            if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd && _captureMouse) return false;
            if (_captureKeyboard) return false;
            return false;
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

            var fullScreenEditorOpen = false;
            if (Children != null)
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    UIBaseWindow child = Children[i];
                    if (child.Size == c.CurrentTarget.Size)
                    {
                        // todo: maybe something more explicit, like a bool in ImGuiWindow?
                        fullScreenEditorOpen = true;
                    }
                }
            }
            InputTransparent = !_captureMouse && !_captureKeyboard && !fullScreenEditorOpen;

            if (ImGui.BeginMenu("Audio"))
            {
                if (ImGui.MenuItem("Audio Preview")) AddLegacyWindow(new AudioMixer());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Art"))
            {
                if (ImGui.MenuItem("Animation Editor")) AddLegacyWindow(new AnimationEditor());
                if (ImGui.MenuItem("Animation Editor New (WIP)")) AddChild(new Animation2DEditor());
                if (ImGui.MenuItem("Rogue Alpha Remover")) AddLegacyWindow(new RogueAlphaRemoval());
                if (ImGui.MenuItem("Palette Editor")) AddLegacyWindow(new PaletteEditor());
                if (ImGui.MenuItem("Font Preview")) AddLegacyWindow(new FontPreview());
                if (ImGui.MenuItem("PNG Exporter")) AddLegacyWindow(new PngExporter());
                if (ImGui.MenuItem("3D Object Viewer")) AddChild(new Viewer3D());
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Gameplay"))
            {
                if (ImGui.MenuItem("Map Editor")) AddChild(new MapEditor());
                if (ImGui.MenuItem("Tmx Viewer")) AddChild(new TmxViewer());
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

        public override void AddChild(UIBaseWindow child)
        {
            base.AddChild(child);
            Controller?.InvalidateInputFocus();
        }
    }
}
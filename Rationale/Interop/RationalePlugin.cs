#region Using

using System;
using System.Numerics;
using System.Reflection;
using Adfectus.Common;
using Adfectus.ImGuiNet;
using Adfectus.IO;
using Adfectus.Primitives;
using ImGuiNET;

#endregion

namespace Rationale.Interop
{
    public class RationalePlugin : Plugin
    {
        public ImGuiNetPlugin GuiPlugin;

        private float _curFps;
        private int _frameCounter;
        private DateTime _lastSec = DateTime.Now;

        private DateTime _lastSecTickTracker = DateTime.Now;
        private int _tickTracker;
        private float _curTps;

        public override void Initialize()
        {
            Engine.AssetLoader.AddSource(new EmbeddedAssetSource(Assembly.GetExecutingAssembly(), "Assets"));
        }

        public override void Update()
        {
            // Tick counter.
            if (_lastSecTickTracker.Second < DateTime.Now.Second)
            {
                _lastSecTickTracker = DateTime.Now;
                _curTps = _tickTracker;
                _tickTracker = 0;
            }

            _tickTracker++;
        }

        public override void Dispose()
        {
        }

        private bool _firstDraw = true;

        private float[] _frameTimesRaw = new float[200];
        private int _frameTimesRawIndexNext;

        private Asset _temp;

        private Color _colorBrightest = new Color(186, 123, 161);
        private Color _colorBrightestAlpha = new Color(186, 123, 161, 125);
        private Color _colorDarkest = new Color(0, 0, 0);
        private Color _colorHighlight = new Color(77,126,168);
        private Color _colorHighlightActive = new Color(112,151,185);
        private Color _colorAlt = new Color(47, 69, 80);
        private Color _colorAltBright = new Color(88, 111, 124);

        public void Draw()
        {
            // Fps counter.
            if (_lastSec.Second < DateTime.Now.Second)
            {
                _lastSec = DateTime.Now;
                _curFps = _frameCounter;
                _frameCounter = 0;
            }

            _frameCounter++;

            // Frame times plot line.
            _frameTimesRaw[_frameTimesRawIndexNext] = (float)Engine.RawFrameTime;
            _frameTimesRawIndexNext++;
            if (_frameTimesRawIndexNext > _frameTimesRaw.Length - 1) _frameTimesRawIndexNext = 0;

            ImGui.NewFrame();
            GuiPlugin.UseFont("SourceSans.ttf");

            ImGui.PushStyleColor(ImGuiCol.Border, _colorDarkest.ToUint());
            ImGui.PushStyleColor(ImGuiCol.WindowBg, _colorAlt.ToUint());
            ImGui.PushStyleColor(ImGuiCol.TitleBg, _colorBrightestAlpha.ToUint());
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, _colorBrightest.ToUint());
            ImGui.PushStyleColor(ImGuiCol.FrameBg, _colorDarkest.ToUint());
            ImGui.PushStyleColor(ImGuiCol.ResizeGrip, _colorBrightest.ToUint());

            ImGui.PushStyleColor(ImGuiCol.CheckMark, _colorHighlight.ToUint());

            ImGui.PushStyleColor(ImGuiCol.Button, _colorHighlight.ToUint());
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, _colorBrightest.ToUint());
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, _colorBrightestAlpha.ToUint());

            ImGui.PushStyleColor(ImGuiCol.PlotLines, _colorHighlight.ToUint());
            ImGui.PushStyleColor(ImGuiCol.PlotLinesHovered, _colorBrightest.ToUint());

            ImGui.PushStyleColor(ImGuiCol.NavHighlight, _colorHighlight.ToUint());

            if (_firstDraw)
            {
                ImGui.SetNextWindowPos(new Vector2(0, 0));
                ImGui.SetNextWindowSize(new Vector2(200, 150));
                _firstDraw = false;
            }

            ImGui.Begin("Rationale Debugger - Stats");

            ImGui.Text($"FPS: {_curFps}");
            ImGui.Text($"TPS: {_curTps}");

            // Draw frame time plot line.
            ImGui.BeginGroup();
            ImGui.AlignTextToFramePadding();
            ImGui.PushItemWidth(-1);
            ImGui.Text("Frame Time:");
            ImGui.SameLine();
            ImGui.PlotLines("", ref _frameTimesRaw[0], _frameTimesRaw.Length, 0, "", 0, 50);
            ImGui.PopItemWidth();

            // Debugging tools.
            if (ImGui.Button("Asset Debug"))
            {
                WindowManager.AddWindow(new AssetDebugger());
            }

            if (ImGui.Button("Script Debug"))
            {
                WindowManager.AddWindow(new ScriptDebugger());
            }

            ImGui.EndGroup();
            ImGui.End();

            // Draw other windows.
            WindowManager.DrawWindows();

            // Done.
            GuiPlugin.UseFont(null);
            Engine.Renderer.RenderGui();

            // Restore default style.
            ImGui.StyleColorsClassic();
        }
    }
}
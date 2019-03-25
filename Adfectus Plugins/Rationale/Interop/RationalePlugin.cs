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
            _frameTimesRaw[_frameTimesRawIndexNext] = (float) Engine.RawFrameTime;
            _frameTimesRawIndexNext++;
            if (_frameTimesRawIndexNext > _frameTimesRaw.Length - 1) _frameTimesRawIndexNext = 0;

            ImGui.NewFrame();
            if (_firstDraw)
            {
                ImGui.SetNextWindowPos(new Vector2(0, 0));
                ImGui.SetNextWindowSize(new Vector2(200, 100));
                _firstDraw = false;
            }

            ImGui.Begin("Stats");
            ImGui.Text($"FPS: {_curFps}");
            ImGui.Text($"TPS: {_curTps}");

            // Draw frame time plot line.
            ImGui.BeginGroup();
            ImGui.AlignTextToFramePadding();
            ImGui.PushItemWidth(-1);
            ImGui.Text("Frame Time:");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.PlotLines, new Color(255, 255, 255).ToUint());
            ImGui.PushStyleColor(ImGuiCol.Border, new Color(73, 10, 109).ToUint());
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Color(53, 48, 56, 125).ToUint());
            ImGui.PushStyleColor(ImGuiCol.TitleBg, new Color(143, 21, 214, 125).ToUint());
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Color(165, 51, 232, 125).ToUint());
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Color(185, 115, 226).ToUint());
            ImGui.PushStyleColor(ImGuiCol.CheckMark, new Color(255, 255, 255).ToUint());
            ImGui.PlotLines("", ref _frameTimesRaw[0], _frameTimesRaw.Length, 0, "", 0, 50);
            ImGui.PopStyleColor();
            ImGui.PopItemWidth();
            ImGui.EndGroup();
            ImGui.End();
            Engine.Renderer.RenderGui();
        }
    }
}
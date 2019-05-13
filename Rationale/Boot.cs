#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.ImGuiNet;
using Adfectus.IO;
using Adfectus.Platform.DesktopGL;
using Adfectus.Primitives;
using Adfectus.Scenography;
using ImGuiNET;
using Rationale.Interop;

#endregion

namespace Rationale
{
    public class Boot
    {
        public static void Main()
        {
            EngineBuilder builder = new EngineBuilder();
            ImGuiNetPlugin imGuiPlugin = new ImGuiNetPlugin();
            builder.AddGenericPlugin(imGuiPlugin);

            Engine.Setup<DesktopPlatform>(builder);
            Engine.SceneManager.SetScene(new RationaleScene(imGuiPlugin));
            Engine.Run();
        }

        public static Action Main(EngineBuilder builder, Assembly gameAssembly)
        {
            RationalePlugin pluginHook = new RationalePlugin();
            builder.AddGenericPlugin(pluginHook);

            // (int) (builder.RenderSize.Y * 15 / 540f)

            // Add dependency plugin.
            builder.AddGenericPlugin(new ImGuiNetPlugin());
            // This needs to be gotten like this, because someone could've loaded the plugin before Rationale - very unlikely, but possible.
            ImGuiNetPlugin guiPlugin = (ImGuiNetPlugin)builder.Plugins.FirstOrDefault(x => x is ImGuiNetPlugin);
            if (guiPlugin == null)
            {
                ErrorHandler.SubmitError(new Exception("Rationale couldn't find ImGuiNetPlugin."));
                return () => { };
            }
            // Load font and attach to the Rationale plugin.
            guiPlugin.LoadFont("SourceSans.ttf", 15, 15);
            pluginHook.GuiPlugin = guiPlugin;
            return pluginHook.Draw;
        }
    }

    public class RationaleScene : Scene
    {
        private float _curFps;
        private int _frameCounter;
        private DateTime _lastSec = DateTime.Now;

        private DateTime _lastSecTickTracker = DateTime.Now;
        private int _tickTracker;
        private float _curTps;

        private bool _firstDraw = true;

        private float[] _frameTimesRaw = new float[200];
        private int _frameTimesRawIndexNext;

        private Asset _temp;

        private Color _colorBrightest = new Color(186, 123, 161);
        private Color _colorBrightestAlpha = new Color(186, 123, 161, 125);
        private Color _colorDarkest = new Color(0, 0, 0);
        private Color _colorHighlight = new Color(77, 126, 168);
        private Color _colorHighlightActive = new Color(112, 151, 185);
        private Color _colorAlt = new Color(47, 69, 80);
        private Color _colorAltBright = new Color(88, 111, 124);

        #region Loader

        private bool _loaded;
        private string _loadPath = "";

        #endregion

        public ImGuiNetPlugin GuiPlugin;

        public RationaleScene(ImGuiNetPlugin guiPlugin)
        {
            guiPlugin.LoadFont("SourceSans.ttf", 15, 15);
            GuiPlugin = guiPlugin;
        }

        public override void Load()
        {

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

        public override void Draw()
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

            if (!_loaded)
            {
                ImGui.Begin("Rationale - Loader");
                ImGui.InputText("Dll Path", ref _loadPath, 100);
                if (ImGui.Button("Load"))
                {
                    if (File.Exists(_loadPath))
                    {
                        _loaded = true;

                        Process proc = Process.Start(_loadPath);
                        
                    }
                }
                ImGui.End();
            }

            // Draw other windows.
            WindowManager.DrawWindows();

            // Done.
            GuiPlugin.UseFont(null);
            Engine.Renderer.RenderGui();

            // Restore default style.
            ImGui.StyleColorsClassic();
        }

        public override void Unload()
        {

        }
    }
}
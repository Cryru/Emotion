#region Using

using System.Diagnostics;
using System.Numerics;
using Adfectus.Common;
using Adfectus.ImGuiNet;
using Adfectus.IO;
using Adfectus.Primitives;
using Adfectus.Scenography;
using ImGuiNET;
using Rationale.Interop;

#endregion

namespace Rationale.Windows
{
    public class RationaleScene : Scene
    {
        private bool _firstDraw = true;

        private Color _colorBrightest = new Color(186, 123, 161);
        private Color _colorBrightestAlpha = new Color(186, 123, 161, 125);
        private Color _colorDarkest = new Color(0, 0, 0);
        private Color _colorHighlight = new Color(77, 126, 168);
        private Color _colorHighlightActive = new Color(112, 151, 185);
        private Color _colorAlt = new Color(47, 69, 80);
        private Color _colorAltBright = new Color(88, 111, 124);

        private Communicator _debugCommunication;
        private Process _proc;
        public ImGuiNetPlugin GuiPlugin;
        private LogViewerWindow _logViewer;

        public RationaleScene(ImGuiNetPlugin guiPlugin)
        {
            guiPlugin.LoadFont("SourceSans.ttf", 15, 15);
            GuiPlugin = guiPlugin;
        }

        public override void Load()
        {
            _logViewer = new LogViewerWindow();
        }

        public override void Update()
        {
        }

        #region Main Window

        private float[] _frameTimesRaw = new float[200];
        private int _frameTimesRawIndexNext;
        private float _curTps;
        private float _curFps;

        private void MainWindowTPSDisplay(float tpsPoll)
        {
            _curTps = tpsPoll;
            _frameTimesRaw[_frameTimesRawIndexNext] = tpsPoll;
            _frameTimesRawIndexNext++;
            if (_frameTimesRawIndexNext > _frameTimesRaw.Length - 1) _frameTimesRawIndexNext = 0;
        }

        #endregion

        public override void Draw()
        {
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
            if (_debugCommunication == null && ImGui.Button("Load")) WindowManager.AddWindow(new LoadExecutableWindow(this));
            if (_debugCommunication != null && ImGui.Button("Asset Debug"))
            {
                _debugCommunication.SendMessage(new DebugMessage { Type = MessageType.RequestAssetData });
                WindowManager.AddWindow(new AssetDebugger());
            }

            if (_debugCommunication != null && ImGui.Button("Script Debug")) WindowManager.AddWindow(new ScriptDebugger());
            if (_debugCommunication != null && ImGui.Button("Log Viewer"))
            {
                _logViewer.Reopen();
                WindowManager.AddWindow(_logViewer);
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

        public override void Unload()
        {
            if (_proc == null || _proc.HasExited) return;
            _proc.Kill();
            _proc.WaitForExit();
        }

        public void AttachedToProcess(Process proc, Communicator processCommunicator)
        {
            _proc = proc;
            _debugCommunication = processCommunicator;
            _debugCommunication.MessageReceiveCallback = MessageBus;
        }

        private void MessageBus(DebugMessage msg)
        {
            switch (msg.Type)
            {
                case MessageType.CurrentFPS:
                    _curFps = (float)msg.Data;
                    break;
                case MessageType.CurrentTPS:
                    MainWindowTPSDisplay((float)msg.Data);
                    break;
                case MessageType.MessageLogged:
                    _logViewer.AddLogMessage((string)msg.Data);
                    break;
                case MessageType.AssetData:
                    AssetDebugger.AllAssetsCache = (string[])msg.StringArrayData;
                    break;
                case MessageType.LoadedAssetData:
                    AssetDebugger.LoadedAssetsCache = (string[])msg.StringArrayData;
                    break;
            }
        }
    }
}
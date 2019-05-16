#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using ImGuiNET;
using Rationale.Interop;

#endregion

namespace Rationale.Windows
{
    public class LoadExecutableWindow : Window
    {
        private string _loadPath = "C:\\Users\\Vlad\\Desktop\\Emotion\\Adfectus.ExecTest\\bin\\Debug\\netcoreapp3.0\\Adfectus.ExecTest.exe";
        private RationaleScene _scene;
        private string _error = "";

        public LoadExecutableWindow(RationaleScene scene) : base("Inject Into", new Vector2(350, 100))
        {
            _scene = scene;
        }

        protected override void DrawContent()
        {
            ImGui.InputText("Executable Path", ref _loadPath, 100);
            if (ImGui.Button("Load"))
            {
                if (File.Exists(_loadPath))
                {
                    try
                    {
                        File.Copy("Rationale.dll", Path.Combine(Path.GetDirectoryName(_loadPath), "Rationale.dll"), true);
                    }
                    catch (Exception)
                    {
                        // no-op
                    }

                    ProcessStartInfo info = new ProcessStartInfo {FileName = _loadPath};
                    Communicator coms = new Communicator(9991);
                    Process proc = Process.Start(info);

                    // Signal the scene that we connected.
                    _scene.AttachedToProcess(proc, coms);
                    Close();
                }
                else
                {
                    _error = "File doesn't exist.";
                }
            }

            ImGui.Text(_error);
        }
    }
}
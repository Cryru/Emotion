#region Using

using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

#endregion

namespace Rationale.Windows
{
    public class LogViewerWindow : Window
    {
        private List<string> _internalLog = new List<string>();

        public LogViewerWindow() : base("Log Viewer", new Vector2(400, 400))
        {
        }

        protected override void DrawContent()
        {
            lock (_internalLog)
            {
                foreach (string text in _internalLog)
                {
                    ImGui.Text(text);
                }
            }
        }

        public void AddLogMessage(string msg)
        {
            lock (_internalLog)
            {
                _internalLog.Add(msg);
            }
        }
    }
}
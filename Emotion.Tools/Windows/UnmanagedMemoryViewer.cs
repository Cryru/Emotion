#region Using

using System.Diagnostics;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Utility;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class UnmanagedMemoryViewer : ImGuiWindow
    {
        private Process _p;

        public UnmanagedMemoryViewer() : base("Unmanaged Memory")
        {
            _p = Process.GetCurrentProcess();
        }

        protected override void RenderContent(RenderComposer composer)
        {
            ImGui.Text($"Working Memory: {Helpers.FormatByteAmountAsString(_p.WorkingSet64)}");
            ImGui.Text(UnmanagedMemoryAllocator.GetDebugInformation());
        }

        public override void Update()
        {
        }
    }
}
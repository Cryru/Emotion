#region Using

using System.Diagnostics;
using System.Linq;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Utility;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class MemoryViewer : ImGuiWindow
    {
        private Process _p;

        public MemoryViewer() : base("Memory")
        {
            _p = Process.GetCurrentProcess();
        }

        protected override void RenderContent(RenderComposer composer)
        {
            long usedRAM = _p.WorkingSet64;
            ImGui.Text($"Working Memory: {Helpers.FormatByteAmountAsString(usedRAM)}");
            ImGui.Text(UnmanagedMemoryAllocator.GetDebugInformation());

            ImGui.Text(" ");
            ImGui.Text("Loaded Assets: ");
            Asset[] loadedAssets = Engine.AssetLoader.LoadedAssets;
            IOrderedEnumerable<Asset> orderedEnum = loadedAssets.OrderByDescending(x => x.Size);
            foreach (Asset asset in orderedEnum)
            {
                float percent = (float) asset.Size / usedRAM;
                ImGui.Text($"{asset.Name} {Helpers.FormatByteAmountAsString(asset.Size)} {percent * 100:0}%%");
                ImGui.Text($"\t{asset.GetType()}");
            }
        }

        public override void Update()
        {
        }
    }
}
using System.Numerics;
using Adfectus.IO;
using ImGuiNET;
using Rationale.Utility;

namespace Rationale.Interop
{
    public sealed class AssetDataWindow : Window
    {
        public Asset Asset { get; private set; }

        public AssetDataWindow(Asset asset) : base($"Asset Data [{asset.Name}]", new Vector2(100, 150))
        {
            Asset = asset;
        }

        protected override void DrawContent()
        {
            ImGui.Text(ObjectDumper.Dump(Asset));
        }
    }
}
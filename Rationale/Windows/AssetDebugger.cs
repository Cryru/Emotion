#region Using

using System.Linq;
using System.Numerics;
using Adfectus.Common;
using Adfectus.IO;
using ImGuiNET;
using Rationale.Interop;

#endregion

namespace Rationale.Windows
{
    public sealed class AssetDebugger : Window
    {
        public AssetDebugger() : base("Asset Debugger", new Vector2(200, 250))
        {
        }

        protected override void DrawContent()
        {
            if (ImGui.TreeNode("Loaded Assets"))
            {
                ImGui.Indent();
                foreach (Asset asset in Engine.AssetLoader.LoadedAssets)
                {
                    if (asset.Disposed)
                        ImGui.TextColored(new Vector4(1, 0, 0, 1), $"[{asset.GetType()}] {asset.Name}");
                    else
                        ImGui.Text($"[{asset.GetType()}] {asset.Name}");

                    if (ImGui.IsItemClicked()) WindowManager.AddWindow(new AssetDataWindow(asset));
                }

                ImGui.Unindent();
                ImGui.TreePop();
            }

            if (!ImGui.TreeNode("All Assets")) return;

            ImGui.Indent();
            Asset[] loadedAssets = Engine.AssetLoader.LoadedAssets;
            foreach (string asset in Engine.AssetLoader.AllAssets)
            {
                if (loadedAssets.Select(x => x.Name).Contains(asset))
                    ImGui.TextColored(new Vector4(0, 1, 0, 1), asset);
                else
                    ImGui.Text(asset);
            }

            ImGui.Unindent();
            ImGui.TreePop();
        }
    }
}
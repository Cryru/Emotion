#region Using

using System.Linq;
using System.Numerics;
using Adfectus.IO;
using ImGuiNET;
using Rationale.Interop;

#endregion

namespace Rationale.Windows
{
    public sealed class AssetDebugger : Window
    {
        public static string[] LoadedAssetsCache = new string[0];
        public static string[] AllAssetsCache = new string[0];

        public AssetDebugger() : base("Asset Debugger", new Vector2(200, 250))
        {
        }

        protected override void DrawContent()
        {
            if (ImGui.TreeNode("Loaded Assets"))
            {
                ImGui.Indent();
                foreach (string asset in LoadedAssetsCache)
                {
                    ImGui.Text(asset);

                    //if (ImGui.IsItemClicked()) WindowManager.AddWindow(new AssetDataWindow(asset));
                }

                ImGui.Unindent();
                ImGui.TreePop();
            }

            if (!ImGui.TreeNode("All Assets")) return;

            ImGui.Indent();
            string[] loadedAssets = LoadedAssetsCache;
            foreach (string asset in AllAssetsCache)
            {
                if (loadedAssets.Contains(asset))
                    ImGui.TextColored(new Vector4(0, 1, 0, 1), asset);
                else
                    ImGui.Text(asset);
            }

            ImGui.Unindent();
            ImGui.TreePop();
        }
    }
}
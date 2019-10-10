#region Using

using System;
using System.IO;
using System.Linq;
using Emotion.Common;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class FileExplorer<T> : ImGuiWindow where T : Asset, new()
    {
        private Action<T> _fileSelected;
        private string _customFile = "";

        /// <summary>
        /// Create a file explorer dialog.
        /// </summary>
        /// <param name="fileSelected">The callback to receive the selected file.</param>
        public FileExplorer(Action<T> fileSelected) : base($"Pick a [{typeof(T)}]")
        {
            _fileSelected = fileSelected;
        }

        protected override void RenderContent()
        {
            // Add custom path option.
            ImGui.InputText("Custom File: ", ref _customFile, 300);
            ImGui.SameLine();
            if (ImGui.Button("Load") && File.Exists(_customFile))
            {
                T file = ExplorerLoadAsset(_customFile);
                _fileSelected?.Invoke(file);
                Open = false;
                return;
            }

            // Get all available assets.
            string[] assets = Engine.AssetLoader.AllAssets;
            assets = assets.OrderBy(x => x).ToArray();
            string directory = null;
            bool nodeOpen = false;
            for (int i = 0; i < assets.Length; i++)
            {
                string curDirectory = Path.GetDirectoryName(assets[i]);
                // If the next asset is from a different directory, swap the tree node.
                if (curDirectory != directory)
                {
                    if (nodeOpen) ImGui.TreePop();
                    nodeOpen = false;
                    if (ImGui.TreeNode(string.IsNullOrEmpty(curDirectory) ? "/" : curDirectory)) nodeOpen = true;
                    directory = curDirectory;
                }

                if (nodeOpen)
                    if (ImGui.Button(Path.GetFileName(assets[i])))
                    {
                        // Load the asset custom so the asset loader's caching doesn't get in the way.
                        T file = ExplorerLoadAsset(_customFile);
                        if (file != null)
                        {
                            _fileSelected?.Invoke(file);
                            Open = false;
                            return;
                        }
                    }
            }

            if (nodeOpen) ImGui.TreePop();
        }

        public override void Update()
        {
        }

        public static T ExplorerLoadAsset(string name)
        {
            // Try to load through the asset loader.
            AssetSource source = Engine.AssetLoader.GetSource(name);
            if (source == null)
            {
                // Try the file system.
                if (File.Exists(name))
                {
                    T file = new T
                    {
                        Name = name
                    };
                    file.Create(File.ReadAllBytes(name));
                    return file;
                }

                // Not found
                return default;
            }

            {
                T file = new T
                {
                    Name = name
                };
                file.Create(source.GetAsset(name));
                return file;
            }
        }
    }
}
#region Using

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class FileExplorer<T> : ImGuiModal where T : Asset, new()
    {
        private Action<T> _fileSelected;
        private string _customFile = "";
        private Task _loadingTask;

        private Tree<string, string> _fileSystem;

        /// <summary>
        /// Create a file explorer dialog.
        /// </summary>
        /// <param name="fileSelected">The callback to receive the selected file.</param>
        public FileExplorer(Action<T> fileSelected) : base($"Pick a [{typeof(T)}]")
        {
            _fileSelected = fileSelected;
            _fileSystem = new Tree<string, string>();
            string[] assets = Engine.AssetLoader.AllAssets;
            foreach (string a in assets)
            {
                if (a.Contains('/'))
                {
                    string[] folderPath = a.Split('/')[..^1];
                    _fileSystem.Add(folderPath, a);
                }
                else
                {
                    _fileSystem.Leaves.Add(a);
                }
            }
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (_loadingTask != null && _loadingTask.Status != TaskStatus.Faulted)
            {
                ImGui.Text("Loading...");
                return;
            }

            // Add custom path option.
            ImGui.InputText("Custom File: ", ref _customFile, 300);
            ImGui.SameLine();
            if (ImGui.Button("Load") && File.Exists(_customFile))
            {
                _loadingTask = Task.Run(async () =>
                {
                    T file = await ExplorerLoadAssetAsync(_customFile);
                    if (file == null)
                    {
                        _loadingTask = null;
                        return;
                    }

                    _fileSelected?.Invoke(file);
                    Open = false;
                });

                return;
            }

            RenderTree(_fileSystem, 0, true);
        }

        private void RenderTree(Tree<string, string> tree, int depth, bool skipFold = false)
        {
            // Add branches
            Tree<string, string> current = tree;

            bool open = skipFold || ImGui.TreeNode(string.IsNullOrEmpty(current.Name) ? "/" : current.Name);
            if (!open) return;

            // Render branches.
            foreach (Tree<string, string> b in current.Branches)
            {
                ImGui.PushID(depth);
                RenderTree(b, depth++);
                ImGui.PopID();
            }

            // Render leaves. (Some LINQ magic here to not render past the clicked button)
            foreach (string name in current.Leaves.Where(name => ImGui.Button(Path.GetFileName(name))))
            {
                // Load the asset custom so the asset loader's caching doesn't get in the way.
                _loadingTask = Task.Run(async () =>
                {
                    T file = await ExplorerLoadAssetAsync(name);
                    if (file == null)
                    {
                        _loadingTask = null;
                        return;
                    }

                    _fileSelected?.Invoke(file);
                    Open = false;
                });
            }

            if(!skipFold) ImGui.TreePop();
        }

        public override void Update()
        {
        }

        public static async Task<T> ExplorerLoadAssetAsync(string name)
        {
            return await Task.Run(() => ExplorerLoadAsset(name));
        }

        public static T ExplorerLoadAsset(string name)
        {
            try
            {
                // Try to load through the asset loader.
                AssetSource source = Engine.AssetLoader.GetSource(name);
                if (source == null)
                {
                    // Try the file system.
                    if (!File.Exists(name)) return default;
                    var file = new T
                    {
                        Name = name
                    };
                    file.Create(File.ReadAllBytes(name));
                    return file;

                    // Not found
                }

                {
                    var file = new T
                    {
                        Name = name
                    };
                    file.Create(source.GetAsset(name));
                    return file;
                }
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Couldn't load asset - {ex}", "FileExplorerTool");
                throw;
            }
        }
    }
}
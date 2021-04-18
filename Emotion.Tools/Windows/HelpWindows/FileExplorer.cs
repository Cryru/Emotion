#region Using

using System;
using System.Collections.Generic;
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

namespace Emotion.Tools.Windows.HelpWindows
{
    public class FileExplorer<T> : ImGuiModal where T : Asset, new()
    {
        public bool UseAssetLoaderCache;

        private Action<T> _fileSelected;
        private string _customFile = "";
        private Task _loadingTask;

        private Tree<string, string> _fileSystem;

        /// <summary>
        /// Create a file explorer dialog.
        /// </summary>
        /// <param name="fileSelected">The callback to receive the selected file.</param>
        /// <param name="useAssetLoaderCache">Whether to load the file from the asset loader cache, if possible.</param>
        public FileExplorer(Action<T> fileSelected, bool useAssetLoaderCache = false) : base($"Pick a [{typeof(T)}]")
        {
            _fileSelected = fileSelected;
            UseAssetLoaderCache = useAssetLoaderCache;
            _fileSystem = FileExplorer.FilesToTree(Engine.AssetLoader.AllAssets);
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
                    T file = await ExplorerLoadAssetAsync(_customFile, UseAssetLoaderCache);
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

            void OnClick(string name)
            {
                _loadingTask = Task.Run(async () =>
                {
                    T file = await ExplorerLoadAssetAsync(name, UseAssetLoaderCache);
                    if (file == null)
                    {
                        _loadingTask = null;
                        return;
                    }

                    _fileSelected?.Invoke(file);
                    Open = false;
                });
            }

            FileExplorer.RenderTree(_fileSystem, 0, OnClick, true);
        }

        public static async Task<T> ExplorerLoadAssetAsync(string name, bool useAssetLoaderCache = false)
        {
            return await Task.Run(() => ExplorerLoadAsset(name, useAssetLoaderCache));
        }

        public static T ExplorerLoadAsset(string name, bool useAssetLoaderCache = false)
        {
            try
            {
                // Load through the asset loader.
                var asset = Engine.AssetLoader.Get<T>(name, useAssetLoaderCache);
                if (asset != null) return asset;

                // Try the file system.
                if (!File.Exists(name)) return default;
                var file = new T
                {
                    Name = name
                };
                file.Create(File.ReadAllBytes(name));
                return file;
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Couldn't load asset - {ex}", "FileExplorerTool");
                throw;
            }
        }

        public override void Update()
        {
        }
    }

    public static class FileExplorer
    {
        public static void CreateNewFileModal(WindowManager manager, string defaultExtension, Func<byte[]> getDefault, Action<string> fileCreated)
        {
            var nameInput = new StringInputModal(fileName =>
            {
                string dirName = AssetLoader.GetDirectoryName(fileName);
                if (dirName == "Assets") fileName = fileName.Replace("Assets/", DebugAssetStore.AssetDevPath);
                if (!fileName.Contains(".")) fileName = fileName + "." + defaultExtension;
                Engine.AssetLoader.Save(getDefault(), fileName);
                fileCreated(fileName);
            }, "Enter Name");
            manager.AddWindow(nameInput);
        }

        public static Tree<string, string> FilesToTree(IEnumerable<string> assets)
        {
            assets = assets.OrderBy(x => x);
            var tree = new Tree<string, string>();
            foreach (string a in assets)
            {
                if (a.Contains('/'))
                {
                    string[] folderPath = a.Split('/')[..^1];
                    tree.Add(folderPath, a);
                }
                else
                {
                    tree.Leaves.Add(a);
                }
            }

            return tree;
        }

        public static void RenderTree(Tree<string, string> tree, int depth, Action<string> onClick, bool skipFold = false)
        {
            // Add branches
            Tree<string, string> current = tree;

            bool open = skipFold || ImGui.TreeNode(string.IsNullOrEmpty(current.Name) ? "/" : current.Name);
            if (!open) return;

            // Render branches.
            foreach (Tree<string, string> b in current.Branches)
            {
                ImGui.PushID(depth);
                RenderTree(b, depth++, onClick);
                ImGui.PopID();
            }

            // Render leaves. (Some LINQ magic here to not render past the clicked button)
            foreach (string name in current.Leaves.Where(name => ImGui.Button(Path.GetFileName(name))))
            {
                // Load the asset custom so the asset loader's caching doesn't get in the way.
                onClick(name);
            }

            if (!skipFold) ImGui.TreePop();
        }
    }
}
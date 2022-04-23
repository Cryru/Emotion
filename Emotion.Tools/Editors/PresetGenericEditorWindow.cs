#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.Tools.DevUI;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

#nullable enable

namespace Emotion.Tools.Editors
{
    public abstract class PresetGenericEditorWindow<T> : ImGuiBaseWindow
    {
        protected XMLAsset<T>? _currentAsset;
        private bool _unsavedChanges;
        private string? _currentFileName;

        protected PresetGenericEditorWindow(string title = "Generic Editor") : base(title)
        {
            _windowFlags |= ImGuiWindowFlags.MenuBar;
        }

        protected override void RenderImGui()
        {
            if (ImGui.BeginMenuBar())
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1f));

                MenuBarButtons();

                if (_currentFileName != null)
                    ImGui.Text($"File: {_currentFileName}");
                else if (_currentAsset != null)
                    ImGui.Text("File: Unsaved File");

                if (_currentFileName != null && Engine.Host is Win32Platform winPlat && ImGui.Button("Show File"))
                {
                    IAssetStore? store = Engine.AssetLoader.GetStore(_currentFileName);
                    string folderRoot = store == null ? DebugAssetStore.AssetDevPath : store.Folder;
                    string fullPath = Path.GetFullPath(Path.Join(folderRoot, _currentFileName));
                    winPlat.OpenFolderAndSelectFile(fullPath);
                }

                if (_unsavedChanges) ImGui.TextColored(new Vector4(1, 0, 0, 1), "Unsaved changes!");
                ImGui.PopStyleColor();

                ImGui.EndMenuBar();
            }
        }

        #region IO

        private void OpenFile()
        {
            var explorer = new FileExplorer<XMLAsset<T>>(asset =>
            {
                if (asset.Content == null)
                {
                    Engine.Log.Warning($"{asset.Name} asset couldn't be loaded. Maybe it is not of type {typeof(T)}.", "Tools");
                    return;
                }

                if (OnFileLoaded(asset))
                {
                    _currentAsset = asset;
                    _currentFileName = asset.Name;
                    _unsavedChanges = false;
                }
            });
            _toolsRoot.AddLegacyWindow(explorer);
        }

        private void NewFile()
        {
            _currentAsset = CreateFile();
            _currentFileName = null;
            _unsavedChanges = false;
        }

        private void SaveFile(bool otherName = false)
        {
            if (!OnFileSaving()) return;

            Debug.Assert(_currentAsset != null);
            if (_currentFileName == null || otherName)
            {
                var nameInput = new StringInputModalWindow(name =>
                {
                    if (!name.Contains(".")) name += ".xml";

                    _currentFileName = name;
                    _currentAsset.Name = name;
                    _currentAsset.Save();
                    _unsavedChanges = false;
                }, "File Path");
                _toolsRoot.AddChild(nameInput);
                return;
            }

            _currentAsset.Save();
            _unsavedChanges = false;
        }

        #endregion

        #region Internal API

        protected void UnsavedChanges()
        {
            _unsavedChanges = true;
        }

        private bool UnsavedChangesCheck(Action followUp)
        {
            if (!_unsavedChanges) return false;

            var yesNo = new YesNoModalWindow("Unsaved Changes", "You have unsaved changes, are you sure?", resp =>
            {
                if (resp) followUp();
            });
            _toolsRoot.AddChild(yesNo);
            return true;
        }

        #endregion

        #region Inherit API

        protected virtual XMLAsset<T> CreateFile()
        {
            var newT = Activator.CreateInstance<T>();
            return XMLAsset<T>.CreateFromContent(newT);
        }

        protected abstract bool OnFileLoaded(XMLAsset<T> file);
        protected abstract bool OnFileSaving();

        protected virtual void MenuBarButtons()
        {
            if (ImGui.Button("New"))
                if (!UnsavedChangesCheck(NewFile))
                    NewFile();

            if (ImGui.Button("Open"))
                if (!UnsavedChangesCheck(OpenFile))
                    OpenFile();
            if (_currentAsset != null && ImGui.Button("Save")) SaveFile();
            if (_currentAsset != null && ImGui.Button("Save As")) SaveFile(true);
        }

        #endregion
    }
}
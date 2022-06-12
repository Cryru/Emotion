#region Using

using System;
using System.Diagnostics;
using System.IO;
using Emotion.Common;
using Emotion.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Tools.EmUI
{
    public abstract class IMBasePresetEditor<T> : IMBaseWindow
    {
        protected XMLAsset<T>? _currentAsset;
        protected T? _currentContent;

        private bool _unsavedChanges;
        private string? _currentFileName;

        protected IMBasePresetEditor() : this($"Editor {typeof(T)}")
        {
        }

        protected IMBasePresetEditor(string title) : base(title)
        {
        }

        protected override void MenuBarButtons(UIBaseWindow headerSecondRow)
        {
            var newButton = new TextCallbackButton("New");
            newButton.Id = "ButtonNew";
            newButton.OnClickedProxy = _ =>
            {
                if (!UnsavedChangesCheck(NewFile)) NewFile();
            };
            headerSecondRow.AddChild(newButton);

            var openButton = new TextCallbackButton("Open");
            openButton.Id = "ButtonOpen";
            openButton.OnClickedProxy = _ =>
            {
                if (!UnsavedChangesCheck(OpenFile)) OpenFile();
            };
            headerSecondRow.AddChild(openButton);

            var saveButton = new TextCallbackButton("Save");
            saveButton.Id = "ButtonSave";
            saveButton.DontTakeSpaceWhenHidden = true;
            saveButton.Visible = false;
            saveButton.OnClickedProxy = _ =>
            {
                Debug.Assert(_currentFileName != null);
                SaveFile(_currentFileName);
            };
            headerSecondRow.AddChild(saveButton);

            var saveAsButton = new TextCallbackButton("Save As");
            saveAsButton.Id = "ButtonSaveAs";
            saveAsButton.DontTakeSpaceWhenHidden = true;
            saveAsButton.Visible = false;
            saveAsButton.OnClickedProxy = _ => { SaveFile(""); };
            headerSecondRow.AddChild(saveAsButton);

            var unsavedChangesLabel = new EditorTextWindow();
            unsavedChangesLabel.Id = "UnsavedChangesLabel";
            unsavedChangesLabel.DontTakeSpaceWhenHidden = true;
            unsavedChangesLabel.Visible = false;
            unsavedChangesLabel.Text = "You have unsaved changes!";
            unsavedChangesLabel.WindowColor = Color.Red;
            headerSecondRow.AddChild(unsavedChangesLabel);

            var currentFile = new EditorTextWindow();
            currentFile.Id = "LabelCurrentFile";
            currentFile.DontTakeSpaceWhenHidden = true;
            currentFile.Visible = false;
            headerSecondRow.AddChild(currentFile);

            var fileView = new TextCallbackButton("Show File");
            fileView.Id = "ButtonViewFile";
            fileView.Visible = false;
            fileView.OnClickedProxy = _ =>
            {
                if (Engine.Host is Win32Platform winPlat)
                {
                    Debug.Assert(_currentFileName != null);
                    IAssetStore? store = Engine.AssetLoader.GetStore(_currentFileName);
                    string folderRoot = store == null ? DebugAssetStore.AssetDevPath : store.Folder;
                    string fullPath = Path.GetFullPath(Path.Join(folderRoot, _currentFileName));
                    winPlat.OpenFolderAndSelectFile(fullPath);
                }
                else
                {
                    Engine.Log.Error("Functionality not implemented for this platform.", MessageSource.Platform);
                }
            };
            headerSecondRow.AddChild(fileView);

            base.MenuBarButtons(headerSecondRow);
        }

        protected override bool UpdateInternal()
        {
            return base.UpdateInternal();
        }

        private bool UnsavedChangesCheck(Action followUp)
        {
            if (!_unsavedChanges) return false;

            var unsavedChangesYesNo = new QuestionModal("Unsaved Changes", "You have unsaved changes, are you sure?", "Yes", "No");
            unsavedChangesYesNo.OnAnswered = resp =>
            {
                if (resp == "Yes") followUp();
            };
            Controller?.AddChild(unsavedChangesYesNo);

            return true;
        }

        public void NewFile()
        {
            XMLAsset<T>? newFile = CreateFile();
            if (newFile == null) return;

            _currentAsset = newFile;
            _currentContent = _currentAsset.Content;
            _currentFileName = null;
            FileStateChanged();
        }

        protected XMLAsset<T>? CreateFile()
        {
            var newT = Activator.CreateInstance<T>();
            return XMLAsset<T>.CreateFromContent(newT);
        }

        public void OpenFile()
        {
            var explorer = new FileChoiceModal<XMLAsset<T>>(asset =>
            {
                if (asset.Content == null)
                {
                    Engine.Log.Warning($"{asset.Name} asset couldn't be loaded. Maybe it is not of type {typeof(T)}.", "Tools");
                    return;
                }

                if (CanOpenFile(asset))
                {
                    _currentAsset = asset;
                    _currentContent = _currentAsset.Content;
                    _currentFileName = null;

                    FileStateChanged();
                }
            });
            Controller?.AddChild(explorer);
        }

        public void SaveFile(string name)
        {
        }

        protected void FileStateChanged()
        {
            _unsavedChanges = false;

            UIBaseWindow? buttonSave = GetWindowById("ButtonSave");
            buttonSave!.Visible = _currentFileName != null;

            UIBaseWindow? buttonSaveAs = GetWindowById("ButtonSaveAs");
            buttonSaveAs!.Visible = _currentAsset != null;

            var labelCurrentFile = (UIText?) GetWindowById("LabelCurrentFile");
            labelCurrentFile!.Visible = _currentAsset != null;
            labelCurrentFile.Text = $"File: {_currentFileName ?? "Unsaved File"}";

            UIBaseWindow? buttonViewFile = GetWindowById("ButtonViewFile");
            buttonViewFile!.Visible = _currentFileName != null;
        }

        protected abstract bool CanOpenFile(XMLAsset<T> file);
        protected abstract bool CanSaveFile(XMLAsset<T> file);
    }
}
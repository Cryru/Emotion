#region Using

using System;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public abstract class EditorBase<T> : ImGuiWindow
    {
        public string Name;
        public T Current;

        public abstract T CreateNew();
        protected abstract T CreateFromFile(ReadOnlyMemory<byte> selectedFileData);
        protected abstract byte[] GetSaveContent();

        public void Save()
        {
            byte[] saveContent = GetSaveContent();
            Engine.AssetLoader.Save(saveContent, Name);
        }

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New"))
                {
                    var nameModal = new StringInputModal(input =>
                    {
                        Current = CreateNew();
                        Name = input;
                    }, "Enter Name");
                    Parent.AddWindow(nameModal);
                }

                if (ImGui.MenuItem("Open"))
                {
                    var explorer = new FileExplorer<OtherAsset>(selectedFile =>
                    {
                        if (selectedFile == null || selectedFile.Content.IsEmpty) return;
                        Current = CreateFromFile(selectedFile.Content);
                        Name = selectedFile.Name;
                    });
                    Parent.AddWindow(explorer);
                }

                if (ImGui.MenuItem("Save", Current != null)) Save();

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();

            ImGui.Text($"Opened File: {Name}");
            ImGui.NewLine();
        }
    }
}
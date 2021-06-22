#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.Utility;
using ImGuiNET;

#endregion

#nullable enable

namespace Emotion.Tools.Windows
{
    public abstract class PresetGenericEditor<T> : ImGuiWindow where T : new()
    {
        protected XMLAsset<T>? _currentAsset;
        private bool _unsavedChanges;
        private string? _currentFileName;

        protected PresetGenericEditor(string title) : base(title)
        {
            MenuBar = true;
        }

        protected override void RenderContent(RenderComposer c)
        {
            if (ImGui.BeginMenuBar())
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                if (ImGui.Button("New"))
                {
                    if (UnsavedChangesCheck(NewFile)) return;
                    NewFile();
                }

                if (ImGui.Button("Open"))
                {
                    if (UnsavedChangesCheck(OpenFile)) return;
                    OpenFile();
                }

                if (ImGui.Button("Save")) SaveFile();
                if (_currentFileName != null)
                    ImGui.Text($"File: {_currentFileName}");
                else if (_currentAsset != null)
                    ImGui.Text("File: Unsaved File");
                if (_unsavedChanges) ImGui.TextColored(new Vector4(1, 0, 0, 1), "Unsaved changes!");
                ImGui.PopStyleColor();

                ImGui.EndMenuBar();
            }
        }

        #region Meta Editor

        protected object? TransformClass(object window, Type newType)
        {
            var oldTypeHandler = (XMLComplexBaseTypeHandler?)XMLHelpers.GetTypeHandler(window.GetType());
            var newTypeHandler = (XMLComplexBaseTypeHandler?)XMLHelpers.GetTypeHandler(newType);
            if (oldTypeHandler == null || newTypeHandler == null) return null;

            object? newTypeObject = Activator.CreateInstance(newType, true);
            Debug.Assert(newTypeObject != null);

            IEnumerator<XMLFieldHandler>? oldFields = oldTypeHandler.EnumFields();

            while (oldFields.MoveNext())
            {
                XMLFieldHandler oldField = oldFields.Current;

                XMLFieldHandler? newFieldsMatch = null;
                IEnumerator<XMLFieldHandler>? newFields = newTypeHandler.EnumFields();
                while (newFields.MoveNext())
                {
                    XMLFieldHandler newField = newFields.Current;
                    if (newField.Name != oldField.Name || newField.TypeHandler != oldField.TypeHandler) continue;
                    newFieldsMatch = newField;
                    break;
                }

                if (newFieldsMatch == null) continue;
                object value = oldField.ReflectionInfo.GetValue(window);
                newFieldsMatch.ReflectionInfo.SetValue(newTypeObject, value);
            }

            return newTypeObject;
        }

        protected void ImGuiEditorForType(T obj, XMLFieldHandler xmlHandler)
        {
            XMLTypeHandler? typeHandler = xmlHandler.TypeHandler;
            object value = xmlHandler.ReflectionInfo.GetValue(obj);
            switch (typeHandler)
            {
                case XMLStringTypeHandler:
                    {
                        string stringValue = (string)(value ?? "");
                        if (ImGui.InputText(xmlHandler.Name, ref stringValue, 1000))
                        {
                            xmlHandler.ReflectionInfo.SetValue(obj, stringValue);
                            UnsavedChanges();
                        }
                        return;
                    }
                case XMLPrimitiveTypeHandler primitive:
                {

                    return;
                }
            }

            ImGui.Text($"{xmlHandler.Name}: {value}");
        }

        #endregion

        #region IO

        private void OpenFile()
        {
            var explorer = new FileExplorer<XMLAsset<T>>(asset =>
            {
                if (OnFileLoaded(asset))
                {
                    _currentAsset = asset;
                    _currentFileName = asset.Name;
                    _unsavedChanges = false;
                }
            });
            Parent.AddWindow(explorer);
        }

        private void NewFile()
        {
            _currentAsset = CreateFile();
            _currentFileName = null;
            _unsavedChanges = false;
        }

        private void SaveFile()
        {
            if (_currentFileName == null)
            {
                var nameInput = new StringInputModal(name =>
                {
                    if (!name.Contains(".")) name += ".xml";

                    _currentFileName = name;
                    SaveFile();
                }, "File Path");
                Parent.AddWindow(nameInput);
                return;
            }

            byte[] bytes = GetByteFile();
            Engine.AssetLoader.Save(bytes, _currentFileName);
        }

        #endregion

        #region Helpers

        protected void UnsavedChanges()
        {
            _unsavedChanges = true;
        }

        private bool UnsavedChangesCheck(Action followUp)
        {
            if (_unsavedChanges)
            {
                var yesNo = new YesNoModal(resp =>
                {
                    if (resp) followUp();
                }, "Unsaved Changes", "You have unsaved changes, are you sure?");
                Parent.AddWindow(yesNo);
                return true;
            }

            return false;
        }

        public List<Type> GetTypesWhichInherit()
        {
            List<Type> inheritors = new();
            Type type = typeof(T);
            foreach (var assembly in Helpers.AssociatedAssemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type assemblyType in types)
                {
                    if (type.IsAssignableFrom(assemblyType)) inheritors.Add(assemblyType);
                }
            }

            return inheritors;
        }

        #endregion

        #region Inherit API

        protected virtual byte[] GetByteFile()
        {
            if (_currentAsset == null) return Array.Empty<byte>();
            string xml = XMLFormat.To(_currentAsset.Content);
            return Encoding.Default.GetBytes(xml);
        }

        protected virtual XMLAsset<T> CreateFile()
        {
            return XMLAsset<T>.CreateFromContent(new T());
        }

        protected abstract bool OnFileLoaded(XMLAsset<T> file);

        #endregion
    }
}
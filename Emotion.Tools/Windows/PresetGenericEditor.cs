#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.Utility;
using ImGuiNET;

#endregion

#nullable enable

namespace Emotion.Tools.Windows
{
    public abstract class PresetGenericEditor<T> : ImGuiWindow
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
                    if (!UnsavedChangesCheck(NewFile))
                        NewFile();

                if (ImGui.Button("Open"))
                    if (!UnsavedChangesCheck(OpenFile))
                        OpenFile();

                if (_currentAsset != null && ImGui.Button("Save")) SaveFile();
                if (_currentAsset != null && ImGui.Button("Save As")) SaveFile(true);
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

                        break;
                    }
                case XMLEnumTypeHandler enumHandler:
                    {
                        string[] enumValueNames = Enum.GetNames(enumHandler.Type);
                        int currentItem = enumValueNames.IndexOf(value.ToString());
                        if (ImGui.Combo(xmlHandler.Name, ref currentItem, enumValueNames, enumValueNames.Length))
                        {
                            value = Enum.Parse(enumHandler.Type, enumValueNames[currentItem]);
                            xmlHandler.ReflectionInfo.SetValue(obj, value);
                            UnsavedChanges();
                        }

                        break;
                    }
                case XMLPrimitiveTypeHandler primitive:
                    {
                        if (primitive.Type == typeof(int))
                        {
                            var intValue = (int)(value ?? 0);
                            if (ImGui.InputInt(xmlHandler.Name, ref intValue))
                            {
                                xmlHandler.ReflectionInfo.SetValue(obj, intValue);
                                UnsavedChanges();
                            }

                            break;
                        }

                        if (primitive.Type == typeof(bool))
                        {
                            var boolValue = (bool)(value ?? false);
                            if (ImGui.Checkbox(xmlHandler.Name, ref boolValue))
                            {
                                xmlHandler.ReflectionInfo.SetValue(obj, boolValue);
                                UnsavedChanges();
                            }

                            break;
                        }

                        if (primitive.Type == typeof(float))
                        {
                            var floatVal = (float) value;
                            if (ImGui.InputFloat(xmlHandler.Name, ref floatVal))
                            {
                                xmlHandler.ReflectionInfo.SetValue(obj, floatVal);
                                UnsavedChanges();
                            }

                            break;
                        }

                        break;
                    }
                case XMLComplexValueTypeHandler valueType:
                    {

                        if (valueType.Type == typeof(Vector2))
                        {
                            var vec2Value = (Vector2)(value ?? Vector2.Zero);
                            if (ImGui.InputFloat2(xmlHandler.Name, ref vec2Value))
                            {
                                xmlHandler.ReflectionInfo.SetValue(obj, vec2Value);
                                UnsavedChanges();
                            }

                            break;
                        }

                        if (valueType.Type == typeof(Vector3))
                        {
                            var vec3Value = (Vector3)(value ?? Vector3.Zero);
                            if (ImGui.InputFloat3(xmlHandler.Name, ref vec3Value))
                            {
                                xmlHandler.ReflectionInfo.SetValue(obj, vec3Value);
                                UnsavedChanges();
                            }

                            break;
                        }

                        if (valueType.Type == typeof(Color))
                        {
                            var colorValue = (Color)(value ?? Color.White);
                            Vector4 colorAsVec4 = colorValue.ToVec4();
                            ImGui.Text(xmlHandler.Name);
                            ImGui.SameLine();
                            ImGui.Text($"(RGBA) {colorValue}");
                            ImGui.SameLine();

                            if (ImGui.ColorButton(xmlHandler.Name, colorAsVec4)) ImGui.OpenPopup(xmlHandler.Name);
                            if (ImGui.BeginPopup(xmlHandler.Name))
                            {
                                if (ImGui.ColorPicker4($"Edit: {xmlHandler.Name}", ref colorAsVec4))
                                {
                                    colorValue = new Color(colorAsVec4);
                                    xmlHandler.ReflectionInfo.SetValue(obj, colorValue);
                                    UnsavedChanges();
                                }

                                ImGui.EndPopup();
                            }

                            break;
                        }

                        if (valueType.Type == typeof(Rectangle))
                        {
                            var rectValue = (Rectangle)(value ?? Rectangle.Empty);
                            var vec4Value = new Vector4(rectValue.X, rectValue.Y, rectValue.Width, rectValue.Height);
                            if (ImGui.InputFloat4(xmlHandler.Name, ref vec4Value))
                            {
                                var r = new Rectangle(vec4Value.X, vec4Value.Y, vec4Value.Z, vec4Value.W);
                                xmlHandler.ReflectionInfo.SetValue(obj, r);
                                UnsavedChanges();
                            }
                        }

                        break;
                    }
                default:
                    {
                        ImGui.Text($"{xmlHandler.Name}: {value}");
                        break;
                    }
            }

            object defaultVal = xmlHandler.DefaultValue;
            bool valueIsNotDefault = value != null && !value.Equals(defaultVal);
            bool defaultIsNotValue = defaultVal != null && !defaultVal.Equals(value);
            if (valueIsNotDefault || defaultIsNotValue)
            {
                ImGui.PushID(xmlHandler.Name);
                ImGui.SameLine();
                if (ImGui.SmallButton("X"))
                {
                    xmlHandler.ReflectionInfo.SetValue(obj, defaultVal);
                    UnsavedChanges();
                }

                ImGui.PopID();
            }
        }

        #endregion

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
            Parent.AddWindow(explorer);
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
                var nameInput = new StringInputModal(name =>
                {
                    if (!name.Contains(".")) name += ".xml";

                    _currentFileName = name;
                    _currentAsset.Name = name;
                    _currentAsset.Save();
                    _unsavedChanges = false;
                }, "File Path");
                Parent.AddWindow(nameInput);
                return;
            }

            _currentAsset.Save();
            _unsavedChanges = false;
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

        protected virtual XMLAsset<T> CreateFile()
        {
            var newT = Activator.CreateInstance<T>();
            return XMLAsset<T>.CreateFromContent(newT);
        }

        protected abstract bool OnFileLoaded(XMLAsset<T> file);
        protected abstract bool OnFileSaving();

        #endregion
    }
}
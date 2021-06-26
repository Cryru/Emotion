#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Animation;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.Tools.Windows.UIEdit;
using Emotion.UI;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class UIEditor : PresetGenericEditor<UIBaseWindow>
    {
        private UIBaseWindow _selectedWindow;
        private UIController _ui = new DebugUIController();
        private List<Type> _validWindowTypes;
        private string[] _validWindowTypesNames;
        private XMLComplexBaseTypeHandler _typeHandler;

        /// <summary>
        /// Blackboard for sub editors spawned by specific fields.
        /// For instance the UV field can be assigned from frame size and other metrics.
        /// </summary>
        private Dictionary<string, object> _subEditorStorage = new();

        public UIEditor() : base("UI Editor")
        {
            _validWindowTypes = GetTypesWhichInherit();
            _validWindowTypes.Remove(typeof(UIController));
            _validWindowTypes.Remove(typeof(DebugUIController));
            _validWindowTypesNames = new string[_validWindowTypes.Count];
            for (var i = 0; i < _validWindowTypes.Count; i++)
            {
                _validWindowTypesNames[i] = _validWindowTypes[i].Name;
            }

            _ui.Margins = new Rectangle(5, 15, 5, 5);
        }

        public override void Update()
        {
            _ui.Update();
        }

        protected override void RenderContent(RenderComposer c)
        {
            base.RenderContent(c);

            if (_currentAsset == null)
            {
                ImGui.Text("No file loaded. Click 'New' to start!");
                return;
            }

            XMLAsset<UIBaseWindow> asset = _currentAsset;
            UIBaseWindow window = asset.Content;

            // Initial selection.
            if (_selectedWindow == null)
            {
                SelectWindow(window);
                _ui.ClearChildren();
                _ui.AddChild(window);
            }

            if (ImGui.Button("Update Preview"))
            {
                _ui.ClearChildren();
                _ui.AddChild(window);
            }

            ImGui.SameLine();
            if (ImGui.Button("Delete Selected"))
            {
                var yesNoModal = new YesNoModal(result =>
                {
                    if (!result) return;
                    _selectedWindow.Parent!.RemoveChild(_selectedWindow);
                    UnsavedChanges();
                    ImGui.CloseCurrentPopup();
                }, "Delete Window", $"Are you sure you want to delete {_selectedWindow}?");
                Parent.AddWindow(yesNoModal);
            }

            ImGui.SameLine();
            if (ImGui.Button("Duplicate Selected"))
            {
                UIBaseWindow newWin =_selectedWindow.Clone();
                _selectedWindow.Parent.AddChild(newWin);
            }

            ImGui.BeginGroup();
            if (ImGui.ArrowButton("Left", ImGuiDir.Left))
            {
                UIBaseWindow parent = _selectedWindow.Parent;
                UIBaseWindow parentParent = parent!.Parent;
                if (parentParent != _ui)
                {
                    parent.RemoveChild(_selectedWindow);
                    parentParent!.AddChild(_selectedWindow);
                }
            }

            ImGui.SameLine();

            if (ImGui.ArrowButton("Right", ImGuiDir.Right))
            {
                UIBaseWindow parent = _selectedWindow.Parent;
                int index = parent!.Children.IndexOf(_selectedWindow);
                if (index != 0)
                {
                    UIBaseWindow prevChild = parent.Children[index - 1];
                    parent.RemoveChild(_selectedWindow);
                    prevChild.AddChild(_selectedWindow);
                }
            }

            ImGui.SameLine();

            if (ImGui.ArrowButton("Up", ImGuiDir.Up))
            {
                UIBaseWindow parent = _selectedWindow.Parent;
                int index = parent!.Children.IndexOf(_selectedWindow);
                if (index != 0)
                {
                    parent.RemoveChild(_selectedWindow);
                    parent.AddChild(_selectedWindow, index - 1);
                }
            }

            ImGui.SameLine();

            if (ImGui.ArrowButton("Down", ImGuiDir.Down))
            {
                UIBaseWindow parent = _selectedWindow.Parent;
                int index = parent!.Children.IndexOf(_selectedWindow);
                if (index != parent!.Children.Count - 1)
                {
                    parent.RemoveChild(_selectedWindow);
                    parent.AddChild(_selectedWindow, index + 1);
                }
            }

            ImGui.BeginChild("Window Tree", new Vector2(400, 500), true, ImGuiWindowFlags.HorizontalScrollbar);
            RenderChildrenTree(window);
            ImGui.EndChild();
            ImGui.EndGroup();
            ImGui.SameLine();

            ImGui.BeginGroup();

            ImGui.BeginTabBar("TabBar");

            if (ImGui.BeginTabItem("Window Properties"))
            {
                ImGui.BeginChild("Window Properties", new Vector2(450, 500), true);
                int currentClass = _validWindowTypes.IndexOf(_selectedWindow.GetType());
                if (ImGui.Combo("Class", ref currentClass, _validWindowTypesNames, _validWindowTypesNames.Length))
                {
                    // Copy children.
                    var children = new List<UIBaseWindow>();
                    if (_selectedWindow.Children != null)
                    {
                        children.AddRange(_selectedWindow.Children);
                        _selectedWindow.ClearChildren();
                    }

                    // Transform class.
                    object newObj = TransformClass(_selectedWindow, _validWindowTypes[currentClass]);
                    if (newObj == null) return;
                    var newWin = (UIBaseWindow) newObj;

                    // Add back children. Should be same order.
                    if (children.Count > 0)
                        for (var i = 0; i < children.Count; i++)
                        {
                            newWin.AddChild(children[i]);
                        }

                    // If we modified the root window then we need to recreate the whole asset.
                    if (_selectedWindow.Parent == _ui)
                    {
                        Debug.Assert(_selectedWindow == window);
                        XMLAsset<UIBaseWindow> newAsset = XMLAsset<UIBaseWindow>.CreateFromContent(newWin);
                        newAsset.Name = _currentAsset!.Name;
                        _currentAsset = newAsset;
                    }

                    // Re-add to parent.
                    int index = _selectedWindow.Parent.Children!.IndexOf(_selectedWindow);
                    _selectedWindow.Parent.RemoveChild(_selectedWindow);
                    _selectedWindow.Parent.AddChild(newWin, index);

                    // Reselect.
                    SelectWindow(newWin);

                    // Query preload.
                    UIController.PreloadUI();
                }

                IEnumerator<XMLFieldHandler> fields = _typeHandler.EnumFields();
                while (fields.MoveNext())
                {
                    XMLFieldHandler field = fields.Current;
                    if (field.Name == "Children") continue;
                    ImGuiEditorForType(_selectedWindow, field!);
                    if (field.Name == "UV")
                    {
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Row/Column"))
                        {
                            ImGui.OpenPopup("UVAssign");
                            _subEditorStorage.Clear();
                            _subEditorStorage.Add("frameSize", Vector2.Zero);
                            _subEditorStorage.Add("spacing", Vector2.Zero);
                            _subEditorStorage.Add("column", 0);
                            _subEditorStorage.Add("row", 0);
                        }

                        var fakeRef = true;
                        if (ImGui.BeginPopupModal("UVAssign", ref fakeRef, ImGuiWindowFlags.AlwaysAutoResize))
                        {
                            var frameSize = (Vector2) _subEditorStorage["frameSize"];
                            ImGui.InputFloat2("FrameSize", ref frameSize);
                            _subEditorStorage["frameSize"] = frameSize;

                            var spacing = (Vector2) _subEditorStorage["spacing"];
                            ImGui.InputFloat2("Spacing", ref spacing);
                            _subEditorStorage["spacing"] = spacing;

                            var row = (int) _subEditorStorage["row"];
                            ImGui.InputInt("Row", ref row);
                            _subEditorStorage["row"] = row;

                            var column = (int) _subEditorStorage["column"];
                            ImGui.InputInt("Column", ref column);
                            _subEditorStorage["column"] = column;

                            if (ImGui.Button("Set UV"))
                            {
                                var textureWnd = (UITexture) _selectedWindow;
                                string textureFileName = textureWnd.TextureFile;
                                var textureAsset = Engine.AssetLoader.Get<TextureAsset>(textureFileName);
                                Rectangle uv = AnimatedTexture.GetGridFrameBounds(textureAsset.Texture.Size, frameSize, spacing, row, column);
                                field.ReflectionInfo.SetValue(_selectedWindow, uv);
                            }

                            ImGui.SameLine();
                            if (ImGui.Button("Close")) ImGui.CloseCurrentPopup();

                            ImGui.EndPopup();
                        }
                    }
                }

                ImGui.EndChild();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Debug Info"))
            {
                ImGui.BeginChild("Debug Properties", new Vector2(450, 500), true, ImGuiWindowFlags.HorizontalScrollbar);

                UIDebugger debugger = _ui.Debugger;
                if (debugger != null)
                {
                    UIDebugNode win = debugger.GetMetricsForWindow(_selectedWindow);
                    if (win != null)
                    {
                        Dictionary<string, object> metrics = win.Metrics;
                        foreach (KeyValuePair<string, object> metric in metrics)
                        {
                            ImGui.Text($"{metric.Key}: {metric.Value}");
                        }
                    }
                    else
                    {
                        ImGui.Text("No metrics for selected window. Try refreshing the preview.");
                    }

                    ImGui.Text($"Final_Position: {_selectedWindow.Position}");
                    ImGui.Text($"Final_Size: {_selectedWindow.Size}");
                }
                else
                {
                    ImGui.Text("Debugger not attached to controller.");
                }

                ImGui.EndChild();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();

            ImGui.EndGroup();

            _ui.Render(c);
        }

        private void SelectWindow(UIBaseWindow window)
        {
            _selectedWindow = window;
            _typeHandler = (XMLComplexBaseTypeHandler) XMLHelpers.GetTypeHandler(_selectedWindow.GetType());
        }

        protected void RenderChildrenTree(UIBaseWindow window, int idIncrement = 0)
        {
            ImGui.PushID($"WindowDepth{idIncrement}");
            var flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (window.Children == null || window.Children.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
            if (_selectedWindow == window) flags |= ImGuiTreeNodeFlags.Selected;
            if (window.Children != null && window.Children.IndexOf(_selectedWindow) != -1) ImGui.SetNextItemOpen(true);

            bool opened = ImGui.TreeNodeEx(window.ToString(), flags);

            if (ImGui.IsItemClicked()) SelectWindow(window);
            ImGui.SameLine();

            if (ImGui.SmallButton("Add Child"))
            {
                var newWindow = new UIBaseWindow();
                window.AddChild(newWindow);
                SelectWindow(newWindow);
                UnsavedChanges();
            }

            if (opened)
            {
                if (window.Children != null)
                    for (var i = 0; i < window.Children.Count; i++)
                    {
                        UIBaseWindow child = window.Children[i];
                        ImGui.PushID(i);
                        RenderChildrenTree(child, idIncrement + 1);
                        ImGui.PopID();
                    }

                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        protected override bool OnFileLoaded(XMLAsset<UIBaseWindow> file)
        {
            _selectedWindow = null;
            return true;
        }
    }
}
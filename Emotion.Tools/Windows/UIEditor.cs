#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
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
        private SortedList<string, XMLFieldHandler> _currentWindowHandlers;
        private bool _readonlyWindow;
        

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

        public override void Dispose()
        {
            _ui.Dispose();
            base.Dispose();
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
            if (ImGui.Button("Delete Selected") && !_readonlyWindow)
            {
                var yesNoModal = new YesNoModal(result =>
                {
                    if (!result) return;
                    UIBaseWindow parent = _selectedWindow.Parent;
                    parent!.RemoveChild(_selectedWindow);
                    SelectWindow(parent);
                    UnsavedChanges();
                    ImGui.CloseCurrentPopup();
                }, "Delete Window", $"Are you sure you want to delete {_selectedWindow}?");
                Parent.AddWindow(yesNoModal);
            }

            ImGui.SameLine();
            if (ImGui.Button("Duplicate Selected") && !_readonlyWindow)
            {
                UIBaseWindow newWin = _selectedWindow.Clone();
                _selectedWindow.Parent!.AddChild(newWin);
            }

            ImGui.BeginGroup();
            if (ImGui.ArrowButton("Left", ImGuiDir.Left) && !_readonlyWindow)
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

            if (ImGui.ArrowButton("Right", ImGuiDir.Right) && !_readonlyWindow)
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

            if (ImGui.ArrowButton("Up", ImGuiDir.Up) && !_readonlyWindow)
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

            if (ImGui.ArrowButton("Down", ImGuiDir.Down) && !_readonlyWindow)
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
                if (ImGui.Combo("Class", ref currentClass, _validWindowTypesNames, _validWindowTypesNames.Length) && !_readonlyWindow)
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
                    _ui.InvalidatePreload();
                }

                foreach (KeyValuePair<string, XMLFieldHandler> pair in _currentWindowHandlers)
                {
                    XMLFieldHandler field = pair.Value;
                    if (_readonlyWindow)
                        ImGui.Text($"{field.Name}: {field.ReflectionInfo.GetValue(_selectedWindow)}");
                    else
                        ImGuiEditorForType(_selectedWindow, field!);
                }

                ImGui.EndChild();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Calculated Props"))
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

        private void SelectWindow(UIBaseWindow window, bool readOnly = false)
        {
            _selectedWindow = window;
            _readonlyWindow = readOnly;

            _typeHandler = (XMLComplexBaseTypeHandler) XMLHelpers.GetTypeHandler(_selectedWindow.GetType());
            _currentWindowHandlers ??= new SortedList<string, XMLFieldHandler>();
            _currentWindowHandlers.Clear();

            IEnumerator<XMLFieldHandler> fields = _typeHandler.EnumFields();
            while (fields.MoveNext())
            {
                XMLFieldHandler field = fields.Current;
                if (field.Name == "Children") continue;

                _currentWindowHandlers.Add(field.Name, field);
            }
        }

        protected void RenderChildrenTree(UIBaseWindow window, int idIncrement = 0, bool generatedWindow = false)
        {
            ImGui.PushID($"WindowDepth{idIncrement}");
            var flags = ImGuiTreeNodeFlags.OpenOnArrow;
            if (window.Children == null || window.Children.Count == 0) flags |= ImGuiTreeNodeFlags.Leaf;
            if (_selectedWindow == window) flags |= ImGuiTreeNodeFlags.Selected;
            if (window.Children != null && window.Children.IndexOf(_selectedWindow) != -1) ImGui.SetNextItemOpen(true);

            bool generatedWin = generatedWindow || window.CodeGenerated;
            if (generatedWin) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            bool opened = ImGui.TreeNodeEx(window.ToString(), flags);
            if (ImGui.IsItemClicked()) SelectWindow(window, generatedWin);

            if (!generatedWin)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("Add Child"))
                {
                    var newWindow = new UIBaseWindow();
                    window.AddChild(newWindow);
                    SelectWindow(newWindow);
                    UnsavedChanges();
                }

                if (window == _currentAsset!.Content)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Add Parent"))
                    {
                        _ui.RemoveChild(window);
                        var newWindow = new UIBaseWindow();
                        newWindow.AddChild(window);
                        XMLAsset<UIBaseWindow> newAsset = XMLAsset<UIBaseWindow>.CreateFromContent(newWindow);
                        newAsset.Name = _currentAsset!.Name;
                        _currentAsset = newAsset;
                        _ui.AddChild(newWindow);
                        SelectWindow(window);
                        UnsavedChanges();
                    }
                }

                if (window.Parent == _currentAsset!.Content)
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Replace Asset Root"))
                    {
                        UIBaseWindow root = _currentAsset!.Content;
                        var yesNoModal = new YesNoModal(result =>
                        {
                            if (!result) return;
                            _ui.RemoveChild(root);
                            var rootChildrenThatArentMe = new List<UIBaseWindow>();
                            for (var i = 0; i < root!.Children.Count; i++)
                            {
                                UIBaseWindow child = root.Children[i];
                                if (child != window) rootChildrenThatArentMe.Add(child);
                            }

                            for (var i = 0; i < rootChildrenThatArentMe.Count; i++)
                            {
                                window.AddChild(rootChildrenThatArentMe[i]);
                            }

                            XMLAsset<UIBaseWindow> newAsset = XMLAsset<UIBaseWindow>.CreateFromContent(window);
                            newAsset.Name = _currentAsset!.Name;
                            _currentAsset = newAsset;
                            _ui.AddChild(window);
                            SelectWindow(window);
                            UnsavedChanges();
                        }, "Delete Window", $"Are you sure you want to delete {root}?");
                        Parent.AddWindow(yesNoModal);
                    }
                }
            }

            if (opened)
            {
                if (window.Children != null)
                    for (var i = 0; i < window.Children.Count; i++)
                    {
                        UIBaseWindow child = window.Children[i];
                        ImGui.PushID(i);
                        RenderChildrenTree(child, idIncrement + 1, generatedWin);
                        ImGui.PopID();
                    }

                ImGui.TreePop();
            }

            ImGui.PopID();
            if (generatedWin) ImGui.PopStyleColor();
        }

        protected override bool OnFileLoaded(XMLAsset<UIBaseWindow> file)
        {
            _selectedWindow = null;
            return true;
        }

        protected override bool OnFileSaving()
        {
            _ui.RemoveCodeGeneratedChildren();
            return true;
        }
    }
}
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
using Emotion.Tools.DevUI;
using Emotion.Tools.Windows.UIEdit;
using Emotion.UI;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Editors.UIEditor
{
    public class UIEditorWindow : PresetGenericEditorWindow<UIBaseWindow>
    {
        private UIBaseWindow _selectedWindow;
        private UIController _ui = new DebugUIController();
        private List<Type> _validWindowTypes;
        private string[] _validWindowTypesNames;
        private XMLComplexBaseTypeHandler _typeHandler;
        private List<DeclaredTypeFieldHandlers> _currentWindowHandlers;
        private bool _readonlyWindow;

        public UIEditorWindow() : base("UI Editor")
        {
            // Setup valid window types.
            _validWindowTypes = EditorHelpers.GetTypesWhichInherit<UIBaseWindow>();
            _validWindowTypes.Remove(typeof(UIController));
            _validWindowTypes.Remove(typeof(DebugUIController));
            _validWindowTypesNames = new string[_validWindowTypes.Count];
            for (var i = 0; i < _validWindowTypes.Count; i++)
            {
                Type type = _validWindowTypes[i];
                bool invalid = type.GetConstructor(Type.EmptyTypes) == null;
                _validWindowTypesNames[i] = invalid ? $"{type.Name} (No Constructor)" : type.Name;
            }

            _ui.Margins = new Rectangle(5, 15, 5, 5);
        }

        public override void DetachedFromController(UIController controller)
        {
            base.DetachedFromController(controller);
            _ui.Dispose();
        }

        protected override bool UpdateInternal()
        {
            _ui.Update();
            return true;
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            base.RenderInternal(c);
            _ui.Render(c);
            return true;
        }

        protected override void RenderImGui()
        {
            base.RenderImGui();

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
                var yesNoModal = new YesNoModalWindow("Delete Window", $"Are you sure you want to delete {_selectedWindow}?", result =>
                {
                    if (!result) return;
                    UIBaseWindow parent = _selectedWindow.Parent;
                    parent!.RemoveChild(_selectedWindow);
                    SelectWindow(parent);
                    UnsavedChanges();
                    ImGui.CloseCurrentPopup();
                });
                _toolsRoot.AddChild(yesNoModal);
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
                    if (parent.ZOffset == _selectedWindow.ZOffset)
                    {
                        parentParent.Children!.Remove(_selectedWindow);
                        int idxOfOldParent = parentParent.Children.IndexOf(parent);
                        parentParent.Children!.Insert(idxOfOldParent + 1, _selectedWindow);
                    }
                }
            }

            ImGui.SameLine();

            if (ImGui.ArrowButton("Right", ImGuiDir.Right) && !_readonlyWindow)
            {
                UIBaseWindow parent = _selectedWindow.Parent;
                int index = parent!.Children!.IndexOf(_selectedWindow);
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
                int index = parent!.Children!.IndexOf(_selectedWindow);
                if (index != 0)
                {
                    UIBaseWindow previousWindow = parent!.Children[index - 1];
                    if (previousWindow.ZOffset == _selectedWindow.ZOffset)
                    {
                        parent.Children[index] = previousWindow;
                        parent.Children[index - 1] = _selectedWindow;
                    }
                    else
                    {
                        Parent!.AddChild(new ImGuiMessageBox("Cant Move", "The selected window has a different ZOffset value to the window above it, so it cannot be moved up."));
                    }
                }
            }

            ImGui.SameLine();

            if (ImGui.ArrowButton("Down", ImGuiDir.Down) && !_readonlyWindow)
            {
                UIBaseWindow parent = _selectedWindow.Parent;
                int index = parent!.Children!.IndexOf(_selectedWindow);
                if (index != parent!.Children.Count - 1)
                {
                    UIBaseWindow nextWindow = parent!.Children[index + 1];
                    if (nextWindow.ZOffset == _selectedWindow.ZOffset)
                    {
                        parent.Children[index] = nextWindow;
                        parent.Children[index + 1] = _selectedWindow;
                    }
                    else
                    {
                        Parent!.AddChild(new ImGuiMessageBox("Cant Move", "The selected window has a different ZOffset value to the window below it, so it cannot be moved down."));
                    }
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
                    string name = _validWindowTypesNames[currentClass];
                    if (name.Contains("(No Constructor)")) return;

                    // Copy children.
                    var children = new List<UIBaseWindow>();
                    if (_selectedWindow.Children != null)
                    {
                        children.AddRange(_selectedWindow.Children);
                        _selectedWindow.ClearChildren();
                    }

                    // Transform class.
                    object newObj = EditorHelpers.TransformClass(_selectedWindow, _validWindowTypes[currentClass]);
                    if (newObj == null) return;
                    var newWin = (UIBaseWindow) newObj;

                    // Add back children. Should be same order.
                    if (children.Count > 0)
                        for (var i = 0; i < children.Count; i++)
                        {
                            UIBaseWindow child = children[i];
                            if (child.CodeGenerated) continue;
                            newWin.AddChild(child);
                        }

                    // If we modified the root window then we need to recreate the whole asset.
                    if (_selectedWindow.Parent == _ui)
                    {
                        Debug.Assert(_selectedWindow == window);
                        XMLAsset<UIBaseWindow> newAsset = XMLAsset<UIBaseWindow>.CreateFromContent(newWin);
                        newAsset.Name = _currentAsset!.Name;
                        _currentAsset = newAsset;
                    }

                    // Re-add to parent. HACK
                    int index = _selectedWindow!.Parent!.Children!.IndexOf(_selectedWindow);
                    _selectedWindow.Parent.RemoveChild(_selectedWindow, false);
                    _selectedWindow.Parent.AddChild(newWin);
                    _selectedWindow.Parent!.Children.Remove(newWin);
                    _selectedWindow.Parent!.Children[index] = newWin;

                    // Reselect.
                    SelectWindow(newWin);

                    // Query preload.
                    _ui.InvalidatePreload();
                }

                foreach (DeclaredTypeFieldHandlers typeCollection in _currentWindowHandlers)
                {
                    ImGui.NewLine();
                    EditorHelpers.CenteredText(typeCollection.DeclaringType.ToString());
                    ImGui.NewLine();

                    List<XMLFieldHandler> fields = typeCollection.Fields;
                    for (var i = 0; i < fields.Count; i++)
                    {
                        XMLFieldHandler field = fields[i];

                        if (_readonlyWindow)
                        {
                            ImGui.Text($"{field.Name}: {field.ReflectionInfo.GetValue(_selectedWindow)}");
                        }
                        else
                        {
                            if (EditorHelpers.ImGuiEditorForType(_selectedWindow, field)) UnsavedChanges();
                        }
                    }
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
        }

        // todo: move this to generic
        private class DeclaredTypeFieldHandlers
        {
            public Type DeclaringType;
            public List<XMLFieldHandler> Fields = new();

            public DeclaredTypeFieldHandlers(Type t)
            {
                DeclaringType = t;
            }
        }

        private void SelectWindow(UIBaseWindow window, bool readOnly = false)
        {
            _selectedWindow = window;
            _readonlyWindow = readOnly;

            _typeHandler = (XMLComplexBaseTypeHandler) XMLHelpers.GetTypeHandler(_selectedWindow.GetType());
            _currentWindowHandlers ??= new();
            _currentWindowHandlers.Clear();

            if (_typeHandler == null) return;

            // Collect type handlers sorted by declared type.
            IEnumerator<XMLFieldHandler> fields = _typeHandler.EnumFields();
            while (fields.MoveNext())
            {
                XMLFieldHandler field = fields.Current;
                if (field == null || field.Name == "Children") continue;

                DeclaredTypeFieldHandlers handlerMatch = null;
                for (var i = 0; i < _currentWindowHandlers.Count; i++)
                {
                    DeclaredTypeFieldHandlers handler = _currentWindowHandlers[i];
                    if (handler.DeclaringType == field.ReflectionInfo.DeclaredIn)
                    {
                        handlerMatch = handler;
                        break;
                    }
                }

                if (handlerMatch == null)
                {
                    handlerMatch = new DeclaredTypeFieldHandlers(field.ReflectionInfo.DeclaredIn);
                    _currentWindowHandlers.Add(handlerMatch);
                }

                handlerMatch.Fields.Add(field);
            }

            // Sort by inheritance.
            var indices = new int[_currentWindowHandlers.Count];
            var idx = 0;
            Type t = _typeHandler.Type;
            while (t != typeof(object))
            {
                for (var i = 0; i < _currentWindowHandlers.Count; i++)
                {
                    DeclaredTypeFieldHandlers handler = _currentWindowHandlers[i];
                    if (handler.DeclaringType != t) continue;
                    indices[i] = idx;
                    idx++;
                    break;
                }

                t = t!.BaseType;
            }

            _currentWindowHandlers.Sort((x, y) =>
            {
                int idxX = _currentWindowHandlers.IndexOf(x);
                int idxY = _currentWindowHandlers.IndexOf(y);
                return indices[idxY] -  indices[idxX];
            });
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
                        var yesNoModal = new YesNoModalWindow("Delete Window", $"Are you sure you want to delete {root}?",
                            result =>
                            {
                                if (!result) return;
                                _ui.RemoveChild(root);
                                var rootChildrenThatArentMe = new List<UIBaseWindow>();
                                for (var i = 0; i < root!.Children!.Count; i++)
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
                            });
                        _toolsRoot.AddChild(yesNoModal);
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
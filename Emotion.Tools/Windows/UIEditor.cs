#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Graphics;
using Emotion.IO;
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

            ImGui.BeginGroup();
            if (ImGui.ArrowButton("Left", ImGuiDir.Left))
            {
                UIBaseWindow parent = _selectedWindow.Parent;
                UIBaseWindow? parentParent = parent!.Parent;
                if (parentParent != null)
                {
                    parent.RemoveChild(_selectedWindow);
                    parentParent.AddChild(_selectedWindow);
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

            if (ImGui.Button("Preview"))
            {
                _ui.ClearChildren();
                _ui.AddChild(window);
            }

            ImGui.BeginChild("Window Properties", new Vector2(400, 500), true);
            int currentClass = _validWindowTypes.IndexOf(_selectedWindow.GetType());
            if (ImGui.Combo("Class", ref currentClass, _validWindowTypesNames, _validWindowTypesNames.Length))
            {
                List<UIBaseWindow> children = new List<UIBaseWindow>();
                if (_selectedWindow.Children != null)
                {
                    children.AddRange(_selectedWindow.Children);
                    _selectedWindow.ClearChildren();
                }

                object newObj = TransformClass(_selectedWindow, _validWindowTypes[currentClass]);
                if (newObj == null) return;
                var newWin = (UIBaseWindow) newObj;
                if (children.Count > 0)
                    for (var i = 0; i < children.Count; i++)
                    {
                        newWin.AddChild(children[i]);
                    }

                bool root = _selectedWindow.Parent == _ui;

                int index = _selectedWindow.Parent.Children!.IndexOf(_selectedWindow);
                _selectedWindow.Parent.RemoveChild(_selectedWindow);
                _selectedWindow.Parent.AddChild(newWin, index);


                if (root)
                {
                    // If changing the root, change the whole asset.
                    Debug.Assert(_selectedWindow == window);
                    XMLAsset<UIBaseWindow> newAsset = XMLAsset<UIBaseWindow>.CreateFromContent(newWin);
                    newAsset.Name = _currentAsset!.Name;
                    _currentAsset = newAsset;
                }

                SelectWindow(newWin);
            }

            IEnumerator<XMLFieldHandler> fields = _typeHandler.EnumFields();
            while (fields.MoveNext())
            {
                XMLFieldHandler field = fields.Current;
                if (field.Name == "Children") continue;
                ImGuiEditorForType(_selectedWindow, field!);
            }

            ImGui.EndChild();
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

            if (window.Parent != null && window.Parent != _ui)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("Delete"))
                {
                    var yesNoModal = new YesNoModal(result =>
                    {
                        if (!result) return;
                        window.Parent!.RemoveChild(window);
                        UnsavedChanges();
                        ImGui.CloseCurrentPopup();
                    }, "Delete Window", $"Are you sure you want to delete {window}?");
                    Parent.AddWindow(yesNoModal);
                }
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
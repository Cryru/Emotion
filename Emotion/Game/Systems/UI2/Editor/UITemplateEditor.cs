#nullable enable

using Emotion.Editor;
using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2.Editor;

public class EditorTreeViewWindow<T> : UIBaseWindow
{
    public T? SelectedObject;

    public Action<T?>? OnObjectSelected;

    private UIBaseWindow _container;
    private Action<T, List<T>> _treeWalk;

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    private Dictionary<T, EditorButton> _objectToButton = new();
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

    public EditorTreeViewWindow(Action<T, List<T>> treeWalk)
    {
        _treeWalk = treeWalk;

        var container = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(5)
            }
        };
        AddChild(container);
        _container = container;
    }

    protected override void OnClose()
    {
        base.OnClose();
        EngineEditor.UnregisterForObjectChanges(this);
    }

    public void SetObject(T? obj)
    {
        _container.ClearChildren();
        _objectToButton.Clear();
        SelectedObject = default;

        if (obj == null) return;
        RecursiveWalk(_container, 0, obj);

        // Respawn the tree view if the object changes
        EngineEditor.RegisterForObjectChanges(obj, (change) =>
        {
            _container.ClearChildren();
            _objectToButton.Clear();
            RecursiveWalk(_container, 0, obj);
            SelectObject(SelectedObject, false);
        }, this);
    }

    public void SelectObject(T? obj, bool notify = true)
    {
        SelectedObject = obj;

        foreach (KeyValuePair<T, EditorButton> item in _objectToButton)
        {
            item.Value.SetActiveMode(false);
        }

        if (obj != null && _objectToButton.TryGetValue(obj, out EditorButton? button))
            button.SetActiveMode(true);

        if (notify)
            OnObjectSelected?.Invoke(obj);
    }

    private void RecursiveWalk(UIBaseWindow parent, int indent, T obj)
    {
        var currentButtonContainer = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(3)
            }
        };
        parent.AddChild(currentButtonContainer);

        var currentButton = new EditorButton($"{obj}")
        {
            Layout =
            {
                Margins = new UISpacing(indent, 0, 0, 0)
            },
            OnClickedProxy = (_) => SelectObject(obj)
        };
        currentButtonContainer.AddChild(currentButton);
        _objectToButton.Add(obj, currentButton);

        var currentButtonChildren = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(5)
            }
        };
        currentButtonContainer.AddChild(currentButtonChildren);

        List<T> currentChildren = new List<T>();
        _treeWalk(obj, currentChildren);
        foreach (T? child in currentChildren)
        {
            if (child == null) continue;
            RecursiveWalk(currentButtonChildren, indent + 5, child);
        }
    }
}

public class UITemplateEditor : TypeEditor
{
    private UIBaseWindow? _selectedWindow;

    private O_UITemplate? _objectEditing = null;
    private ComplexObjectEditor<O_UITemplate> _objEditor;
    private ObjectPropertyWindow _windowEditor;
    private UIBaseWindow _viewPort;
    private EditorTreeViewWindow<UIBaseWindow>? _treeView;

    public UITemplateEditor()
    {
        var contentPanel = new UIBaseWindow
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(0) //  LayoutMode.HorizontalEditorPanel
            }
        };
        AddChild(contentPanel);

        var viewPort = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.CornflowerBlue
            }
        };
        _viewPort = viewPort;

        var leftSideTabs = new EditorTabbedContent();
        leftSideTabs.AddTab("Visual", (tabParent) =>
        {
            tabParent.AddChild(_viewPort);
        });
        leftSideTabs.AddTab("Tree View", (tabParent) =>
        {
            var treeViewContainer = new UIBaseWindow();
            tabParent.AddChild(treeViewContainer);

            var treeViewScrollContent = new EditorScrollArea();
            treeViewContainer.AddChild(treeViewScrollContent);

            var treeView = new EditorTreeViewWindow<UIBaseWindow>((obj, list) => list.AddRange(obj.Children));
            if (_objectEditing != null)
                treeView.SetObject(_objectEditing.Window);
            treeView.SelectObject(_selectedWindow, false);
            treeViewScrollContent.AddChildInside(treeView);
            treeView.OnObjectSelected = SelectSubWindow;
            _treeView = treeView;

            var scrollVert = new EditorScrollBar();
            treeViewContainer.AddChild(scrollVert);
            scrollVert.ScrollParent = treeViewScrollContent;
        });
        leftSideTabs.SetTab("Visual");
        contentPanel.AddChild(leftSideTabs);

        contentPanel.AddChild(new HorizontalPanelSeparator()
        {
            SeparationPercent = 0.6f
        });

        var right = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(0)
            }
        };
        contentPanel.AddChild(right);

        var meEditor = new ComplexObjectEditor<O_UITemplate>();
        meEditor.Layout.SizingY = UISizing.Fixed(150);
        right.AddChild(meEditor);
        _objEditor = meEditor;

        var windowEditor = new ObjectPropertyWindow();
        right.AddChild(windowEditor);
        _windowEditor = windowEditor;
    }

    protected override void OnClose()
    {
        base.OnClose();
        SetValue(null);
    }

    public override void SetValue(object? value)
    {
        SelectSubWindow(null);
        if (_objectEditing != null)
            EngineEditor.UnregisterForObjectChanges(_objectEditing);

        if (value is O_UITemplate template)
            _objectEditing = template;
        else
            _objectEditing = null;

        _viewPort.ClearChildren();

        if (_objectEditing != null)
        {
            SelectSubWindow(_objectEditing.Window);

            _objEditor.SetValue(_objectEditing);
            _treeView?.SetObject(_objectEditing.Window);
            _viewPort.ClearChildren();
            _viewPort.AddChild(_objectEditing.Window);
        }
        else
        {
            _treeView?.SetObject(null);
            _objEditor.SetValue(null);
            _viewPort.ClearChildren();
        }
    }

    public void SelectSubWindow(UIBaseWindow? win)
    {
        _selectedWindow = win;
        _windowEditor.SetEditor(_selectedWindow);
        if (_selectedWindow != null)
        {
            AssertNotNull(_objectEditing);
            _windowEditor.HACK_SetActualPageRoot(_objectEditing);
        }
        _treeView?.SelectObject(_selectedWindow, false);
    }
}
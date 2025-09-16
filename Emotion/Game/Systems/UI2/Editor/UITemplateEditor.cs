#nullable enable

using Emotion.Editor;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2.Editor;

public class O_TempWorkaround : O_UIBaseWindow
{
    public UIBaseWindow ViewportOldSystem = null!;

    public O_TempWorkaround()
    {
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
    }

    protected override Vector2 InternalGetWindowMinSize()
    {
        Layout.Offset = ViewportOldSystem.Position2;
        return ViewportOldSystem.Size;
    }
}

public class UITemplateEditor : TypeEditor
{
    private O_UITemplate? _objectEditing = null;
    private ComplexObjectEditor<O_UITemplate> _objEditor;
    private ObjectPropertyWindow _windowEditor;
    private UISolidColor _viewPort;

    public UITemplateEditor()
    {
        var contentPanel = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalEditorPanel
        };
        AddChild(contentPanel);

        var viewPort = new UISolidColor()
        {
            WindowColor = Color.CornflowerBlue
        };
        contentPanel.AddChild(viewPort);
        _viewPort = viewPort;

        contentPanel.AddChild(new HorizontalPanelSeparator()
        {
            SeparationPercent = 0.6f
        });

        var right = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.VerticalList
        };
        contentPanel.AddChild(right);

        var meEditor = new ComplexObjectEditor<O_UITemplate>();
        meEditor.MinSizeY = 150;
        right.AddChild(meEditor);
        _objEditor = meEditor;

        var windowEditor = new ObjectPropertyWindow();
        right.AddChild(windowEditor);
        _windowEditor = windowEditor;
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        SetValue(null);
    }

    public override void SetValue(object? value)
    {
        if (_objectEditing != null)
            EngineEditor.UnregisterForObjectChanges(_objectEditing);

        if (value is O_UITemplate template)
            _objectEditing = template;
        else
            _objectEditing = null;

        _viewPort.ClearChildren();

        if (_objectEditing != null)
        {
            _objEditor.SetValue(_objectEditing);
            _windowEditor.SetEditor(_objectEditing.Window);
            //EngineEditor.RegisterForObjectChanges(_objectEditing.Window, (ev) => _objectEditing.Window.InvalidateLayout());

            // temp
            EngineEditor.EditorUI.ClearChildren();

            var t = new O_TempWorkaround();
            t.Layout.LayoutMode = LayoutMode.HorizontalList;
            t.ViewportOldSystem = _viewPort;
            t.AddChild(_objectEditing.Window);
            EngineEditor.EditorUI.AddChild(t);
            // temp
        }
        else
        {
            _objEditor.SetValue(null);
            _windowEditor.SetEditor(null);
            EngineEditor.EditorUI.ClearChildren();
        }
    }
}
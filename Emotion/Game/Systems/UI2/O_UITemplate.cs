#nullable enable

using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.Tools.InterfaceTool;
using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public class O_UITemplate : GameDataObject
{
    public O_UIBaseWindow Window = new O_UIBaseWindow();
}

public class UITemplateEditor : TypeEditor
{
    private O_UITemplate? _objectEditing = null;
    private ComplexObjectEditor<O_UIBaseWindow> _objEditor;
    private UISolidColor _viewPort;

    public UITemplateEditor()
    {
        //GrowY = false;

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
            SeparationPercent = 0.5f
        });

        var objEditor = new ComplexObjectEditor<O_UIBaseWindow>();
        contentPanel.AddChild(objEditor);
        _objEditor = objEditor;
    }

    public override void SetValue(object? value)
    {
        if (value is O_UITemplate template)
            _objectEditing = template;

        _objEditor.SetValue(_objectEditing?.Window);
        _viewPort.ClearChildren();
        //if (_objectEditing != null)
        //    _viewPort.AddChild(_objectEditing.Window);
    }
}
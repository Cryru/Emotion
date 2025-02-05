using Emotion.Game.World.Editor;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorEditObjectButton : SquareEditorButton
{
    private object? _obj;
    private IGenericReflectorTypeHandler? _typeHandler;

    private UITexture _editorIcon;

    public EditorEditObjectButton() : base()
    {
        UITexture editorIcon = new UITexture()
        {
            TextureFile = "Editor/Edit.png",
            ImageScale = new Vector2(0.75f),
            Smooth = true,
            AnchorAndParentAnchor = UIAnchor.CenterCenter,
            IgnoreParentColor = true
        };
        AddChild(editorIcon);
        _editorIcon = editorIcon;

        Enabled = false; // default - no object set
    }

    public void SetEditor(object? obj)
    {
        _obj = obj;
        _typeHandler = obj != null ? ReflectorEngine.GetTypeHandler(obj.GetType()) : null;
        Enabled = _typeHandler != null;
    }

    protected override void RecalculateButtonColor()
    {
        base.RecalculateButtonColor();
        _editorIcon.WindowColor = Enabled ? MapEditorColorPalette.TextColor : MapEditorColorPalette.TextColor * 0.5f;
        
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        AssertNotNull(_obj);
        AssertNotNull(_typeHandler);

        var objEditWindow = new ObjectPropertyEditorWindow(_obj);
        EngineEditor.EditorRoot.AddChild(objEditWindow);
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        // nop
    }
}

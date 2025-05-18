using Emotion.Game.TwoDee;
using Emotion.IO;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools.SpriteEntityTool;

public class SpriteEntityEditor : TwoSplitEditorWindowFileSupport<UISolidColor, ObjectPropertyWindow, SpriteEntity>
{
    private UIList _list = null!;

    private SpriteEntityMetaState? _entityMetaState;
    private ListEditor<SpriteAnimation>? _animList;
    private SpriteAnimation? _selectedAnim;
    private float _animTime;

    public SpriteEntityEditor() : base("Sprite Entity Editor")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        NewFile();
    }

    protected override UISolidColor GetLeftSideContent()
    {
        var viewPort = new UISolidColor
        {
            IgnoreParentColor = true,
            Id = "Viewport",
            MinSize = new Vector2(50),
            WindowColor = Color.CornflowerBlue
        };

        var proxyRender = new EditorProxyRender()
        {
            OnRender = RenderViewport
        };
        viewPort.AddChild(proxyRender);

        return viewPort;
    }

    protected override ObjectPropertyWindow GetRightSideContent()
    {
        var objProps = new ObjectPropertyWindow()
        {
            Id = "EntityData",
            IgnoreParentColor = true,
            MinSize = new Vector2(50)
        };
        return objProps;
    }

    protected override bool UpdateInternal()
    {
        if (_animList != null)
            _selectedAnim = _animList.GetSelected();

        _animTime += Engine.DeltaTime;
        _entityMetaState?.UpdateAnimation(_selectedAnim, _animTime);
        return base.UpdateInternal();
    }

    protected void RenderViewport(UIBaseWindow win, RenderComposer c)
    {
        var center = win.Bounds.Center.ToVec3();

        float lineLength = 10;
        c.RenderLine(center + new Vector3(0, lineLength, 0), center - new Vector3(0, lineLength, 0), Color.Red, 3);
        c.RenderLine(center + new Vector3(lineLength, 0, 0), center - new Vector3(lineLength, 0, 0), Color.Red, 3);

        if (ObjectBeingEdited != null)
        {
            AssertNotNull(_entityMetaState);
            c.RenderEntityStandalone(ObjectBeingEdited, _entityMetaState, center, Engine.Renderer.IntScale);
        }
    }

    protected override void CreateTopBarButtons(UIBaseWindow topBar)
    {
        base.CreateTopBarButtons(topBar);

        //EditorButton fileButton = new EditorButton("Animation");
        //fileButton.OnClickedProxy = (me) =>
        //{
        //    UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

        //    {
        //        EditorButton button = new EditorButton("Add Frame To Current (Single Texture)");
        //        button.GrowX = true;
        //        button.OnClickedProxy = (_) =>
        //        {
        //            Controller?.DropDown?.Close();

        //            ObjectPropertyWindow? entityData = _rightContent;
        //            TypeEditor? animEditor = entityData?.GetEditorForProperty(nameof(SpriteEntity.Animations));
        //            if (animEditor is ListEditor<SpriteAnimation> listEditor)
        //            {
        //                SpriteAnimation? selectedAnim = listEditor.GetSelected();
        //                if (selectedAnim != null)
        //                {
        //                    FilePicker<TextureAsset>.SelectFile(this, (file) =>
        //                    {
        //                        if (file == null) return;

        //                    });
        //                }
        //            }
        //        };
        //        dropDown.AddChild(button);
        //    }
        //};
        //topBar.AddChild(fileButton);
    }

    protected override void OnObjectBeingEditedChange(SpriteEntity? newObj)
    {
        EngineEditor.UnregisterForObjectChanges(this);
        if (newObj != null)
            EngineEditor.RegisterForObjectChanges(newObj, (_) => MarkUnsavedChanges(), this);

        if (newObj != null)
            _entityMetaState = new SpriteEntityMetaState(newObj);
        else
            _entityMetaState = null;

        ObjectPropertyWindow? entityData = _rightContent;
        AssertNotNull(entityData);
        entityData.SetEditor(newObj);
        TypeEditor? animEditor = entityData.GetEditorForProperty(nameof(SpriteEntity.Animations));
        if (animEditor is ListEditor<SpriteAnimation> animEdit)
            _animList = animEdit;
    }

    protected override Action<UIBaseWindow, Action<SpriteAsset?>> GetFileOpenFunction()
    {
        return FilePicker<SpriteAsset>.SelectFile;
    }
}
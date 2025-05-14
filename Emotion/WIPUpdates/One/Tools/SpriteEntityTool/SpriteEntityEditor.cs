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

    private Texture _spriteEntityTexture;

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

        return base.UpdateInternal();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        ObjectBeingEdited?.Render(c);

        return base.RenderInternal(c);
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
        ObjectPropertyWindow? entityData = _rightContent;
        entityData?.SetEditor(newObj);
    }
}

//public class SelectTextureModal : EditorWindow
//{
//    public SelectTextureModal() : base("Select Texture")
//    {
//        PanelMode = Editor.EditorHelpers.PanelMode.Modal;
//    }
//}
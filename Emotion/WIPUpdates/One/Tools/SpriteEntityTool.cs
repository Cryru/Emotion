using Emotion.Editor.EditorHelpers;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Helpers;
using Emotion.WIPUpdates.One.Work;
using System.Collections;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public class HorizontalPanelSeparator : UIBaseWindow
{
    public float SeparationPercent = 0.5f;

    public HorizontalPanelSeparator()
    {
        MinSizeX = 10;
    }
}

public class SpriteEntityTool : EditorWindow
{
    private UIList _list = null!;

    public SpriteEntityTool() : base("Sprite Entity Tool")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow mainDiv = new()
        {
            LayoutMode = LayoutMode.VerticalList
        };
        _contentParent.AddChild(mainDiv);

        {
            UIBaseWindow buttonList = new()
            {
                LayoutMode = LayoutMode.HorizontalList,
                Paddings = new Primitives.Rectangle(5, 5, 5, 5)
            };
            mainDiv.AddChild(buttonList);

            EditorButton button = new EditorButton("Import Texture");
            button.OnClickedProxy = (_) =>
            {
                var explorer = new FilePicker<TextureAsset>((file) => NewEntity(file));
                Parent!.AddChild(explorer);
            };
            buttonList.AddChild(button);
        }

        UISolidColor content = new UISolidColor
        {
            IgnoreParentColor = true,
            MinSize = new Vector2(100),
            WindowColor = new Color(0, 0, 0, 50),
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
            LayoutMode = LayoutMode.HorizontalEditorPanel
        };
        mainDiv.AddChild(content);

        UISolidColor viewPort = new UISolidColor
        {
            IgnoreParentColor = true,
            Id = "Viewport",
            MinSize = new Vector2(50),
            WindowColor = Color.CornflowerBlue
        };
        content.AddChild(viewPort);

        content.AddChild(new HorizontalPanelSeparator());
        
        UIBaseWindow contentRight = new()
        {
            IgnoreParentColor = true,
            Id = "EntityData",
            LayoutMode = LayoutMode.VerticalList,
            FillX = false,
            MinSize = new Vector2(50)
        };
        content.AddChild(contentRight);
    }

    protected override bool UpdateInternal()
    {

        return base.UpdateInternal();
    }

    #region UI Helpers

    public void PopulateEntityDataForEntity(SpriteEntity entity)
    {
        var entityData = GetWindowById("EntityData");
        if (entityData == null) return;
        entityData.ClearChildren();

        UIText name = new UIText
        {
            Text = "Name: " + entity.Name,
            FontSize = 20
        };
        entityData.AddChild(name);
    }

    #endregion

    private SpriteEntity SpriteEntity;

    private Texture _spriteEntityTexture;

    public void NewEntity(TextureAsset baseFile)
    {
        SpriteEntity = new SpriteEntity();
        SpriteEntity.SourceFile = baseFile.Name;
        _spriteEntityTexture = baseFile.Texture;

        PopulateEntityDataForEntity(SpriteEntity);
    }
}

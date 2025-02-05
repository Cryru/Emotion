using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Implementation.Win32;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.WIPUpdates.One.Work;
using System.Collections;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public class SpriteEntityTool : EditorWindowFileSupport
{
    private UIList _list = null!;

    private SpriteEntity SpriteEntity;

    private Texture _spriteEntityTexture;

    public SpriteEntityTool() : base("Sprite Entity Tool")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        /*
         EditorButton button = new EditorButton("Import Texture");
            newFile.OnClickedProxy = (_) =>
            {
                var platform = Engine.Host;
                if (platform is DesktopPlatform winPl)
                {
                    winPl.DeveloperMode_SelectFileNative<TextureAsset>((file) =>
                    {
                        bool a = true;
                    });
                }

                //var explorer = new FilePicker<TextureAsset>((file) => NewEntity(file));
                //Parent!.AddChild(explorer);
            };
            buttonList.AddChild(newFile);
         */

        UISolidColor content = new UISolidColor
        {
            IgnoreParentColor = true,
            MinSize = new Vector2(100),
            WindowColor = new Color(0, 0, 0, 50),
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
            LayoutMode = LayoutMode.HorizontalEditorPanel
        };
        contentParent.AddChild(content);

        UISolidColor viewPort = new UISolidColor
        {
            IgnoreParentColor = true,
            Id = "Viewport",
            MinSize = new Vector2(50),
            WindowColor = Color.CornflowerBlue
        };
        content.AddChild(viewPort);

        content.AddChild(new HorizontalPanelSeparator()
        {
            SeparationPercent = 0.6f
        });

        content.AddChild(new ObjectPropertyWindow()
        {
            Id = "EntityData",
            IgnoreParentColor = true,
            FillX = false,
            MinSize = new Vector2(50)
        });
        
        //UIBaseWindow contentRight = new()
        //{
        //    IgnoreParentColor = true,
        //    Id = "EntityData",
        //    LayoutMode = LayoutMode.VerticalList,
        //    FillX = false,
        //    MinSize = new Vector2(50)
        //};
        //content.AddChild(contentRight);

        NewFile();
    }

    protected override bool UpdateInternal()
    {

        return base.UpdateInternal();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        SpriteEntity.Render(c);

        return base.RenderInternal(c);
    }

    protected override void CreateTopBarButtons(UIBaseWindow topBar)
    {
        base.CreateTopBarButtons(topBar);

        EditorButton fileButton = new EditorButton("Image");
        fileButton.OnClickedProxy = (me) =>
        {
            UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

            {
                EditorButton button = new EditorButton("Add");
                button.FillX = true;
                button.OnClickedProxy = (_) =>
                {
                    NewFile();
                    Controller?.DropDown?.Close();
                };
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Open...");
                button.FillX = true;
                button.OnClickedProxy = (_) => OpenFile();
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Save");
                button.FillX = true;
                button.OnClickedProxy = (_) => SaveFile();
                dropDown.AddChild(button);
            }

            //{
            //    EditorButton button = new EditorButton("Save As");
            //    button.FillX = true;
            //    dropDown.AddChild(button);
            //}
        };
        topBar.AddChild(fileButton);
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

    protected override void NewFile()
    {
        base.NewFile();

        SpriteEntity = new SpriteEntity();

        ObjectPropertyWindow? entityData = GetWindowById<ObjectPropertyWindow>("EntityData");
        entityData?.SetEditor(SpriteEntity);
    }

    public void NewEntity(TextureAsset baseFile)
    {
        SpriteEntity = new SpriteEntity();
        SpriteEntity.SourceFile = baseFile.Name;
        _spriteEntityTexture = baseFile.Texture;

        PopulateEntityDataForEntity(SpriteEntity);
    }
}

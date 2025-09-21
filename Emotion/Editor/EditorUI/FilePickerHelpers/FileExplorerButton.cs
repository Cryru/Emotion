#region Using

using System.IO;
using Emotion.Core.Systems.IO;
using Emotion.Game.Systems.UI;

#endregion

namespace Emotion.Editor.EditorUI.FilePickerHelpers;

#nullable enable

public class FileExplorerButton : UICallbackButton
{
    private UIText _label;
    private UISolidColor _bg;
    private UISolidColor _notch;

    private string _extension = "";
    private string _fileName = "";

    public FileExplorerButton()
    {
        LayoutMode = LayoutMode.VerticalList;

        var directoryNotch = new UISolidColor
        {
            WindowColor = EditorColorPalette.ButtonColor,
            MinSizeY = 10,
            MinSizeX = 20,
            Visible = false,
            GrowY = false,
            GrowX = false
        };
        _notch = directoryNotch;
        AddChild(directoryNotch);

        var bg = new UISolidColor
        {
            WindowColor = EditorColorPalette.ButtonColor,
            Name = "buttonBackground",
            MinSizeY = 60,
            MaxSizeY = 60
        };
        _bg = bg;
        AddChild(bg);

        var txt = new UIText
        {
            ParentAnchor = UIAnchor.TopCenter,
            Anchor = UIAnchor.TopCenter,
            WindowColor = EditorColorPalette.TextColor,
            Name = "buttonText",
            FontSize = EditorColorPalette.EditorButtonTextSize - 2,
            IgnoreParentColor = true,
            Margins = new Rectangle(0, 5, 0, 0),
            MaxSizeX = 140
        };
        _label = txt;
        AddChild(txt);

        MinSizeX = 150;
        MaxSizeX = 150;

        GrowX = false;
        GrowY = false;
    }

    public void SetFileName(string fileName)
    {
        _fileName = fileName;
        string extension = Path.GetExtension(fileName);
        _extension = extension;
        _label.Text = AssetLoader.GetFileName(fileName);
        _notch.Visible = false;
        GeneratePreviewUI();
    }

    public void SetDirectory(string dirName)
    {
        _fileName = dirName;
        _extension = dirName;
        _label.Text = "";
        _notch.Visible = true;
        GeneratePreviewUI();
    }

    public override void OnMouseEnter(Vector2 _)
    {
        base.OnMouseEnter(_);
        _bg.WindowColor = EditorColorPalette.ActiveButtonColor;
        _notch.WindowColor = EditorColorPalette.ActiveButtonColor;
    }

    public override void OnMouseLeft(Vector2 _)
    {
        base.OnMouseLeft(_);
        _bg.WindowColor = EditorColorPalette.ButtonColor;
        _notch.WindowColor = EditorColorPalette.ButtonColor;
    }

    private void GeneratePreviewUI()
    {
        UIBaseWindow previewWindow = GeneratePreviewUIInternal(_extension);
        UIBaseWindow? filePreview = GetWindowById("FilePreview");
        if (filePreview != null) filePreview.Parent?.RemoveChild(filePreview);

        previewWindow.Name = "FilePreview";
        UIBaseWindow? bg = GetWindowById("buttonBackground");
        if (bg != null) bg.AddChild(previewWindow);
    }

    private UIBaseWindow GeneratePreviewUIInternal(string extension)
    {
        var bg = GetWindowById("buttonBackground");

        if (_extension == ".em3" || _extension == ".gltf"
#if MORE_MESH_TYPES
                                 || _extension == ".obj" || _extension == ".fbx" || _extension == ".dae"
#endif
           )
        {
            //var obj3DPreview = new UIMeshEntityWindow
            //{
            //    AssetPath = _fileName,
            //    Async = true,
            //    ParentAnchor = UIAnchor.CenterCenter,
            //    Anchor = UIAnchor.CenterCenter
            //};

            //return obj3DPreview;
        }

        if (_extension == ".png" || _extension  == ".bmp" || _extension == ".eib")
        {
            var texturePreview = new UITexture
            {
                TextureFile = _fileName,
                //RenderSize = new Vector2(-200, -100),
                ParentAnchor = UIAnchor.CenterCenter,
                Anchor = UIAnchor.CenterCenter,
                RenderSize = new Vector2(-100, -100)
            };

            return texturePreview;
        }

        // If unknown file type then display the
        // preview as a label of the file extension.

        var ext = new UIText
        {
            ParentAnchor = UIAnchor.CenterCenter,
            Anchor = UIAnchor.CenterCenter,
            WindowColor = EditorColorPalette.TextColor,
            FontSize = EditorColorPalette.EditorButtonTextSize,
            IgnoreParentColor = true,
            Margins = new Rectangle(5, 0, 5, 0),
            Text = _extension
        };
        return ext;
    }
}
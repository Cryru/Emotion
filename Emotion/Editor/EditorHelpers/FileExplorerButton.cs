#region Using

using System.IO;
using System.Threading.Tasks;
using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers;

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
            WindowColor = MapEditorColorPalette.ButtonColor,
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
            WindowColor = MapEditorColorPalette.ButtonColor,
            Id = "buttonBackground",
            MinSizeY = 60,
            MaxSizeY = 60
        };
        _bg = bg;
        AddChild(bg);

        var txt = new UIText
        {
            ParentAnchor = UIAnchor.TopCenter,
            Anchor = UIAnchor.TopCenter,
            WindowColor = MapEditorColorPalette.TextColor,
            Id = "buttonText",
            FontSize = MapEditorColorPalette.EditorButtonTextSize - 2,
            IgnoreParentColor = true,
            Margins = new Primitives.Rectangle(0, 5, 0, 0),
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
        _bg.WindowColor = MapEditorColorPalette.ActiveButtonColor;
        _notch.WindowColor = MapEditorColorPalette.ActiveButtonColor;
    }

    public override void OnMouseLeft(Vector2 _)
    {
        base.OnMouseLeft(_);
        _bg.WindowColor = MapEditorColorPalette.ButtonColor;
        _notch.WindowColor = MapEditorColorPalette.ButtonColor;
    }

    private void GeneratePreviewUI()
    {
        UIBaseWindow previewWindow = GeneratePreviewUIInternal(_extension);
        UIBaseWindow? filePreview = GetWindowById("FilePreview");
        if (filePreview != null) filePreview.Parent?.RemoveChild(filePreview);

        previewWindow.Id = "FilePreview";
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
            var obj3DPreview = new UIMeshEntityWindow
            {
                AssetPath = _fileName,
                Async = true,
                ParentAnchor = UIAnchor.CenterCenter,
                Anchor = UIAnchor.CenterCenter
            };

            return obj3DPreview;
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
            WindowColor = MapEditorColorPalette.TextColor,
            FontSize = MapEditorColorPalette.EditorButtonTextSize,
            IgnoreParentColor = true,
            Margins = new Rectangle(5, 0, 5, 0),
            Text = _extension
        };
        return ext;
    }
}
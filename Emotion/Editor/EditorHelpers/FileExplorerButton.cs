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
        ScaleMode = UIScaleMode.FloatScale;
        LayoutMode = LayoutMode.VerticalList;

        var directoryNotch = new UISolidColor
        {
            WindowColor = MapEditorColorPalette.ButtonColor,
            MaxSizeY = 5,
            MaxSizeX = 10,
            Visible = false
        };
        _notch = directoryNotch;
        AddChild(directoryNotch);

        var bg = new UISolidColor
        {
            WindowColor = MapEditorColorPalette.ButtonColor,
            Margins = new Rectangle(0, 0, 0, 2),
            MaxSizeY = 30,
            Id = "buttonBackground"
        };
        _bg = bg;
        AddChild(bg);

        var txt = new UIText
        {
            ParentAnchor = UIAnchor.BottomCenter,
            Anchor = UIAnchor.BottomCenter,
            ScaleMode = UIScaleMode.FloatScale,
            WindowColor = MapEditorColorPalette.TextColor,
            Id = "buttonText",
            FontFile = "Editor/UbuntuMono-Regular.ttf",
            FontSize = MapEditorColorPalette.EditorButtonTextSize - 2,
            IgnoreParentColor = true
        };
        _label = txt;
        AddChild(txt);

        MinSize = new Vector2(70, 0);
        MaxSize = new Vector2(70, 999);
        StretchY = true;
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
        var previewWindow = GeneratePreviewUIInternal(_extension);
        var filePreview = GetWindowById("FilePreview");
        if (filePreview != null) filePreview.Parent.RemoveChild(filePreview);

        previewWindow.Id = "FilePreview";
        var bg = GetWindowById("buttonBackground");
        bg.AddChild(previewWindow);
    }

    private UIBaseWindow GeneratePreviewUIInternal(string extension)
    {
        var bg = GetWindowById("buttonBackground");

        if (_extension == ".em3" || _extension == ".obj"
#if ASSIMP
                                 || _extension == ".fbx" || _extension == ".gltf" || _extension == ".dae"
#endif
           )
        {
            var obj3DPreview = new UIMeshEntityWindow();
            obj3DPreview.AssetPath = _fileName;
            obj3DPreview.Async = true;
            obj3DPreview.ParentAnchor = UIAnchor.CenterCenter;
            obj3DPreview.Anchor = UIAnchor.CenterCenter;

            return obj3DPreview;
        }
        // If unknown file type then display the
        // preview as a label of the file extension.

        var ext = new UIText
        {
            ParentAnchor = UIAnchor.CenterCenter,
            Anchor = UIAnchor.CenterCenter,
            ScaleMode = UIScaleMode.FloatScale,
            WindowColor = MapEditorColorPalette.TextColor,
            FontFile = "Editor/UbuntuMono-Regular.ttf",
            FontSize = MapEditorColorPalette.EditorButtonTextSize,
            IgnoreParentColor = true,
            Margins = new Rectangle(5, 0, 5, 0),
            Text = _extension
        };
        return ext;
    }
}
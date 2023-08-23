using System.IO;
using Emotion.Game.World.Editor;
using Emotion.IO;
using Emotion.UI;

namespace Emotion.Editor.EditorHelpers;

#nullable enable

public class FileExplorerButton : UICallbackButton
{
    private UIText _label;
    private UISolidColor _bg;
    private UISolidColor _notch;
    private UIText _extension;
    
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
        };
        _bg = bg;
        AddChild(bg);

        var ext = new UIText
        {
            ParentAnchor = UIAnchor.CenterCenter,
            Anchor = UIAnchor.CenterCenter,
            ScaleMode = UIScaleMode.FloatScale,
            WindowColor = MapEditorColorPalette.TextColor,
            FontFile = "Editor/UbuntuMono-Regular.ttf",
            FontSize = MapEditorColorPalette.EditorButtonTextSize,
            IgnoreParentColor = true,
            Margins = new Rectangle(5, 0, 5, 0)
        };
        _extension = ext;
        bg.AddChild(ext);
        
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
        string extension = Path.GetExtension(fileName);
        _extension.Text = extension;
        _label.Text = AssetLoader.GetFileName(fileName);
        _notch.Visible = false;
    }
    
    public void SetDirectory(string dirName)
    {
        _extension.Text = dirName;
        _label.Text = "";
        _notch.Visible = true;
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
}
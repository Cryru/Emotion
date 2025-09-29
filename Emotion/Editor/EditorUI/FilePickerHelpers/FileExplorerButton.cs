#region Using

using System.IO;
using Emotion.Core.Systems.IO;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2.Editor;

#endregion

namespace Emotion.Editor.EditorUI.FilePickerHelpers;

#nullable enable

public class FileExplorerButton : UICallbackButton
{
    private NewUIText _label;
    private UIBaseWindow _bg;
    private UIBaseWindow _notch;

    private string _extension = "";
    private string _fileName = "";

    public FileExplorerButton()
    {
        Layout.LayoutMethod = UILayoutMethod.VerticalList(0);

        UIBaseWindow directoryNotch = new()
        {
            Layout =
            {
                SizingX = UISizing.Fixed(20),
                SizingY = UISizing.Fixed(10),
            },
            Visuals =
            {
                BackgroundColor = EditorColorPalette.ButtonColor,
                Visible = false
            }
        };
        _notch = directoryNotch;
        AddChild(directoryNotch);

        UIBaseWindow bg = new()
        {
            Name = "buttonBackground",
            Layout =
            {
                SizingY = UISizing.Fixed(60)
            },
            Visuals =
            {
                BackgroundColor = EditorColorPalette.ButtonColor,
            }
        };
        _bg = bg;
        AddChild(bg);

        NewUIText txt = new()
        {
            Name = "buttonText",

            TextColor = EditorColorPalette.TextColor,
            FontSize = EditorColorPalette.EditorButtonTextSize - 2,
            IgnoreParentColor = true,

            Layout =
            {
                Margins = new UISpacing(0, 5, 0, 0),
                MaxSizeX = 140,
                AnchorAndParentAnchor = UIAnchor.TopCenter
            }
        };
        _label = txt;
        AddChild(txt);

        Layout.SizingX = UISizing.Fixed(150);
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

        if (_extension == ".png" || _extension == ".bmp" || _extension == ".eib")
        {
            UIPicture texturePreview = new()
            {
                Texture = _fileName,
                //RenderSize = new Vector2(-100, -100)
                Layout =
                {
                    AnchorAndParentAnchor = UIAnchor.CenterCenter
                },
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
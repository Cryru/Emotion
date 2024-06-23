#region Using

using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World2D;
using Emotion.UI;
using System.Threading.Tasks;

#endregion

#nullable enable

namespace Emotion.WIPUpdates.TextUpdate;

public class TextUpdateDemoScene : World2DBaseScene<Map2D>
{
    public override Task LoadAsync()
    {
        TextLayoutEngine.TestNewRenderer = true;
        _editor.EnterEditor();

        //UIRichText testText = new UIRichText();
        ////testText.Text = "The quick <color #c26e2f>brown</> fox <color 255 125 255>jumped</> over the lazy dog.";
        ////testText.Text = "The quick brown fox\njumped over the lazy dog.";
        //testText.Text = "The quick brown fox jumped over the lazy dog.";
        //testText.Anchor = UIAnchor.CenterCenter;
        //testText.ParentAnchor = UIAnchor.CenterCenter;
        //testText.MaxSizeX = 50;
        //_editor.UIController.AddChild(testText);

        UISolidColor container = new UISolidColor();
        container.UseNewLayoutSystem = true;
        container.StretchX = false;
        container.StretchY = false;
        container.Anchor = UIAnchor.CenterCenter;
        container.ParentAnchor = UIAnchor.CenterCenter;
        container.Offset = new Vector2(-200, 0);
        container.WindowColor = new Color("#1a1a1a");
        _editor.UIController!.AddChild(container);

        UITextInput2 textInput = new UITextInput2();
        textInput.Text = "The quick brown fox jumped";
        textInput.MultiLine = true;
        textInput.MaxSizeX = 100;
        textInput.MaxSizeY = 150;
        container.AddChild(textInput);

        return Task.CompletedTask;
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        composer.ClearDepth();
        composer.SetUseViewMatrix(true);

        base.Draw(composer);
    }
}

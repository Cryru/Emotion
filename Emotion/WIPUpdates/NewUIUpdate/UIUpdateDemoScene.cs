#region Using

using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World2D;
using Emotion.UI;
using System.Threading.Tasks;
using Emotion.WIPUpdates.NewUIUpdate;

#endregion

#nullable enable

namespace Emotion.WIPUpdates.NewUIUpdate;

public class UIUpdateDemoScene : Scene_UIUpdate
{
    public override Task LoadAsync()
    {
        UISolidColor container = new UISolidColor();
        container.LayoutMode = LayoutMode.HorizontalList;
        //container.Anchor = UIAnchor.CenterCenter;
        //container.ParentAnchor = UIAnchor.CenterCenter;
        container.WindowColor = new Color("#1a1a1a");
        container.SetChildren = new List<UIBaseWindow>()
        {
            new UIRichText
            {
                Text = "The quick <color #c26e2f>brown</> fox <color 255 125 255>jumped</> over the lazy dog.",
            },
            new UIRichText
            {
                Text = "The quick <color #c26e2f>brown</> fox <color 255 125 255>jumped</> over the lazy dog.",
                AnchorAndParentAnchor = UIAnchor.CenterRight
            }
        };
        UI.AddChild(container);

        return Task.CompletedTask;
    }

    public override void Draw(RenderComposer composer)
    {
        base.Draw(composer);
    }

    protected override void UpdateScene(float dt)
    {

    }

    protected override void RenderScene(RenderComposer c)
    {
        c.SetUseViewMatrix(false);
        c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.CornflowerBlue);
        c.ClearDepth();
        c.SetUseViewMatrix(true);
    }
}

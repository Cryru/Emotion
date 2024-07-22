using Emotion.Scenography;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.NewUIUpdate;

// todo: add to Scenes
// todo: scenes - make current scene current even while loading, add loaded bool
// todo: investigate loading exceptions for the 100th time

public class UISystem : UIController
{
    public Vector2 TargetResolution = new Vector2(1920, 1080);
    public Vector2 TargetDPI = new Vector2(96);

    protected bool _updateScale;

    public UISystem()
    {
        Engine.Host.OnResize += HostResized;
        HostResized(Engine.Renderer.ScreenBuffer.Size);
    }

    private void HostResized(Vector2 size)
    {
        Vector2 scaledTarget = TargetResolution * TargetDPI;
        Vector2 scaledCurrent = size * Engine.Host.GetDPI();

        Scale = scaledCurrent / scaledTarget;
        _updateScale = true;
        Engine.Log.Info($"UI Scale is {Scale}", MessageSource.UI);
    }

    public override void InvalidateLayout()
    {
        _updateScale = true;
        base.InvalidateLayout();
    }

    protected override bool UpdateInternal()
    {
        if (_updateScale) UpdateScaleValues();
        return base.UpdateInternal();
    }

    private void UpdateScaleValues()
    {
        foreach (UIBaseWindow win in this)
        {
            win.Scale = 1f * win.Parent.Scale;
        }
        _updateScale = false;
    }

    protected override void RenderChildren(RenderComposer c)
    {
        c.EnableSpriteBatcher(true);
        base.RenderChildren(c);
        c.EnableSpriteBatcher(false);
    }
}


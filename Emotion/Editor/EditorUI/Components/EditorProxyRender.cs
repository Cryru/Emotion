#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Editor.EditorUI.Components;

[DontSerialize]
public class EditorProxyRender : UIBaseWindow
{
    public Action<UIBaseWindow, Renderer>? OnRender;

    protected override bool RenderInternal(Renderer c)
    {
        OnRender?.Invoke(this, c);
        return base.RenderInternal(c);
    }
}

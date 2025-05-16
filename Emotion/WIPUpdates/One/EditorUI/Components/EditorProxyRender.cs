using Emotion.Common.Serialization;
using Emotion.UI;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Components;

[DontSerialize]
public class EditorProxyRender : UIBaseWindow
{
    public Action<UIBaseWindow, RenderComposer>? OnRender;

    protected override bool RenderInternal(RenderComposer c)
    {
        OnRender?.Invoke(this, c);
        return base.RenderInternal(c);
    }
}

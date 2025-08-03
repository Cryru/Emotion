#nullable enable

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    // todo: merge with children as its set func
    public List<UIBaseWindow> SetChildren
    {
        set
        {
            ClearChildren();
            for (int i = 0; i < value.Count; i++)
            {
                var child = value[i];
                AddChild(child);
            }
        }
    }
}
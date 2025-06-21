using Emotion.Common.Serialization;
using Emotion.IO;

#nullable enable

namespace Emotion.WIPUpdates.One.Work;

public partial class MapObject
{
    [DontSerialize]
    public GameMap? Map;

    [DontSerialize]
    public bool Initialized = false;

    public virtual void Init()
    {
        Initialized = true;
    }

    public virtual void Done()
    {

    }

    public virtual void Update(float dt)
    {

    }

    public virtual void Render(RenderComposer c)
    {

    }
}

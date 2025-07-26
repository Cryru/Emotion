using Emotion.Common.Serialization;
using Emotion.IO;

#nullable enable

namespace Emotion.WIPUpdates.One.Work;

public partial class MapObject
{
    // todo: flags
    public bool AlwaysRender;

    [DontSerialize]
    public GameMap? Map;

    [DontSerialize]
    public bool Initialized = false;

    /// <summary>
    /// Called by the map when object is initialized
    /// todo: friend class adapter
    /// </summary>
    public virtual void Init()
    {
        Initialized = true;
    }

    /// <summary>
    /// Called by the map when the object is removed/destroyed
    /// todo: friend class adapter
    /// </summary>
    public virtual void Done()
    {

    }

    public virtual void Update(float dt)
    {

    }

    public virtual void Render(RenderComposer c)
    {

    }

    public void RemoveFromMap()
    {
        if (Map == null) return;
        Map.RemoveObject(this);
    }
}

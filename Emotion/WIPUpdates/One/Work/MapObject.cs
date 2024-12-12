using Emotion.Common.Serialization;
using Emotion.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.WIPUpdates.One.Work;

public partial class MapObject
{
    [DontSerialize]
    public GameMap? Map;

    [DontSerialize]
    public bool Initialized = false;

    public virtual void LoadAssets(AssetLoader assetLoader)
    {

    }

    public virtual void Init()
    {
        Initialized = true;
    }

    public virtual void Update(float dt)
    {

    }

    public virtual void Render(RenderComposer c)
    {

    }
}

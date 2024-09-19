using Emotion.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.WIPUpdates.One.Work;

public class MapObject : Transform
{
    [DontSerialize]
    public GameMap? Map; 

    public virtual void Init()
    {

    }

    public virtual void Update(float dt)
    {

    }

    public virtual void Render(RenderComposer c)
    {

    }
}

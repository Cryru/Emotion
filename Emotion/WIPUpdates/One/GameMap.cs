using Emotion.WIPUpdates.One.Work;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One;

public class GameMap
{
    private List<MapObject> _objects = new();

    public IEnumerator LoadRoutine()
    {
        yield break;
    }

    public void AddObject(MapObject obj)
    {
        _objects.Add(obj);
    }

    public void AddAndInitObject(MapObject obj)
    {
        AddObject(obj);
    }

    public void Update(float dt)
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            obj.Update(dt);
        }
    }

    public void Render(RenderComposer c)
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            obj.Render(c);
        }
    }
}

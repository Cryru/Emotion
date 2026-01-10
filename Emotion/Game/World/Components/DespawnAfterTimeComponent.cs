#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Time;

namespace Emotion.Game.World.Components;

public class DespawnAfterTimeComponent : IGameObjectComponent, IUpdateableComponent
{
    private GameObject? _objectAttachedTo;
    private After _timer;

    public DespawnAfterTimeComponent(int time)
    {
        _timer = new After(time);
    }

    public void Done(GameObject obj)
    {

    }

    public Coroutine? Init(GameObject obj)
    {
        _objectAttachedTo = obj;
        return null;
    }

    public void Update(float dt)
    {
        AssertNotNull(_objectAttachedTo);

        _timer.Update(dt);
        if (_timer.Finished)
        {
            _objectAttachedTo.RemoveFromMap();
        }
    }
}

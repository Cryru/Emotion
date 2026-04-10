#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Time;
using Emotion.Game.World.Components;

namespace Emotion.Network.World;

public class LockStepVerifyObjectComponent : IGameObjectComponent, IUpdateableComponent
{
    private GameObject _obj = null!;
    private ValueTimer _checkTimer = new ValueTimer(100);

    public IRoutineWaiter? Init(GameObject obj)
    {
        _obj = obj;
        return null;
    }

    public void Update(float dt)
    {
        _checkTimer.Update(dt);
        if (_checkTimer.Finished)
        {
            Vector3 pos = _obj.Position3D;
            Engine.Multiplayer.LockStepVerify($"{_obj.ObjectId}, {_obj.Name}, [X:{pos.X}, Y:{pos.Y}, Z:{pos.Z}]");
            _checkTimer.Reset();
        }
    }

    public void Done(GameObject obj)
    {

    }
}
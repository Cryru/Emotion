#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Game.World.Components;

public interface IGameObjectComponent
{
    public IRoutineWaiter? Init(GameObject obj);

    public void Done(GameObject obj);
}
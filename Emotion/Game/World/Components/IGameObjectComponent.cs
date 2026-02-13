#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Game.World.Components;

public interface IGameObjectComponent
{
    public IRoutineWaiter? Init(GameObject obj);

    /// <summary>
    /// Called on the first update
    /// </summary>
    public virtual void GameInit()
    {

    }

    public void Done(GameObject obj);
}
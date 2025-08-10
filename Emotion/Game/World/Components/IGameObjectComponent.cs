#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Game.World.Components;

public interface IGameObjectComponent
{
    public Coroutine? Init(GameObject obj);

    public void Done(GameObject obj);
}
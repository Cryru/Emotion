#nullable enable

using Emotion.Game.World.Systems;

namespace Emotion.Game.World.Components;

public interface IRenderableComponent
{
    public void Render(Renderer r);
}

public interface IUpdateableComponent
{
    public void Update(float dt);
}

public interface ISystemicComponent : IGameObjectComponent
{
    public abstract void Attach(GameMap map, GameObject obj);
    public abstract void Dettach(GameMap map, GameObject obj);
}

public interface ISystemicComponent<T, TSystem> : ISystemicComponent
    where T : ISystemicComponent<T, TSystem>
    where TSystem : WorldSystem<T>, new()
{
    void ISystemicComponent.Attach(GameMap map, GameObject obj)
    {
        TSystem? system = map.GetSystem<TSystem>();
        if (system == null) // First component of kind - add whole system
        {
            system = new();
            map.AddSystem(system);
        }

        if (this is T asT)
            system.ComponentAdded(obj, asT);
    }

    void ISystemicComponent.Dettach(GameMap map, GameObject obj)
    {
        TSystem? system = map.GetSystem<TSystem>();
        if (system == null)
        {
            Assert(false, "System of component being removed doesn't exist???");
            return;
        }

        if (this is T asT)
            system.ComponentRemoved(obj, asT);
    }
}
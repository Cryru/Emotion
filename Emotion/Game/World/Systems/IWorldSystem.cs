#nullable enable

using Emotion.Game.World.Components;
using Emotion.Graphics.Camera;

namespace Emotion.Game.World.Systems;

public abstract class WorldSystem
{
    public abstract void AttachToMap(GameMap map);
    public abstract void Dispose();
}

public abstract class WorldSystem<TComponent> : WorldSystem
    where TComponent : IGameObjectComponent
{
    public GameMap Map = null!;

    public List<TComponent> Components = new List<TComponent>(16);

    public override void AttachToMap(GameMap map)
    {
        Map = map;
        InitInternal();
    }

    public override void Dispose()
    {
        DoneInternal();
    }

    public void ComponentAdded(GameObject obj, TComponent component)
    {
        Components.Add(component);
        OnComponentListChanged();
    }

    public void ComponentRemoved(GameObject obj, TComponent component)
    {
        Helpers.ListRemoveRotate(Components, component);
        OnComponentListChanged();
    }

    protected abstract void InitInternal();
    protected virtual void DoneInternal()
    {

    }
    protected abstract void OnComponentListChanged();
}

public interface IWorldSimulationSystem
{
    public void Update(float dt);
}

public interface IWorldRenderSystem
{
    public void Render(Renderer r, in CameraCullingContext cull);
}
#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Game.World.Components;

public class SolidColorComponent : IRenderableComponent, IGameObjectComponent
{
    public GameObject Object { get; private set; } = null!;

    public Color Color;

    public SolidColorComponent(Color color)
    {
        Color = color;
    }

    public Coroutine? Init(GameObject obj)
    {
        Object = obj;
        return null;
    }

    public void Done(GameObject obj)
    {

    }

    public virtual void Render(Renderer r)
    {
        Rectangle b = Object.GetBoundingRect();
        r.RenderSprite(b, Color);
    }
}

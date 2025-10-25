#nullable enable

namespace Emotion.Game.World.Components;

public interface IRenderableComponent
{
    public void Render(Renderer r);
}

public interface IUpdateableComponent
{
    public void Update(float dt);
}
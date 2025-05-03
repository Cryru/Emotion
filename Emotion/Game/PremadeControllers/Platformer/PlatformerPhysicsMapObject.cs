#nullable enable

namespace Emotion.Game.PremadeControllers.Platformer;

public interface IPlatformControllerCustomLogic
{
    public void SetInputLeftRight(float input);

    public void Jump();
}

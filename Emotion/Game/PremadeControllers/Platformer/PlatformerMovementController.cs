#nullable enable

using Emotion.Game.World;

namespace Emotion.Game.PremadeControllers.Platformer;

public class PlatformerMovementController
{
    public float WalkingSpeed = 0.2f; // Per millisecond

    private GameObject? _character;

    private float _input;

    public void Attach()
    {
        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);
    }

    public void Dettach()
    {
        Engine.Host.OnKey.RemoveListener(KeyHandler);
    }

    public void SetCharacter(GameObject obj)
    {
        _character = obj;
    }

    public void Update(float dt)
    {
        if (_character == null) return;

        GameMap? map = _character.Map;
        if (map == null) return;
       
        float leftRight = _input;
        if (_character is IPlatformControllerCustomLogic objInterface)
            objInterface.SetInputLeftRight(leftRight);
        else
            _character.X += leftRight * WalkingSpeed * dt;
    }

    private bool KeyHandler(Key key, KeyState status)
    {
        Vector2 axis = Engine.Host.GetKeyAxisPart(key, Key.AxisAD);
        if (axis != Vector2.Zero)
        {
            if (status == KeyState.Up) axis = -axis;
            _input += axis.X;
            return false;
        }

        if (key == Key.Space && status == KeyState.Down)
        {
            if (_character is IPlatformControllerCustomLogic objInterface)
                objInterface.Jump();
        }

        return true;
    }
}

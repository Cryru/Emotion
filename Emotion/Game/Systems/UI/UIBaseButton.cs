#nullable enable

namespace Emotion.Game.Systems.UI;

public class UIBaseButton : UIBaseWindow
{
    [DontSerialize]
    public Action<UIBaseButton>? OnClicked;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            OnEnabledChanged();
        }
    }

    private bool _enabled = true;

    protected bool _clickIsOnUp;

    public UIBaseButton()
    {
        HandleInput = true;
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft && Enabled)
        {
            if (status == KeyState.Down && !_clickIsOnUp)
            {
                InternalOnClicked();
                return false;
            }

            if (status == KeyState.Up && _clickIsOnUp)
            {
                InternalOnClicked();
                return false;
            }
        }

        return base.OnKey(key, status, mousePos);
    }

    protected virtual void InternalOnClicked()
    {
        OnClicked?.Invoke(this);
    }

    protected virtual void OnEnabledChanged()
    {

    }
}

#region Using

using Emotion.Common.Serialization;
using Emotion.Platform.Input;

#endregion

#nullable enable

namespace Emotion.UI;

public class UICallbackButton : UIBaseWindow
{
    [DontSerialize] public Action<UICallbackButton> OnMouseEnterProxy;
    [DontSerialize] public Action<UICallbackButton> OnMouseLeaveProxy;
    [DontSerialize] public Action<UICallbackButton, Vector2> OnMouseMoveProxy;
    [DontSerialize] public Action<UICallbackButton> OnClickedProxy;
    [DontSerialize] public Action<UICallbackButton> OnClickedUpProxy;
    [DontSerialize] public Func<UIRollover> OnRolloverSpawn;

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

    public UICallbackButton()
    {
        HandleInput = true;
    }

    public override void OnMouseEnter(Vector2 _)
    {
        base.OnMouseEnter(_);
        OnMouseEnterProxy?.Invoke(this);
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);
        OnMouseLeaveProxy?.Invoke(this);
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        base.OnMouseMove(mousePos);
        OnMouseMoveProxy?.Invoke(this, mousePos);
    }

    public override UIRollover? GetRollover()
    {
        return OnRolloverSpawn?.Invoke();
    }

    public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft && Enabled)
        {
            if (status == KeyStatus.Down)
            {
                OnClicked();
                return false;
            }

            if (status == KeyStatus.Up)
            {
                OnClickedUp();
                return false;
            }
        }

        return base.OnKey(key, status, mousePos);
    }

    protected virtual void OnClicked()
    {
        OnClickedProxy?.Invoke(this);
    }

    protected virtual void OnClickedUp()
    {
        OnClickedUpProxy?.Invoke(this);
    }

    protected virtual void OnEnabledChanged()
    {

    }
}
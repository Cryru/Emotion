﻿#region Using

using Emotion.Common.Serialization;
using Emotion.Platform.Input;

#endregion

namespace Emotion.UI
{
    public class UICallbackButton : UIBaseWindow
    {
        [DontSerialize] public Action<UICallbackButton> OnMouseEnterProxy;
        [DontSerialize] public Action<UICallbackButton> OnMouseLeaveProxy;
        [DontSerialize] public Action<UICallbackButton> OnClickedProxy;
        [DontSerialize] public Action<UICallbackButton> OnClickedUpProxy;

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

        public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
            if (key == Key.MouseKeyLeft)
            {
                if (status == KeyStatus.Down)
                {
                    OnClickedProxy?.Invoke(this);
                    return false;
                }

                if (status == KeyStatus.Up)
                {
                    OnClickedUpProxy?.Invoke(this);
                    return false;
                }
            }

            return base.OnKey(key, status, mousePos);
        }
    }
}
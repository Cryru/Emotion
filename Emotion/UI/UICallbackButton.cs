#region Using

using System;
using System.Numerics;
using Emotion.Common.Serialization;
using Emotion.Platform.Input;

#endregion

namespace Emotion.UI
{
    public class UICallbackButton : UIBaseWindow
    {
        [DontSerialize] public Action<UIBaseWindow> OnMouseEnterProxy;
        [DontSerialize] public Action<UIBaseWindow> OnMouseLeaveProxy;
        [DontSerialize] public Action<UIBaseWindow> OnClickedProxy;

        public UICallbackButton()
        {
            InputTransparent = false;
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
            if (key == Key.MouseKeyLeft && status == KeyStatus.Down) OnClickedProxy?.Invoke(this);
            return base.OnKey(key, status, mousePos);
        }
    }
}
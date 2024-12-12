#region Using

using Emotion.Game.World.Editor;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.PropertyEditors
{
    public class PropEditorBool : UIBaseWindow, IPropEditorGeneric
    {
        public XMLFieldHandler? Field { get; set; }

        private bool _value;
        private Action<object>? _callback;

        public PropEditorBool()
        {
            StretchX = true;
            StretchY = true;
            HandleInput = true;
        }

        public void SetValue(object value)
        {
            _value = (bool) value;

            UIBaseWindow? checkMark = GetWindowById("Checkmark");
            if (checkMark != null)
                checkMark.Visible = _value;
        }

        public object GetValue()
        {
            return _value;
        }

        public void SetCallbackValueChanged(Action<object> callback)
        {
            _callback = callback;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            var inputBg = new UISolidColor();
            inputBg.StretchX = true;
            inputBg.StretchY = true;
            inputBg.WindowColor = MapEditorColorPalette.ButtonColor;
            inputBg.Paddings = new Rectangle(2, 1, 2, 1);
            inputBg.Anchor = UIAnchor.CenterLeft;
            inputBg.ParentAnchor = UIAnchor.CenterLeft;
            inputBg.Id = "Background";

            var checkMark = new UITexture();
            checkMark.TextureFile = "Editor/Checkmark.png";
            checkMark.RenderSize = new Vector2(8, 8);
            checkMark.Smooth = true;
            checkMark.Visible = _value;
            checkMark.Id = "Checkmark";

            inputBg.AddChild(checkMark);
            AddChild(inputBg);
        }

        public override void OnMouseEnter(Vector2 _)
        {
            base.OnMouseEnter(_);

            UIBaseWindow? bg = GetWindowById("Background");
            if (bg != null)
                bg.WindowColor = MapEditorColorPalette.ActiveButtonColor;
        }

        public override void OnMouseLeft(Vector2 mousePos)
        {
            base.OnMouseLeft(mousePos);

            UIBaseWindow? bg = GetWindowById("Background");
            if (bg != null)
                bg.WindowColor = MapEditorColorPalette.ButtonColor;
        }

        public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
        {
            if (key == Key.MouseKeyLeft && status == KeyState.Down)
            {
                SetValue(!_value);
                _callback?.Invoke(_value);
                return false;
            }

            return base.OnKey(key, status, mousePos);
        }
    }
}
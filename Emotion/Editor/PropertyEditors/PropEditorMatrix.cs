#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Editor.PropertyEditors
{
    public class PropEditorMatrix : UIBaseWindow, IPropEditorGeneric
    {
        public XMLFieldHandler Field { get; set; }

        private Matrix4x4 _value;
        private Action<object> _callback;

        private MapEditorLabel _label;

        public PropEditorMatrix()
        {
            StretchX = true;
            StretchY = true;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            _label = new MapEditorLabel("");
            AddChild(_label);
            UpdateValueString();
        }

        public void SetValue(object value)
        {
            Matrix4x4 valueAsMat = (Matrix4x4) value;
            _value = valueAsMat;
            UpdateValueString();
        }

        private void UpdateValueString()
        {
            if (_label == null) return;
            _label.Text = $"Matrix4x4 {(_value.IsIdentity ? "Identity" : "")}";
        }

        public object GetValue()
        {
            return _value;
        }

        public void SetCallbackValueChanged(Action<object> callback)
        {
            _callback = callback;
        }
    }
}
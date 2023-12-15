#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Editor.PropertyEditors
{
    public class PropEditorNestedObject : EditorButton, IPropEditorGeneric
    {
        public XMLFieldHandler Field { get; set; }

        private object _value;
        private Action<object> _changeCallback;

        public PropEditorNestedObject()
        {
            OnClickedProxy = _ =>
            {
                if (_value == null) return;
                var panel = new GenericPropertiesEditorPanel(_value);
                panel.OnPropertyEdited = (_, __) => { _changeCallback?.Invoke(_value); };
                Controller?.AddChild(panel);
            };
            StretchY = true;
        }

        public object GetValue()
        {
            return _value;
        }

        public void SetCallbackValueChanged(Action<object> callback)
        {
            _changeCallback = callback;
        }

        public void SetValue(object value)
        {
            Text = value == null ? "<null>" : value.ToString();
            _value = value;
        }
    }
}
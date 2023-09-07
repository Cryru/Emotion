#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Editor.PropertyEditors
{
	public class PropEditorEnum : EditorButtonDropDown, IPropEditorGeneric
	{
		public XMLFieldHandler? Field { get; set; }

		private Type _enumType;
		private object? _value;
		private bool _nullable;

		private string[] _enumValueNames;

		private Action<object?>? _callback;
		private EditorButton? _button;

		public PropEditorEnum(Type enumType, bool nullable)
		{
			_enumType = enumType;
			_enumValueNames = Enum.GetNames(enumType);
			_nullable = nullable;

			if (_nullable) _enumValueNames = _enumValueNames.AddToArray("<null>", true);
		}

		public void SetValue(object? value)
		{
			_value = value;
			if (_button != null) _button.Text = _value?.ToString() ?? "null";
		}

		public object GetValue()
		{
			return _value!;
		}

		public void SetCallbackValueChanged(Action<object> callback)
		{
			_callback = callback;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			var currentIdx = 0;
			var dropDownItems = new EditorDropDownButtonDescription[_enumValueNames.Length];
			for (var i = 0; i < _enumValueNames.Length; i++)
			{
				string enumValName = _enumValueNames[i];
				object? enumVal = enumValName == "<null>" ? null : Enum.Parse(_enumType, enumValName);

				dropDownItems[i] = new EditorDropDownButtonDescription
				{
					Name = enumValName,
					Click = (_, __) =>
					{
						SetValue(enumVal);
						_callback?.Invoke(enumVal);
					},
					Enabled = () => enumVal != _value
				};

				if (enumVal == _value) currentIdx = i;
			}

			SetItems(dropDownItems, currentIdx);

			Text = "";

			var button = (EditorButton?) GetWindowById("Button");
			if (button != null)
			{
				button.Text = _value?.ToString() ?? "null";
				_button = button;
			}
		}
	}
}
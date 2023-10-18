#region Using

using Emotion.Common.Serialization;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;
using System.Linq;
using System.Reflection;

#endregion

#nullable enable

namespace Emotion.Editor.PropertyEditors
{
	public class PropEditorGameDataReference<T> : EditorButtonDropDown, IPropEditorGeneric
	{
		public XMLFieldHandler? Field { get; set; }

		private Type _enumType;
		private object? _value;
		private bool _nullable;

		private string[] _optionNames;

		private Action<object?>? _callback;
		private EditorButton? _button;

		private bool _isFlags;
		private Enum _flagZeroValue;

		private T[] _options;

		public PropEditorGameDataReference(T[] options)
		{
			_options = options;
			_optionNames = new string[options.Length];
			for (int i = 0; i < options.Length; i++)
			{
				_optionNames[i] = options[i]?.ToString() ?? "<null>";
			}
		}
			
		//	Type enumType, bool nullable)
		//{
		//	_enumType = enumType;
		//	_enumValueNames = Enum.GetNames(enumType);
		//	_nullable = nullable;
		//	_isFlags = enumType.GetCustomAttribute<FlagsAttribute>() != null;

		//	if (_isFlags)
		//	{
		//		var dontSerializeValues = _enumType.GetCustomAttribute<DontSerializeFlagValueAttribute>();
		//		var namesAsList = _enumValueNames.ToList();

		//		var underlyingType = Enum.GetUnderlyingType(_enumType);
		//		var values = Enum.GetValues(enumType);
		//		for (int i = 0; i < values.Length; i++)
		//		{
		//			var value = values.GetValue(i);
		//			if (value == null) continue;

		//			dynamic? underlyingValue = Convert.ChangeType(value, underlyingType);
		//			if (underlyingValue == 0) _flagZeroValue = (Enum)value;

		//			//bool dontSerialize = dontSerializeValues?.SkipThisOne((uint) underlyingValue) ?? false;
		//			//if (dontSerialize)
		//			//{
		//			//	var nameToRemove = Enum.GetName(_enumType, value);
		//			//	if (nameToRemove != null)
		//			//		namesAsList.Remove(nameToRemove);
		//			//}
		//		}

		//		_enumValueNames = namesAsList.ToArray();
		//	}

		//	if (_nullable) _enumValueNames = _enumValueNames.AddToArray("<null>", true);
		//}

		protected override void UpdateCurrentOptionText()
		{
			var button = (EditorButton?)GetWindowById("Button");
			if (button == null) return;

			string text;
			if (_isFlags)
			{
				text = $"{_enumType.Name}: 0x{EditorUtility.GetEnumFlagsAsBinaryString(_value as Enum)}";
			}
			else
			{
				int maxChars = 20;
				text = _value?.ToString() ?? "null";
				if (text.Length > maxChars)
				{
					text = text.Substring(0, maxChars) + "...";
				}
			}

			button.Text = text;
			button.Enabled = true;
		}

		public void SetValue(object? value)
		{
			_value = value;
			UpdateCurrentOptionText();
		}

		public object GetValue()
		{
			return _value!;
		}
		public void SetCallbackValueChanged(Action<object?> callback)
		{
			_callback = callback;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			if (_isFlags)
			{
				var underlyingType = Enum.GetUnderlyingType(_enumType);

				var dropDownItems = new EditorDropDownButtonDescription[_optionNames.Length];
				for (var i = 0; i < _optionNames.Length; i++)
				{
					string enumValName = _optionNames[i];
					Enum? enumValAsEnum = Enum.Parse(_enumType, enumValName) as Enum;
					dynamic? numericEnumVal = Convert.ChangeType(enumValAsEnum, underlyingType);

					var valueAsEnum = _value as Enum;
					bool hasFlag = Helpers.AreObjectsEqual(enumValAsEnum, _flagZeroValue) ?
									Helpers.AreObjectsEqual(valueAsEnum, _flagZeroValue) : valueAsEnum.HasFlag(enumValAsEnum);
					dropDownItems[i] = new EditorDropDownCheckboxDescription
					{
						Name = enumValName,
						Click = (thisItem, __) =>
						{
							var checkListItem = thisItem as EditorDropDownCheckboxDescription;

							var valueAsEnum = _value as Enum;
							bool hasFlag = checkListItem.Checked();

							var result = EditorUtility.EnumSetFlag(valueAsEnum, enumValAsEnum, !hasFlag);
							if (!hasFlag && Helpers.AreObjectsEqual(enumValAsEnum, _flagZeroValue))
							{
								result = _flagZeroValue;
							}

							SetValue(result);
							_callback?.Invoke(result);
						},
						Checked = () =>
						{
							var valueAsEnum = _value as Enum;
							return Helpers.AreObjectsEqual(enumValAsEnum, _flagZeroValue) ?
									Helpers.AreObjectsEqual(valueAsEnum, _flagZeroValue) : valueAsEnum.HasFlag(enumValAsEnum);
						}
					};
				}

				SetItems(dropDownItems, 0);
			}
			else
			{
				var currentIdx = 0;
				var dropDownItems = new EditorDropDownButtonDescription[_optionNames.Length];
				for (var i = 0; i < _optionNames.Length; i++)
				{
					string enumValName = _optionNames[i];
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
			}

			Text = "";
			UpdateCurrentOptionText();
		}
	}
}
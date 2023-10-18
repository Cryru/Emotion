#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;


#endregion

namespace Emotion.Editor.PropertyEditors;

public class PropEditorArray : EditorButton, IPropEditorGeneric
{
	public XMLFieldHandler? Field { get; set; }

	public object? Value;
	private Action<object>? _changeCallback;

	public PropEditorArray()
	{
		OnClickedProxy = (_) =>
		{
			OpenPanel();
		};
		StretchY = true;
	}

	protected void OpenPanel()
	{
		var panel = new PropEditorArrayPanel(this);
		Controller?.AddChild(panel);
	}

	public void SetValue(object value)
	{
		Array? valAsArray = value as Array;
		valAsArray ??= Array.Empty<object>();

		var declType = Field!.TypeHandler.Type;
		var itemType = declType.GetElementType();
		Text = $"{XMLHelpers.GetTypeName(itemType)}[{valAsArray.Length}]";
		if (value == null) Text += " (null)";

		if (Helpers.AreObjectsEqual(Value, value)) return;

		Value = value;
		_changeCallback?.Invoke(value);
	}

	public void ArrayItemModified(int index)
	{
		_changeCallback?.Invoke(Value);
	}

	public object GetValue()
	{
		return Value;
	}

	public void SetCallbackValueChanged(Action<object> callback)
	{
		_changeCallback = callback;
	}

	

	//private void FillDropdownItems()
	//{
	//	var valueAsArray = (Array?) _value;
	//	if (valueAsArray != null)
	//	{
	//		var arrayItems = new EditorDropDownButtonDescription[valueAsArray.Length];
	//		for (var i = 0; i < valueAsArray.Length; i++)
	//		{
	//			object? value = valueAsArray.GetValue(i);
	//			arrayItems[i] = new EditorDropDownButtonDescription
	//			{
	//				Name = value?.ToString() ?? "<null>",
	//				Click = (_, __) =>
	//				{
	//					if (value == null) return;
	//					var panel = new GenericPropertiesEditorPanel(value);
	//					Controller?.AddChild(panel);
	//				}
	//			};
	//		}

	//		SetItems(arrayItems, 0);
	//	}
	//	else
	//	{
	//		SetItems(_noItems, 0);
	//	}
	//}

	//protected override void UpdateCurrentOptionText()
	//{
	//	var button = (EditorButton?) GetWindowById("Button");
	//	if (button == null) return;
	//	button.Text = _value?.ToString() ?? "Null Array";
	//	button.Enabled = true;
	//}
}
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
}
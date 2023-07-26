#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.PropertyEditors
{
	public class PropEditorNone : UIBaseWindow, IPropEditorGeneric
	{
		public XMLFieldHandler Field { get; set; } = null!;

		public string Text
		{
			get => (string?) GetValue();
			set => SetValue(value);
		}

		private object? _value;
		private string _txt = "";
		private Action<object>? _callback;
		private MapEditorLabel? _label;

		public PropEditorNone()
		{
			StretchX = true;
			StretchY = true;
		}

		public void SetValue(object? value)
		{
			_value = value;
			_txt = value?.ToString() ?? "null";
			if (_label != null)
				_label.Text = _txt;
        }

		public object? GetValue()
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

			var lbl = new MapEditorLabel("");
			lbl.Text = _txt;
			AddChild(lbl);
	
			_label = lbl;
			SetValue(_value); // To force setting it in the UI
		}

		protected override bool UpdateInternal()
		{
			return base.UpdateInternal();
		}
	}
}
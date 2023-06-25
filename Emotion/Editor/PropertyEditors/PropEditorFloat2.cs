#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Editor.PropertyEditors
{
	public class PropEditorFloat2 : UIBaseWindow, IPropEditorGeneric
	{
		public XMLFieldHandler Field { get; set; }

		private Vector2 _value;
		private Action<object> _callback;

		private PropEditorNumber<float> _editorX;
		private PropEditorNumber<float> _editorY;

		public PropEditorFloat2()
		{
			LayoutMode = LayoutMode.HorizontalList;
			ListSpacing = new Vector2(2, 0);
			InputTransparent = false;
			StretchX = true;
			StretchY = true;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			var labelX = new MapEditorLabel("X:");
			AddChild(labelX);
			var editorX = new PropEditorNumber<float>();
			AddChild(editorX);
			_editorX = editorX;
			editorX.SetValue(_value.X);
			editorX.SetCallbackValueChanged(newXVal =>
			{
				_value.X = (float) newXVal;
				_callback?.Invoke(_value);
			});

			var labelY = new MapEditorLabel("Y:");
			labelY.Margins = new Rectangle(2, 0, 0, 0);
			AddChild(labelY);
			var editorY = new PropEditorNumber<float>();
			AddChild(editorY);
			_editorY = editorY;
			editorY.SetValue(_value.Y);
			editorY.SetCallbackValueChanged(newYVal =>
			{
				_value.Y = (float) newYVal;
				_callback?.Invoke(_value);
			});
		}

		public void SetValue(object value)
		{
			_value = (Vector2) value;
			_editorX?.SetValue(_value.X);
			_editorY?.SetValue(_value.Y);
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
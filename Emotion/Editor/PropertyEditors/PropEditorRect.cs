#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Editor.PropertyEditors
{
	public class PropEditorRect : UIBaseWindow, IPropEditorGeneric
	{
		public XMLFieldHandler Field { get; set; }

		private Rectangle _value;
		private Action<object> _callback;
		private bool _withLabels;

		private PropEditorNumber<float> _editorX;
		private PropEditorNumber<float> _editorY;
		private PropEditorNumber<float> _editorZ;
		private PropEditorNumber<float> _editorW;

		public PropEditorRect(bool showLabels = true)
		{
			_withLabels = showLabels;
			LayoutMode = LayoutMode.HorizontalList;
			ListSpacing = new Vector2(2, 0);
			StretchX = true;
			StretchY = true;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			if (_withLabels)
			{
				var labelX = new MapEditorLabel("X:");
				AddChild(labelX);
			}
			
			var editorX = new PropEditorNumber<float>();
			AddChild(editorX);
			_editorX = editorX;
			editorX.SetValue(_value.X);
			editorX.SetCallbackValueChanged(newXVal =>
			{
				_value.X = (float) newXVal;
				_callback?.Invoke(_value);
			});

			if (_withLabels)
			{
				var label = new MapEditorLabel("Y:");
				label.Margins = new Rectangle(2, 0, 0, 0);
				AddChild(label);
			}

			var editorY = new PropEditorNumber<float>();
			AddChild(editorY);
			_editorY = editorY;
			editorY.SetValue(_value.Y);
			editorY.SetCallbackValueChanged(newYVal =>
			{
				_value.Y = (float) newYVal;
				_callback?.Invoke(_value);
			});

			if (_withLabels)
			{
				var label = new MapEditorLabel("Width:");
				label.Margins = new Rectangle(2, 0, 0, 0);
				AddChild(label);
			}

			var editorZ = new PropEditorNumber<float>();
			AddChild(editorZ);
			_editorZ = editorZ;
			editorZ.SetValue(_value.Width);
			editorZ.SetCallbackValueChanged(newZVal =>
			{
				_value.Width = (float) newZVal;
				_callback?.Invoke(_value);
			});

            if (_withLabels)
            {
                var label = new MapEditorLabel("Height:");
                label.Margins = new Rectangle(2, 0, 0, 0);
                AddChild(label);
            }

            var editorW = new PropEditorNumber<float>();
            AddChild(editorW);
            _editorW = editorW;
            editorW.SetValue(_value.Height);
            editorW.SetCallbackValueChanged(newZVal =>
            {
                _value.Height = (float)newZVal;
                _callback?.Invoke(_value);
            });
        }

		public void SetValue(object value)
		{
			_value = (Rectangle) value;
			_editorX?.SetValue(_value.X);
			_editorY?.SetValue(_value.Y);
			_editorZ?.SetValue(_value.Width);
			_editorW?.SetValue(_value.Height);
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
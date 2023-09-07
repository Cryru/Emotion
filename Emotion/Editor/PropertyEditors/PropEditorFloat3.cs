#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Editor.PropertyEditors
{
	public class PropEditorFloat3 : UIBaseWindow, IPropEditorGeneric
	{
		public XMLFieldHandler Field { get; set; }

		private Vector3 _value;
		private Action<object> _callback;
		private bool _withLabels;

		private PropEditorNumber<float> _editorX;
		private PropEditorNumber<float> _editorY;
		private PropEditorNumber<float> _editorZ;

		public PropEditorFloat3(bool showLabels = true)
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
				var labelY = new MapEditorLabel("Y:");
				labelY.Margins = new Rectangle(2, 0, 0, 0);
				AddChild(labelY);
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
				var labelZ = new MapEditorLabel("Z:");
				labelZ.Margins = new Rectangle(2, 0, 0, 0);
				AddChild(labelZ);
			}

			var editorZ = new PropEditorNumber<float>();
			AddChild(editorZ);
			_editorZ = editorZ;
			editorZ.SetValue(_value.Z);
			editorZ.SetCallbackValueChanged(newZVal =>
			{
				_value.Z = (float) newZVal;
				_callback?.Invoke(_value);
			});
		}

		public void SetValue(object value)
		{
			_value = (Vector3) value;
			_editorX?.SetValue(_value.X);
			_editorY?.SetValue(_value.Y);
			_editorZ?.SetValue(_value.Z);
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
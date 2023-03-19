#region Using

using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorFloat3 : UIBaseWindow, IMapEditorGeneric
	{
		private Vector3 _value;
		private Action<object> _callback;

		private MapEditorFloat _editorX;
		private MapEditorFloat _editorY;
		private MapEditorFloat _editorZ;

		public MapEditorFloat3()
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
			var editorX = new MapEditorFloat();
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
			var editorY = new MapEditorFloat();
			AddChild(editorY);
			_editorY = editorY;
			editorY.SetValue(_value.Y);
			editorY.SetCallbackValueChanged(newYVal =>
			{
				_value.Y = (float) newYVal;
				_callback?.Invoke(_value);
			});

			var labelZ = new MapEditorLabel("Z:");
			labelZ.Margins = new Rectangle(2, 0, 0, 0);
			AddChild(labelZ);
			var editorZ = new MapEditorFloat();
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
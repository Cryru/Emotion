#region Using

using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorFloat3 : UIBaseWindow, IMapEditorGeneric
	{
		public XMLFieldHandler Field { get; set; }

		private Vector3 _value;
		private Action<object> _callback;

		private MapEditorNumber<float> _editorX;
		private MapEditorNumber<float> _editorY;
		private MapEditorNumber<float> _editorZ;

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
			var editorX = new MapEditorNumber<float>();
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
			var editorY = new MapEditorNumber<float>();
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
			var editorZ = new MapEditorNumber<float>();
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
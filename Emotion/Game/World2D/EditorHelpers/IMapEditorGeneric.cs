using Emotion.Standard.XML;

namespace Emotion.Game.World2D.EditorHelpers
{
	public interface IMapEditorGeneric
	{
		public XMLFieldHandler Field { get; set; }

		public void SetValue(object value);
		public object GetValue();

		public void SetCallbackValueChanged(Action<object> callback);
	}
}
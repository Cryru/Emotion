namespace Emotion.Game.World2D.EditorHelpers
{
	public interface IMapEditorGeneric
	{
		public void SetValue(object value);
		public object GetValue();

		public void SetCallbackValueChanged(Action<object> callback);
	}
}
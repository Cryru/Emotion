#nullable enable

namespace Emotion.Game.World2D.EditorHelpers
{
	public class EditorDropDownButtonDescription
	{
		public string Name = null!;
		public Action<MapEditorTopBarButton>? Click;
		public Func<bool>? Enabled;
	}
}
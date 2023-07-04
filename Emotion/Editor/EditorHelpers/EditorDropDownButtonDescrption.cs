#nullable enable

using Emotion.Game.World2D.EditorHelpers;

namespace Emotion.Editor.EditorHelpers
{
	public class EditorDropDownButtonDescription
	{
		public string Name = null!;
		public Action<EditorDropDownButtonDescription, MapEditorTopBarButton>? Click;
		public Func<bool>? Enabled;
		public object? UserData;
	}
}
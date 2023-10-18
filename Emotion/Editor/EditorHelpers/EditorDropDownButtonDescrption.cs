#nullable enable

namespace Emotion.Editor.EditorHelpers
{
	public class EditorDropDownButtonDescription
	{
		public string Name = null!;
		public Action<EditorDropDownButtonDescription, EditorButton>? Click;
		public Func<bool>? Enabled;
		public object? UserData;
	}

	public class EditorDropDownCheckboxDescription : EditorDropDownButtonDescription
	{
		public Func<bool> Checked { get; set; }
	}
}
#nullable enable

namespace Emotion.Editor.EditorHelpers
{
	public class EditorDropDownItem
	{
		public string Name = null!;
		public Func<string> NameFunc = null!;

		public Action<EditorDropDownItem, EditorButton>? Click;
		public Func<bool>? Enabled;
		public object? UserData;
	}

	public class EditorDropDownCheckboxItem : EditorDropDownItem
	{
		public Func<bool> Checked { get; set; }
	}
}
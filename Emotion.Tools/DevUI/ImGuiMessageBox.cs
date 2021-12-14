namespace Emotion.Tools.DevUI
{
    public class ImGuiMessageBox : ButtonInputModalWindow
    {
        public ImGuiMessageBox(string title, string text) : base(title, text, null, "Ok")
        {
        }
    }
}
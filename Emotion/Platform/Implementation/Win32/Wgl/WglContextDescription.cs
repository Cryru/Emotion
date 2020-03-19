namespace Emotion.Platform.Implementation.Win32.Wgl
{
    public enum GLProfile
    {
        Core,
        Compat,
        Any
    }

    public class WglContextDescription
    {
        public int Major = 1;
        public int Minor = 0;
        public bool Debug;
        public GLProfile Profile = GLProfile.Core;
        public bool ForwardCompat = true;

        public override string ToString()
        {
            return $"GL: {Major}.{Minor}, Debug: {Debug}, Profile: {Profile}, Forward: {ForwardCompat}";
        }
    }
}
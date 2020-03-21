namespace Emotion.Platform.OpenGL.Meta
{
    public class GLContextDescription
    {
        public int Major = 1;
        public int Minor = 0;
        public bool Debug;
        public GLProfile Profile = GLProfile.Any;
        public bool ForwardCompat = false;

        public override string ToString()
        {
            return $"GL: {Major}.{Minor}, Debug: {Debug}, Profile: {Profile}, ForwardCompat: {ForwardCompat}";
        }
    }
}
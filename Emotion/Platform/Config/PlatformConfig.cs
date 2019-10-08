#region Using

using Emotion.Common;

#endregion

namespace Emotion.Platform.Config
{
    public class PlatformConfig
    {
        public int MajorVersion = 1;
        public int MinorVersion = 0;
        public bool ForwardCompatible = false;
        public bool Debug = false;
        public ContextProfile Profile = ContextProfile.Any;

        public string Title = "Untitled Window";
        public bool Resizable = true;
        public CursorMode CursorMode = CursorMode.Normal;

        public int Width = 960;
        public int Height = 540;
        public int MinWidth = -1;
        public int MinHeight = -1;
        public int MaxWidth = -1;
        public int MaxHeight = -1;

        public sbyte Samples = 0;

        public DisplayMode DisplayMode = DisplayMode.Windowed;

        public bool Retina = true;

        public bool IsValidContext()
        {
            // OpenGL 1.0 is the smallest valid version
            // OpenGL 1.x series ended with version 1.5
            // OpenGL 2.x series ended with version 2.1
            // OpenGL 3.x series ended with version 3.3
            // For now, let everything else through
            if (MajorVersion < 1 || MinorVersion < 0 ||
                MajorVersion == 1 && MinorVersion > 5 ||
                MajorVersion == 2 && MinorVersion > 1 ||
                MajorVersion == 3 && MinorVersion > 3)
            {
                Engine.Log.Warning($"Invalid OpenGL version {MajorVersion}.{MinorVersion}", "Emotion.Platform.PlatformConfig");
                return false;
            }

            if (Profile != ContextProfile.Any)
                if (MajorVersion <= 2 || MajorVersion == 3 && MinorVersion < 2)
                {
                    Engine.Log.Warning("OpenGL core profiles are only defined for version 3.2 and above.", "Emotion.Platform.PlatformConfig");
                    return false;
                }

            if (!ForwardCompatible || MajorVersion > 2) return true;

            Engine.Log.Warning("OpenGL forward compatible contexts are defined for version 3.0 and above.", "Emotion.Platform.PlatformConfig");
            return false;
        }
    }
}
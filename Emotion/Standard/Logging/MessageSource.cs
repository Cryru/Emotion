namespace Emotion.Standard.Logging
{
    public static class MessageSource
    {
        // Engine and modules
        public static string Engine = "Engine"; // General engine setup
        public static string StdErr = "StdErr"; // Uncaught errors
        public static string Renderer = "Renderer"; // Sent by the renderer - includes mostly statistics
        public static string GL = "GL"; // Sent by the renderer and graphics related objects.
        public static string ShaderSource = "ShaderSource"; // Sent by the ShaderFactory - is spammy and contains shader source code.
        public static string AssetLoader = "Assets"; // Assetloader and various assets
        public static string SceneManager = "Scenes"; // SceneManager and scene related code.
        public static string Platform = "Platform"; // Sent by the base platform logic
        public static string Audio = "Audio"; // AudioManager, AudioLayers and everything audio related, excluding platform audio code.
        public static string Input = "Input"; // InputManager, usually logs input - is spammy.

        public static string Debug = "Debug"; // Functionality used for debugging, spread all over the code.
        public static string Other = "Other"; // When I was lazy and didn't know what to put.

        public static string Game = "Game"; // General game logic and functionality.
        public static string Anim = "Animation";

        // Specific
        public static string ImagePng = "ImagePng";
        public static string FontParser = "FontParser";
        public static string XML = "XML";
        public static string TMX = "TMX";
        public static string PackedAssetSource = "PackedAssetSource";

        // Platform
        public static string Win32 = "Win32";
        public static string Wgl = "Wgl";
        public static string WGallium = "Win32-Gallium";
        public static string WasApi = "WasApi";
        public static string Glfw = "Glfw";
        public static string Egl = "Egl";
        public static string OpenAL = "OpenAL";

        // Other
        public static string ScriptingEngine = "Scripting";
    }
}
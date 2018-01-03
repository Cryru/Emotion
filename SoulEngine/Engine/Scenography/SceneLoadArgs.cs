// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine.Scenography
{
    public class SceneLoadArgs
    {
        public Scene Scene;
        public bool SwapTo;
        public string Name;

        // Trackers
        public bool Loaded { get; internal set; } = false;
        public bool Queued { get; internal set; } = false;
    }
}
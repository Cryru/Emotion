// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine
{
    public class SceneLoadArgs
    {
        public Scene Scene;
        public bool SwapTo;
        public string Name;
        public bool Loaded = false;
        public bool Queued = false;
    }
}
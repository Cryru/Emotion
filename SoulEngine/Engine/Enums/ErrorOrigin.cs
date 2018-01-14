// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine.Enums
{
    /// <summary>
    /// The origin system of an error.
    /// </summary>
    public enum ErrorOrigin
    {
        Unknown = 0,
        SoulLib = 1,
        Scripting = 2,
        Breath = 3,
        SceneManager = 4,
        SceneLogic = 5,
        Physics = 6,
        AssetManager = 7
    }
}
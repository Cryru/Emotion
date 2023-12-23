#nullable enable

using Emotion;

namespace Emotion.Game.World.Prefab;

public class GameObjectPrefab
{
    public string PrefabName;
    public int PrefabVersion;

    public string ObjectData;

    /// <summary>
    /// Used for comparison to placed objects, and to avoid having to
    /// initialize the object. Also it keeps tracks of properties in
    /// previous versions.
    /// </summary>
    public List<Dictionary<string, object?>>? DefaultProperties;

    public override string ToString()
    {
        return PrefabName;
    }
}
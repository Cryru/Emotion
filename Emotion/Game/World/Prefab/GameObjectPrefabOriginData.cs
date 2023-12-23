#nullable enable

using Emotion;

namespace Emotion.Game.World.Prefab;

public class GameObjectPrefabOriginData
{
    public string PrefabName;
    public int PrefabVersion;

    public GameObjectPrefabOriginData(GameObjectPrefab prefab)
    {
        PrefabName = prefab.PrefabName;
        PrefabVersion = prefab.PrefabVersion;
    }

    protected GameObjectPrefabOriginData()
    {
        // serialization
        PrefabName = null!;
    }

    public override string ToString()
    {
        return $"{PrefabName} v{PrefabVersion}";
    }
}
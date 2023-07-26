#region Using


#endregion

#nullable enable

namespace Emotion.Game.World2D.Editor;

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

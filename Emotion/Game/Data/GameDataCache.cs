#nullable enable

#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.Data;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Game.Data.GameDataObject>;

#endregion

namespace Emotion.Game.Data;

public static partial class GameDataDatabase
{
    // Database internal class for handling storage of game data objects of a particular type.
    private sealed class GameDataCache
    {
        public Type Type { get; init; }

        public List<GameDataObject> Objects = new();

        public Dictionary<string, int> IdMap = new(StringComparer.OrdinalIgnoreCase);

        public GameDataCache(Type type)
        {
            Type = type;
        }

        public void RecreateIdMap()
        {
            IdMap.Clear();

            Objects.Sort();
            for (int i = 0; i < Objects.Count; i++)
            {
                GameDataObject obj = Objects[i];
                if (IdMap.ContainsKey(obj.Id))
                {
                    Engine.Log.Warning($"Found duplicate game data id {obj.Id} of type {Type.Name}", MessageSource.GameData);
                    obj.Id = EnsureNonDuplicatedId(obj.Id);
                }

                IdMap.Add(obj.Id, i);
            }
        }

        public string EnsureNonDuplicatedId(string name)
        {
            var counter = 1;
            string originalName = name;
            while (IdMap.ContainsKey(name)) name = originalName + "_" + counter++;
            return name;
        }

        public GameDataObject? GetObjectById(string id)
        {
            if (!IdMap.TryGetValue(id, out int idx)) return null;
            return Objects[idx];
        }

        public GameDataArray<T> GetDataEnum<T>() where T : GameDataObject
        {
            return new GameDataArray<T>(Objects);
        }

        //public static async Task LoadGameDataAssetTask(string fileName, GameDataCache thisAssetCache)
        //{
        //    GameDataObjectAsset? asset = await Engine.AssetLoader.GetAsync<GameDataObjectAsset>(fileName, false);
        //    if (asset == null || asset.Content == null) return;

        //    GameDataObject gameDataObjContent = asset.Content;
        //    gameDataObjContent.LoadedFromFile = asset.Name;

        //    if (thisAssetCache == null) return;

        //    lock (thisAssetCache)
        //    {
        //        thisAssetCache.Objects.Add(gameDataObjContent);
        //    }
        //}
    }
}
#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Grids;

namespace Emotion.Game.World;

public class GameMapAsset : XMLAssetBase<GameMapFactory, GameMapAsset>
{
    public const string FILE_EXTENSION = "gamemap";
    public const string FILE_EXTENSION_WITH_DOT = $".{FILE_EXTENSION}";
    public const string DATA_FOLDER_NAME = "_Data";

    static GameMapAsset()
    {
        RegisterFileExtensionSupport<GameMapAsset>([FILE_EXTENSION]);
    }

    public GameMapAsset()
    {
        _useNewLoading = true;
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        yield return base.Internal_LoadAssetRoutine(data);

        GameMapFactory? map = Content;
        if (map == null)
        {
            Engine.Log.Warning($"Failed to deserialize map data for map {Name}", "GameMap");
            yield break;
        }
        map.MapPath = Name;

        string mapFolder = GetMapFolder();
        IMapGrid[] grids = map.Grids;
        Coroutine[] routines = new Coroutine[grids.Length];
        for (int i = 0; i < grids.Length; i++)
        {
            IMapGrid grid = grids[i];
            routines[i] = Engine.CoroutineManager.StartCoroutine(grid._LoadRoutine($"{mapFolder}/{grid.UniqueId}", this));
        }

        yield return Coroutine.WhenAll(routines);
    }

    private string GetMapFolder()
    {
        return $"{Name.Replace(FILE_EXTENSION_WITH_DOT, string.Empty)}{DATA_FOLDER_NAME}";
    }

    public static GameMapAsset CreateFromMap(GameMap map, string path)
    {
        if (!path.EndsWith(FILE_EXTENSION_WITH_DOT))
            path = $"{path}{FILE_EXTENSION_WITH_DOT}";

        GameMapFactory factory = GameMapFactory.CreateFromMap(map);
        return GameMapAsset.CreateFromContent(factory, path);
    }

    public override bool SaveAs(string name, bool backup = true)
    {
        if (Content == null)
            return false;

        if (!name.EndsWith(FILE_EXTENSION_WITH_DOT))
            name = $"{name}{FILE_EXTENSION_WITH_DOT}";

        bool saved = base.SaveAs(Name, backup);
        if (saved)
        {
            string mapFolder = GetMapFolder();
            IMapGrid[] grids = Content.Grids;
            for (int i = 0; i < grids.Length; i++)
            {
                IMapGrid grid = grids[i];
                bool gridSaved = grid._Save($"{mapFolder}/{grid.UniqueId}");
                if (!gridSaved) return false;
            }
        }
        return true;
    }
}

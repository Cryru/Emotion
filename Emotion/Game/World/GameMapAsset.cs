#nullable enable

using Emotion;
using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Serialization.XML;
using System.Runtime.InteropServices;

namespace Emotion.Game.World;

public class GameMapAsset : Asset, IAssetContainingObject<GameMap>
{
    public const string FILE_EXTENSION = "gamemap";
    public const string FILE_EXTENSION_WITH_DOT = $".{FILE_EXTENSION}";
    public const string DATA_FOLDER_NAME = "_Data";

    private GameMap? _gameMap;

    static GameMapAsset()
    {
        RegisterFileExtensionSupport<GameMapAsset>([FILE_EXTENSION]);
    }

    public GameMapAsset()
    {
        _useNewLoading = true;
    }

    public GameMap? GetObject()
    {
        return _gameMap;
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        ReadOnlySpan<byte> dataSpan = data.Span;

        ValueStringReader reader;
        System.Text.Encoding encoding = Helpers.GuessStringEncoding(dataSpan);
        if (encoding == System.Text.Encoding.Unicode)
        {
            ReadOnlySpan<char> asChar = MemoryMarshal.Cast<byte, char>(dataSpan);
            reader = new ValueStringReader(asChar);
        }
        else
        {
            reader = new ValueStringReader(dataSpan);
        }

        var map = XMLSerialization.From<GameMap>(ref reader);
        if (map == null)
        {
            Engine.Log.Warning($"Failed to deserialize map data for map {Name}", "GameMap");
            yield break;
        }
        map.MapPath = Name;
        _gameMap = map;

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

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {

    }

    protected override void DisposeInternal()
    {

    }

    private string GetMapFolder()
    {
        return $"{Name.Replace(FILE_EXTENSION_WITH_DOT, string.Empty)}{DATA_FOLDER_NAME}";
    }

    public static GameMapAsset CreateFromMap(GameMap map, string path)
    {
        if (!path.EndsWith(FILE_EXTENSION_WITH_DOT))
            path = $"{path}{FILE_EXTENSION_WITH_DOT}";

        map.MapPath = path;
        return new GameMapAsset()
        {
            Name = path,
            _gameMap = map,
            Processed = true
        };
    }

    public bool Save(string? path = null)
    {
        GameMap? map = _gameMap;
        if (map == null) return false;

        string? data = XMLSerialization.To(map);
        AssertNotNull(data);
        if (data == null) return false;

        bool mapFileSaved = Engine.AssetLoader.Save(Name, data);
        if (!mapFileSaved) return false;

        string mapFolder = GetMapFolder();
        IMapGrid[] grids = map.Grids;
        for (int i = 0; i < grids.Length; i++)
        {
            IMapGrid grid = grids[i];
            bool gridSaved = grid._Save($"{mapFolder}/{grid.UniqueId}");
            if (!gridSaved) return false;
        }

        return true;
    }
}

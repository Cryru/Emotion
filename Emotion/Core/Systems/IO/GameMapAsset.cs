#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using Emotion.Standard.Reflector;
using Emotion.Standard.Serialization.XML;
using System.Runtime.InteropServices;

namespace Emotion.Core.Systems.IO;

public class GameMapAsset : Asset, IAssetContainingObject<GameMap>
{
    public const string FILE_EXTENSION = "gamemap";
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

        _gameMap = XMLSerialization.From<GameMap>(ref reader);
        if (_gameMap == null)
        {
            Engine.Log.Warning($"Failed to deserialize map data for map {Name}", "GameMap");
            yield break;
        }

        GameMap map = _gameMap;
        string mapName = map.MapName;
        string mapFolder = $"{mapName}{GameMapAsset.DATA_FOLDER_NAME}";

        //LoadAssetDependency

        IMapGrid[] grids = map.Grids;
        Coroutine[] routines = new Coroutine[grids.Length];
        for (int i = 0; i < grids.Length; i++)
        {
            IMapGrid grid = grids[i];
            routines[i] = Engine.CoroutineManager.StartCoroutine(grid._LoadRoutine($"{mapFolder}/{grid.UniqueId}"));
        }

        yield return Coroutine.WhenAll(routines);
    }

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {

    }

    protected override void DisposeInternal()
    {

    }
}

#nullable enable

namespace Emotion.Core.Systems.IO.Sources;

[DontSerialize] // reflector freaks out over this for some reason
public interface IAssetSource<TSourceData>
{
    public abstract static FileReadRoutineResult GetAssetContent(TSourceData loadData);
}
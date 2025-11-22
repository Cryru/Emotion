#nullable enable

using Emotion.Core.Systems.IO.Sources;
using Emotion.Primitives.DataStructures;

namespace Emotion.Core.Systems.IO;

public abstract class AssetFileEntry
{
    public string Name { get; init; }

    public string FullName { get; init; }

    public AssetFileEntry(ReadOnlySpan<char> name, NTreeString<AssetFileEntry> branch)
    {
        Name = name.AsLowerCaseString();
        FullName = branch.GetFullPathToRoot('/', name, true);
    }

    public override string ToString()
    {
        return Name;
    }

    public abstract FileReadRoutineResult GetAssetData();
}

public class AssetFileEntry<T, TSourceData> : AssetFileEntry
    where T : IAssetSource<TSourceData>
{
    public TSourceData AssetSourceLoadData { get; init; }

    public AssetFileEntry(ReadOnlySpan<char> name, NTreeString<AssetFileEntry> branch, TSourceData sourceLoadData) : base(name, branch)
    {
        AssetSourceLoadData = sourceLoadData;
    }

    public override FileReadRoutineResult GetAssetData()
    {
        return T.GetAssetContent(AssetSourceLoadData);
    }
}
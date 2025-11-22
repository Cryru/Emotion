#nullable enable

using Emotion.Primitives.DataStructures;

namespace Emotion.Core.Systems.IO.Sources;

public class AliasSource : IAssetSource<string>
{
    public static FileReadRoutineResult GetAssetContent(string loadData)
    {
        throw new NotImplementedException();
    }
}

public class AliasEntry : AssetFileEntry<AliasSource, string>
{
    public AliasEntry(string name, NTreeString<AssetFileEntry> branch, string sourceLoadData) : base(name, branch, sourceLoadData)
    {
    }
}
#region Using

using Emotion.IO;

#endregion

#nullable enable

namespace Emotion.Editor;

public class AssetFileNameAttribute<T> : Attribute where T : Asset
{
}
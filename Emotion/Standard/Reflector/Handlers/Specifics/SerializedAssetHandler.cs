#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Standard.Reflector.Handlers.Base;

namespace Emotion.Standard.Reflector.Handlers.Specifics;

public class SerializedAssetHandler<T, TAsset> : ComplexTypeHandler<T> where TAsset : Asset, new()
{
    public SerializedAssetHandler(Func<T>? createNew, string typeName, ComplexTypeHandlerMemberBase[] members) : base(createNew, typeName, members)
    {
    }

    public override TypeEditor? GetEditor()
    {
        return new AssetHandleEditor<TAsset>();
    }
}

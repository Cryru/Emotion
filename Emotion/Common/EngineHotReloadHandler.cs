#nullable enable
#if DEBUG

using Emotion.Game.Data;
using Emotion.Standard.Reflector;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(EngineHotReloadHandler))]

namespace Emotion.Common;

internal static class EngineHotReloadHandler
{
    public static void UpdateApplication(Type[]? updatedTypes)
    {
#if AUTOBUILD
        return;
#endif
        if (updatedTypes == null) return;

        ReflectorEngine.OnHotReload(updatedTypes);
        GameDatabase.EditorAdapter.OnHotReload(updatedTypes);
    }
}

#endif
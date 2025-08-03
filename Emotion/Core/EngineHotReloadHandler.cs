#nullable enable
#if DEBUG

using Emotion;
using Emotion.Core;
using Emotion.Standard.Reflector;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(EngineHotReloadHandler))]

namespace Emotion.Core;

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
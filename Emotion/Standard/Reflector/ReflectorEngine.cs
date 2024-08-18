#nullable enable

namespace Emotion.Standard.Reflector;

public static class ReflectorEngine
{
    private static List<IEmotionReflectorDataSource> _dataSources = new List<IEmotionReflectorDataSource>();

    public static void RegisterDataSource(IEmotionReflectorDataSource source)
    {
        _dataSources.Add(source);
    }

    public static IReflectorTypeData? GetTypeData(string name)
    {
        for (int i = 0; i < _dataSources.Count; i++)
        {
            var src = _dataSources[i];
            var typ = src.GetTypeData(name);
            if (typ != null) return typ;
        }
        return null;
    }
}

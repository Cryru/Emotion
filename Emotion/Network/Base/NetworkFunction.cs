using Emotion.Core.Utility.Coroutines;
using Emotion.Standard.Parsers.XML;

#nullable enable

namespace Emotion.Network.Base;

public abstract class NetworkFunction
{
    public abstract void Invoke(ReadOnlySpan<byte> data);

    public abstract void InvokeAtGameTime(CoroutineManagerGameTime timeManager, int time, ReadOnlySpan<byte> data);
}

public class NetworkFunction<T> : NetworkFunction
{
    public string Name { get; protected set; }

    private Action<T> _method;

    public NetworkFunction(string name, Action<T> method)
    {
        Name = name;
        _method = method;
    }

    public override void Invoke(ReadOnlySpan<byte> data)
    {
        T? deserializedData = XMLFormat.From<T>(data);
        if (deserializedData == null) return;
        _method?.Invoke(deserializedData);
    }

    public override void InvokeAtGameTime(CoroutineManagerGameTime timeManager, int time, ReadOnlySpan<byte> data)
    {
        T? deserializedData = XMLFormat.From<T>(data);
        if (deserializedData == null) return;
        timeManager.StartCoroutine(GameTimeRoutine(timeManager, time, deserializedData));
    }

    private IEnumerator GameTimeRoutine(CoroutineManagerGameTime timeManager, int time, T data)
    {
        int timeDiff = time - (int)timeManager.Time;
        yield return timeDiff;
        Assert(timeManager.Time == time);

        _method?.Invoke(data);
    }
}

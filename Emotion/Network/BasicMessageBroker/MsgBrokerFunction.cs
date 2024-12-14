using Emotion.Game.Time.Routines;
using Emotion.Standard.XML;

#nullable enable

namespace Emotion.Network.BasicMessageBroker;

public abstract class MsgBrokerFunction
{
    public abstract void Invoke(ReadOnlySpan<byte> data);
    public abstract void InvokeAtGameTime(CoroutineManagerGameTime timeManager, int time, ReadOnlySpan<byte> data);
}

public class MsgBrokerFunction<T> : MsgBrokerFunction
{
    public string Name { get; protected set; }

    private Action<T> _method;

    public MsgBrokerFunction(string name, Action<T> method)
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

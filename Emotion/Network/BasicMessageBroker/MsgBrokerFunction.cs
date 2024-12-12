using Emotion.Standard.XML;

#nullable enable

namespace Emotion.Network.BasicMessageBroker;

public abstract class MsgBrokerFunction
{
    public abstract void Invoke(ReadOnlySpan<byte> data);
    public abstract void InvokeAtGameTime(int time, ReadOnlySpan<byte> data);
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

    public override void InvokeAtGameTime(int time, ReadOnlySpan<byte> data)
    {
        T? deserializedData = XMLFormat.From<T>(data);
        if (deserializedData == null) return;
        Engine.CoroutineManagerGameTime.StartCoroutine(GameTimeRoutine(time, deserializedData));
    }

    private IEnumerator GameTimeRoutine(int time, T data)
    {
        int timeDiff = time - (int)Engine.CurrentGameTime;
        yield return timeDiff;
        Assert(Engine.CurrentGameTime == time);

        _method?.Invoke(data);
    }
}

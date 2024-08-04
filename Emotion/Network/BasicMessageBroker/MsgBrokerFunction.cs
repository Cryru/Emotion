using Emotion.Standard.XML;

#nullable enable

namespace Emotion.Network.BasicMessageBroker;

public abstract class MsgBrokerFunction
{
    public abstract void Invoke(string data);
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

    public override void Invoke(string data)
    {
        T? deserializedData = XMLFormat.From<T>(data);
        if (deserializedData == null) return;
        _method?.Invoke(deserializedData);
    }
}

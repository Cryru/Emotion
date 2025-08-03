using Emotion.Network.ServerSide;
using Emotion.Standard.Parsers.XML;


#nullable enable

namespace Emotion.Network.Base;

public abstract class ServerFunction
{
    public abstract void Invoke(ServerUser sender, ReadOnlySpan<byte> data);
}

public class ServerFunction<T> : ServerFunction
{
    public string Name { get; protected set; }

    private Action<ServerUser, T> _method;

    public ServerFunction(string name, Action<ServerUser, T> method)
    {
        Name = name;
        _method = method;
    }

    public override void Invoke(ServerUser sender, ReadOnlySpan<byte> data)
    {
        T? deserializedData = XMLFormat.From<T>(data);
        if (deserializedData == null) return;
        _method?.Invoke(sender, deserializedData);
    }
}
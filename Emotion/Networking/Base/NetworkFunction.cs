using Emotion.Networking.ServerSide;
using Emotion.Standard.XML;

#nullable enable

namespace Emotion.Networking.Base;

public abstract class NetworkFunction
{
    public abstract bool TryInvoke(ServerPlayer? sender, NetworkMessageData msg);
}

public class NetworkFunction<T> : NetworkFunction
{
    public uint MessageType { get; init; }

    private Action<ServerPlayer, T>? _funcWithSender;
    private Action<T>? _funcNoSender;

    public NetworkFunction(uint messageType, Action<ServerPlayer, T> func)
    {
        MessageType = messageType;
        _funcWithSender = func;
    }

    public NetworkFunction(uint messageType, Action<T> func)
    {
        MessageType = messageType;
        _funcNoSender = func;
    }

    public override unsafe bool TryInvoke(ServerPlayer? sender, NetworkMessageData msg)
    {
        var contentLength = msg.ContentLength;
        Span<byte> data = new Span<byte>(msg.Content, contentLength);
        T? obj = XMLFormat.From<T>(data);
        if (obj == null)
            return false;

        if (_funcWithSender != null)
        {
            if (sender == null)
                return false;

            _funcWithSender.Invoke(sender, obj);
            return true;
        }

        AssertNotNull(_funcNoSender);
        _funcNoSender.Invoke(obj);
        return true;
    }
}

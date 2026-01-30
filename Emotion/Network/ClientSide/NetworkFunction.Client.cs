#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;

namespace Emotion.Network.ClientSide;

public abstract class NetworkFunctionBase
{
    public abstract bool TryInvoke(in NetworkMessage msg);
}

public class NetworkFunction : NetworkFunctionBase
{
    public uint MessageType { get; init; }
    protected NetworkFunc _func;

    public NetworkFunction(uint messageType, NetworkFunc func)
    {
        MessageType = messageType;
        _func = func;
    }

    public override bool TryInvoke(in NetworkMessage msg)
    {
        _func();
        return true;
    }
}

public class NetworkFunction<TMsg> : NetworkFunctionBase
    where TMsg : unmanaged
{
    public uint MessageType { get; init; }
    protected NetworkFunc<TMsg> _func;

    public NetworkFunction(uint messageType, NetworkFunc<TMsg> func)
    {
        MessageType = messageType;
        _func = func;
    }

    public override bool TryInvoke(in NetworkMessage msg)
    {
        if (NetworkMessage.GetContentAs(in msg, out TMsg msgData))
        {
            _func(msgData);
            return true;
        }
        return false;
    }
}
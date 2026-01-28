#nullable enable

namespace Emotion.Network.Base.Invocation;

public abstract class NetworkFunctionBase<TThis, TSenderType>
{
    public abstract bool TryInvoke(TThis self, TSenderType sender, in NetworkMessage msg);
}

public class NetworkFunction<TThis, TSenderType> : NetworkFunctionBase<TThis, TSenderType>
{
    public uint MessageType { get; init; }
    protected NetworkFunc<TThis, TSenderType> _func;

    public NetworkFunction(uint messageType, NetworkFunc<TThis, TSenderType> func)
    {
        MessageType = messageType;
        _func = func;
    }

    public override bool TryInvoke(TThis self, TSenderType sender, in NetworkMessage msg)
    {
        _func(self, sender);
        return true;
    }
}

public class NetworkFunctionDirect<TThis, TSenderType> : NetworkFunctionBase<TThis, TSenderType>
{
    public uint MessageType { get; init; }
    private NetworkFunc<TThis, TSenderType, NetworkMessage> _func;

    public NetworkFunctionDirect(uint messageType, NetworkFunc<TThis, TSenderType, NetworkMessage> func)
    {
        MessageType = messageType;
        _func = func;
    }

    public override bool TryInvoke(TThis self, TSenderType sender, in NetworkMessage msg)
    {
        int contentLength = msg.ContentLength;
        if (contentLength > NetworkMessage.MaxContentSize) return false;
        _func(self, sender, msg);
        return true;
    }
}


public class NetworkFunction<TThis, TSenderType, TMsg> : NetworkFunctionBase<TThis, TSenderType>
    where TMsg : unmanaged
{
    public uint MessageType { get; init; }
    protected NetworkFunc<TThis, TSenderType, TMsg> _func;

    public NetworkFunction(uint messageType, NetworkFunc<TThis, TSenderType, TMsg> func)
    {
        MessageType = messageType;
        _func = func;
    }

    public override bool TryInvoke(TThis self, TSenderType sender, in NetworkMessage msg)
    {
        if (NetworkMessage.GetContentAs(in msg, out TMsg msgData))
        {
            _func(self, sender, msgData);
            return true;
        }
        return false;
    }
}
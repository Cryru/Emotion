#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;

namespace Emotion.Network.ServerSide;

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
        if (NetworkMessage.GetContentAs(in msg, out TMsg msgData, out _))
        {
            _func(self, sender, msgData);
            return true;
        }
        return false;
    }
}
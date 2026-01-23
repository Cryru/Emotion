#nullable enable

using Emotion;
using Emotion.Network.Base;
using Emotion.Standard.Serialization.XML;
using System.Runtime.InteropServices;

namespace Emotion.Network.Base.Invocation;

public abstract class NetworkFunctionBase<TThis, TSenderType>
{
    public abstract bool TryInvoke(TThis self, TSenderType? sender, in NetworkMessage msg);
}

public class NetworkFunction<TThis, TSenderType> : NetworkFunctionBase<TThis, TSenderType>
{
    public uint MessageType { get; init; }
    private NetworkFunc<TThis, TSenderType> _func;

    public NetworkFunction(uint messageType, NetworkFunc<TThis, TSenderType> func)
    {
        MessageType = messageType;
        _func = func;
    }

    public override bool TryInvoke(TThis self, TSenderType? sender, in NetworkMessage msg)
    {
        _func(self, sender);
        return true;
    }
}

public class NetworkFunction<TThis, TSenderType, TMsg> : NetworkFunctionBase<TThis, TSenderType>
    where TMsg: unmanaged
{
    public uint MessageType { get; init; }
    private NetworkFunc<TThis, TSenderType, TMsg> _func;

    public NetworkFunction(uint messageType, NetworkFunc<TThis, TSenderType, TMsg> func)
    {
        MessageType = messageType;
        _func = func;
    }

    public override unsafe bool TryInvoke(TThis self, TSenderType? sender, in NetworkMessage msg)
    {
        int contentLength = msg.ContentLength;
        if (contentLength > NetworkMessage.MaxContentSize) return false;
        if (contentLength != sizeof(TMsg)) return false; // Huh?

        fixed (byte* msgContentPtr = msg.Content)
        {
            Span<byte> data = new Span<byte>(msgContentPtr, contentLength);
            if (!MemoryMarshal.TryRead<TMsg>(data, out TMsg msgData)) return false;
            _func(self, sender, msgData);
        }

        return true;
    }
}

#nullable enable

using Emotion.Network.ServerSide;

namespace Emotion.Network.Base.Invocation;

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
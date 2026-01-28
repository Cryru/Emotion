#nullable enable

using Emotion.Network.ClientSide;
using System;
using System.Reflection;

namespace Emotion.Network.Base.Invocation;

public abstract class NetworkFunctionBase<TThis, TSenderType>
{
    public abstract bool TryInvoke(TThis self, TSenderType sender, in NetworkMessage msg);
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

public class SyncNetworkFunction<TMsg> : NetworkFunction<ClientBase, int, TMsg> where TMsg : unmanaged
{
    public SyncNetworkFunction(uint messageType, NetworkFunc<ClientBase, int, TMsg> func) : base(messageType, func)
    {
    }

    public override bool TryInvoke(ClientBase self, int sender, in NetworkMessage msg)
    {
        if (NetworkMessage.GetContentAs(in msg, out TMsg msgData))
        {
            self.GameTimeCoroutineManager.StartCoroutine(ExecuteSyncFunction(self, msg.GameTime, msgData, _func));
            return true;
        }
        return false;
    }

    private static IEnumerator ExecuteSyncFunction(ClientBase self, uint gameTime, TMsg msgData, NetworkFunc<ClientBase, int, TMsg> func)
    {
        uint diff = gameTime - (uint)self.GameTimeCoroutineManager.Time;
        if (diff > 0)
            yield return diff;
        func(self, 0, msgData);
    }
}

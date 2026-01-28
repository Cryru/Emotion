#nullable enable

using Emotion.Network.ClientSide;
using System.Runtime.CompilerServices;

namespace Emotion.Network.Base.Invocation;

public delegate void NetworkFunc<TThis, TSenderType, TMsg>(TThis self, TSenderType sender, in TMsg msg);
public delegate void NetworkFunc<TThis, TSenderType>(TThis self, TSenderType sender);

public class ClientNetworkFunctionInvoker : NetworkFunctionInvoker<ClientBase, int>
{
    public void RegisterSync<TEnum, TMsg>(TEnum messageType, NetworkFunc<ClientBase, int, TMsg> func)
        where TEnum : unmanaged, Enum
        where TMsg : unmanaged
    {
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new SyncNetworkFunction<TMsg>(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }

    public void RegisterSync<TMsg>(NetworkFunc<ClientBase, int, TMsg> func)
        where TMsg : unmanaged, INetworkMessageStruct
    {
        uint typAsUint = TMsg.MessageType;

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new SyncNetworkFunction<TMsg>(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }
}

public class NetworkFunctionInvoker<TThis, TSenderType>
{
    protected Dictionary<uint, NetworkFunctionBase<TThis, TSenderType>> _functions = new();

    public void Register<TEnum, TMsg>(TEnum messageType, NetworkFunc<TThis, TSenderType, TMsg> func)
        where TEnum : unmanaged, Enum
        where TMsg : unmanaged
    {
        Assert(Enum.GetUnderlyingType(typeof(TEnum)) == typeof(uint));
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new NetworkFunction<TThis, TSenderType, TMsg>(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }

    public void Register<TEnum>(TEnum messageType, NetworkFunc<TThis, TSenderType> func)
       where TEnum : unmanaged, Enum
    {
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new NetworkFunction<TThis, TSenderType>(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }

    public void RegisterDirect<TEnum>(TEnum messageType, NetworkFunc<TThis, TSenderType, NetworkMessage> func)
        where TEnum : unmanaged, Enum
    {
        Assert(Enum.GetUnderlyingType(typeof(TEnum)) == typeof(uint));
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        RegisterDirect(typAsUint, func);
    }

    public void RegisterDirect(uint messageType, NetworkFunc<TThis, TSenderType, NetworkMessage> func)
    {
        if (GetFunctionFromMessageType(messageType) != null) return;
        var netFunc = new NetworkFunctionDirect<TThis, TSenderType>(messageType, func);
        _functions.Add(messageType, netFunc);
    }

    public NetworkFunctionBase<TThis, TSenderType>? GetFunctionFromMessageType(uint messageType)
    {
        _functions.TryGetValue(messageType, out NetworkFunctionBase<TThis, TSenderType>? func);
        return func;
    }
}

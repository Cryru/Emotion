#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.LockStep;
using System.Runtime.CompilerServices;

namespace Emotion.Network.ClientSide;

public class NetworkFunctionInvoker
{
    protected Dictionary<uint, NetworkFunctionBase> _functions = new();

    public void Register<TEnum, TMsg>(TEnum messageType, NetworkFunc<TMsg> func)
        where TEnum : unmanaged, Enum
        where TMsg : unmanaged
    {
        Assert(Enum.GetUnderlyingType(typeof(TEnum)) == typeof(uint));
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new NetworkFunction<TMsg>(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }

    public void Register<TEnum>(TEnum messageType, NetworkFunc func)
       where TEnum : unmanaged, Enum
    {
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new NetworkFunction(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }

    public NetworkFunctionBase? GetFunctionFromMessageType(uint messageType)
    {
        _functions.TryGetValue(messageType, out NetworkFunctionBase? func);
        return func;
    }
}

public class ClientNetworkFunctionInvoker : NetworkFunctionInvoker
{
    public void RegisterLockStepFunc<TEnum, TMsg>(TEnum messageType, NetworkFunc<TMsg> func)
        where TEnum : unmanaged, Enum
        where TMsg : unmanaged
    {
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new LockStepNetworkFunction<TMsg>(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }

    public void RegisterLockStepFunc<TMsg>(NetworkFunc<TMsg> func)
        where TMsg : unmanaged, INetworkMessageStruct
    {
        uint typAsUint = TMsg.MessageType;

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new LockStepNetworkFunction<TMsg>(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }

    public void RegisterLockStepFunc<TEnum>(TEnum messageType, NetworkFunc func)
        where TEnum : unmanaged, Enum
    {
        uint typAsUint = Unsafe.As<TEnum, uint>(ref messageType);

        if (GetFunctionFromMessageType(typAsUint) != null) return;
        var netFunc = new LockStepNetworkFunction(typAsUint, func);
        _functions.Add(typAsUint, netFunc);
    }
}
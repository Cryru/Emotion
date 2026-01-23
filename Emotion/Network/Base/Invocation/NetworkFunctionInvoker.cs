#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Network.Base.Invocation;

public delegate void NetworkFunc<TThis, TSenderType, TMsg>(TThis self, TSenderType? sender, TMsg msg);
public delegate void NetworkFunc<TThis, TSenderType>(TThis self, TSenderType? sender);

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

    public NetworkFunctionBase<TThis, TSenderType>? GetFunctionFromMessageType(uint messageType)
    {
        _functions.TryGetValue(messageType, out NetworkFunctionBase<TThis, TSenderType>? func);
        return func;
    }
}

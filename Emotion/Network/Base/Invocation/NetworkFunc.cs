#nullable enable

namespace Emotion.Network.Base.Invocation;

public delegate void NetworkFunc<TThis, TSenderType, TMsg>(TThis self, TSenderType sender, in TMsg msg);
public delegate void NetworkFunc<TThis, TSenderType>(TThis self, TSenderType sender);
public delegate void NetworkFunc<TMsg>(in TMsg msg);
public delegate void NetworkFunc();
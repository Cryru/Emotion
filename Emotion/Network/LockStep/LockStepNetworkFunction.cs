#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.ClientSide;

namespace Emotion.Network.LockStep;

public class LockStepNetworkFunction<TMsg> : NetworkFunction<TMsg> where TMsg : unmanaged
{
    public LockStepNetworkFunction(uint messageType, NetworkFunc<TMsg> func) : base(messageType, func)
    {
    }

    public override bool TryInvoke(in NetworkMessage msg)
    {
        uint gameTime = msg.GameTime;

        // Update limit with the newest message
        if (gameTime <= Engine.CoroutineManagerGameTime.GameTimeAdvanceLimit)
        {
            Engine.Log.Warning($"Got a lockstep message from the past?", nameof(LockStepNetworkFunction<>), true);
            return false;
        }
        Engine.CoroutineManagerGameTime.SetGameTimeAdvanceLimit(gameTime);

        if (!NetworkMessage.GetContentAs(in msg, out TMsg msgData)) return false;
        Engine.CoroutineManagerGameTime.StartCoroutine(ExecuteSyncFunction(msg.GameTime, msgData, _func));
        return true;
    }

    private static IEnumerator ExecuteSyncFunction(uint gameTime, TMsg msgData, NetworkFunc<TMsg> func)
    {
        uint diff = gameTime - (uint)Engine.CoroutineManagerGameTime.Time;
        if (diff > 0)
            yield return diff;
        func(msgData);
    }

    public void InvokeOffline(in TMsg msgData)
    {
        Engine.CoroutineManagerGameTime.StartCoroutine(ExecuteSyncFunctionOffline(msgData, _func));
    }

    private static IEnumerator ExecuteSyncFunctionOffline(TMsg msgData, NetworkFunc<TMsg> func)
    {
        func(msgData);
        yield break;
    }
}

public class LockStepNetworkFunction : NetworkFunction
{
    public LockStepNetworkFunction(uint messageType, NetworkFunc func) : base(messageType, func)
    {
    }

    public override bool TryInvoke(in NetworkMessage msg)
    {
        uint gameTime = msg.GameTime;

        // Update limit with the newest message
        if (gameTime <= Engine.CoroutineManagerGameTime.GameTimeAdvanceLimit)
        {
            Engine.Log.Warning($"Got a lockstep message from the past?", nameof(LockStepNetworkFunction<>), true);
            return false;
        }
        Engine.CoroutineManagerGameTime.SetGameTimeAdvanceLimit(gameTime);

        Engine.CoroutineManagerGameTime.StartCoroutine(ExecuteSyncFunction(msg.GameTime, _func));
        return true;
    }

    private static IEnumerator ExecuteSyncFunction(uint gameTime, NetworkFunc func)
    {
        uint diff = gameTime - (uint)Engine.CoroutineManagerGameTime.Time;
        if (diff > 0)
            yield return diff;
        func();
    }

    public void InvokeOffline()
    {
        Engine.CoroutineManagerGameTime.StartCoroutine(ExecuteSyncFunctionOffline(_func));
    }

    private static IEnumerator ExecuteSyncFunctionOffline(NetworkFunc func)
    {
        func();
        yield break;
    }
}



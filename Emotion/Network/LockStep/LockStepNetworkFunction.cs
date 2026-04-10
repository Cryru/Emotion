#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.ClientSide;
using Emotion.Network.New.Base;

namespace Emotion.Network.LockStep;

public class LockStepNetworkFunction<TMsg> : NetworkFunction<TMsg> where TMsg : unmanaged
{
    public LockStepNetworkFunction(uint messageType, NetworkFunc<TMsg> func, string? friendlyName = null) : base(messageType, func, friendlyName)
    {
    }

    public override bool TryInvoke(in NetworkMessage msg)
    {
        uint gameTime = msg.GameTime;

        // Update limit with the newest message
        if (gameTime < Engine.CoroutineManagerGameTime.GameTimeAdvanceLimit)
        {
            Engine.Log.Warning($"Got a lockstep message from the past?", nameof(LockStepNetworkFunction<>), true);
            return true;
        }
        Engine.CoroutineManagerGameTime.SetGameTimeAdvanceLimit(gameTime);

        if (!NetworkMessage.GetContentAs(in msg, out TMsg msgData, out int msgDataHash)) return false;
        Engine.CoroutineManagerGameTime.StartCoroutine(ExecuteSyncFunction(msg.GameTime, msgData, msgDataHash, this));
        return true;
    }

    private static IEnumerator ExecuteSyncFunction(uint gameTime, TMsg msgData, int msgDataHash, LockStepNetworkFunction<TMsg> registeredFunc)
    {
        uint diff = gameTime - (uint)Engine.CoroutineManagerGameTime.Time;
        if (diff > 0)
            yield return diff;

        Engine.Multiplayer.LockStepVerify($"{registeredFunc.FriendlyString} {msgDataHash}");

        registeredFunc._func(msgData);
    }

    public void InvokeOffline(in TMsg msgData)
    {
        var coroutineScript = new NetworkFunctionCoroutineScript<TMsg>(_func, msgData);
        Engine.CoroutineManagerGameTime.StartCoroutineUntracked(coroutineScript);
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
        if (gameTime < Engine.CoroutineManagerGameTime.GameTimeAdvanceLimit)
        {
            Engine.Log.Warning($"Got a lockstep message from the past?", nameof(LockStepNetworkFunction<>), true);
            return true;
        }
        Engine.CoroutineManagerGameTime.SetGameTimeAdvanceLimit(gameTime);

        Engine.CoroutineManagerGameTime.StartCoroutine(ExecuteSyncFunction(msg.GameTime, this));
        return true;
    }

    private static IEnumerator ExecuteSyncFunction(uint gameTime, LockStepNetworkFunction registeredFunc)
    {
        uint diff = gameTime - (uint)Engine.CoroutineManagerGameTime.Time;
        if (diff > 0)
            yield return diff;

        Engine.Multiplayer.LockStepVerify(registeredFunc.FriendlyString);

        registeredFunc._func();
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



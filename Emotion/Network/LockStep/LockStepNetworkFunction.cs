#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.ClientSide;

namespace Emotion.Network.LockStep;

public class LockStepNetworkFunction<TMsg> : NetworkFunction<ClientBase, uint, TMsg> where TMsg : unmanaged
{
    public LockStepNetworkFunction(uint messageType, NetworkFunc<ClientBase, uint, TMsg> func) : base(messageType, func)
    {
    }

    public override bool TryInvoke(ClientBase self, uint gameTime, in NetworkMessage msg)
    {
        // Update limit with the newest message
        if (gameTime <= self.GameTimeCoroutineManager.GameTimeAdvanceLimit)
        {
            Engine.Log.Warning($"Got a lockstep message from the past?", nameof(LockStepNetworkFunction<>), true);
            return false;
        }
        self.GameTimeCoroutineManager.SetGameTimeAdvanceLimit(gameTime);

        if (!NetworkMessage.GetContentAs(in msg, out TMsg msgData)) return false;
        self.GameTimeCoroutineManager.StartCoroutine(ExecuteSyncFunction(self, msg.GameTime, msgData, _func));
        return true;
    }

    private static IEnumerator ExecuteSyncFunction(ClientBase self, uint gameTime, TMsg msgData, NetworkFunc<ClientBase, uint, TMsg> func)
    {
        uint diff = gameTime - (uint)self.GameTimeCoroutineManager.Time;
        if (diff > 0)
            yield return diff;
        func(self, gameTime, msgData);
    }
}

public class LockStepNetworkFunction : NetworkFunction<ClientBase, uint>
{
    public LockStepNetworkFunction(uint messageType, NetworkFunc<ClientBase, uint> func) : base(messageType, func)
    {
    }

    public override bool TryInvoke(ClientBase self, uint gameTime, in NetworkMessage msg)
    {
        // Update limit with the newest message
        if (gameTime <= self.GameTimeCoroutineManager.GameTimeAdvanceLimit)
        {
            Engine.Log.Warning($"Got a lockstep message from the past?", nameof(LockStepNetworkFunction<>), true);
            return false;
        }
        self.GameTimeCoroutineManager.SetGameTimeAdvanceLimit(gameTime);

        self.GameTimeCoroutineManager.StartCoroutine(ExecuteSyncFunction(self, msg.GameTime, _func));
        return true;
    }

    private static IEnumerator ExecuteSyncFunction(ClientBase self, uint gameTime, NetworkFunc<ClientBase, uint> func)
    {
        uint diff = gameTime - (uint)self.GameTimeCoroutineManager.Time;
        if (diff > 0)
            yield return diff;
        func(self, gameTime);
    }
}



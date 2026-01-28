#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.ServerSide;

namespace Emotion.Network.LockStep;

public class LockStepGameRoom : TickingServerRoom
{
    private float _lastTickRealTime;

    public LockStepGameRoom(ServerBase server, ServerPlayer? host, uint roomId) : base(server, host, roomId)
    {

    }

    protected override void OnGameplayTick(uint dt)
    {
        base.OnGameplayTick(dt);
        _lastTickRealTime = Engine.CurrentRealTime;
    }

    public static void RegisterLockStepEvent<TEnum>(NetworkFunctionInvoker<ServerRoom, ServerPlayer> invoker, TEnum messageType)
        where TEnum : unmanaged, Enum
    {
        invoker.RegisterDirect(messageType, Msg_LockStep);
    }

    public static void RegisterLockStepEvent(NetworkFunctionInvoker<ServerRoom, ServerPlayer> invoker, uint messageType)
    {
        invoker.RegisterDirect(messageType, Msg_LockStep);
    }

    private void OnLockStepMsg(in NetworkMessage msg)
    {
        // Resend message to all players
        float timeDiff = Engine.CurrentRealTime - _lastTickRealTime;
        timeDiff = Math.Min(timeDiff, GameTimeTickInterval - 1);
        uint timeDiffInt = (uint) Math.Floor(timeDiff);

        NetworkMessage msgCopy = msg;
        msgCopy.GameTime = GameTime + timeDiffInt;

        foreach (ServerPlayer player in UsersInside)
        {
            Server.SendMessageToPlayerRaw(player, msgCopy);
        }
    }

    protected static void Msg_LockStep(ServerRoom room, ServerPlayer sender, in NetworkMessage msg)
    {
        if (room is not LockStepGameRoom syncRoom) return;
        syncRoom.OnLockStepMsg(msg);
    }
}
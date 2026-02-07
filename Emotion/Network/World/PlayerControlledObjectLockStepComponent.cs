#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Components;
using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Network.LockStep;
using Emotion.Network.New.Base;
using Emotion.Network.ServerSide;
using System.Runtime.InteropServices;

namespace Emotion.Network.World;

public abstract class PlayerControlledObjectLockStepComponent<T> : IGameObjectComponent
    where T : unmanaged, INetworkMessageStruct, IPlayerControlledObjectLockStepComponentData<T>
{
    public int PlayerId { get; init; }
    protected uint _syncObjectId;
    protected GameObject? _obj;

    public PlayerControlledObjectLockStepComponent(int playerId, uint syncObjectId)
    {
        PlayerId = playerId;
        _syncObjectId = syncObjectId;
    }

    public Coroutine? Init(GameObject obj)
    {
        _obj = obj;
        Engine.Multiplayer.OnServerTick += Client_OnServerTick;

        return null;
    }

    public void Done(GameObject obj)
    {
        Engine.Multiplayer.OnServerTick -= Client_OnServerTick;
    }

    private void Client_OnServerTick()
    {
        // Send messages only from the player who controls this
        if (PlayerId != Engine.Multiplayer.PlayerId)
            return;

        // Object not added yet?
        if (_obj == null || _obj.ObjectId == 0)
            return;

        Engine.Multiplayer.SendLockStepMessage(GetPayload());
    }

    public static void RegisterNetFunctions()
    {
        ServerRoom.NetworkFunctions.RegisterLockStepEvent(T.MessageType);
        ClientBase.NetworkFunctions.RegisterLockStepFunc<T>(T.ProcessPayload);
    }

    protected abstract T GetPayload();
}

public class PlayerControlledObjectLockStepComponent : PlayerControlledObjectLockStepComponent<MessageObjectPositionUpdate>
{
    public PlayerControlledObjectLockStepComponent(int playerId, uint syncObjectId) : base(playerId, syncObjectId)
    {
    }

    protected override MessageObjectPositionUpdate GetPayload()
    {
        AssertNotNull(_obj);

        var msg = new MessageObjectPositionUpdate()
        {
            ObjectId = _syncObjectId,
            NewPosition = _obj.Position3D
        };
        return msg;
    }
}

public interface IPlayerControlledObjectLockStepComponentData<T>
{
    public abstract static void ProcessPayload(in T data);
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MessageObjectPositionUpdate : INetworkMessageStruct, IPlayerControlledObjectLockStepComponentData<MessageObjectPositionUpdate>
{
    public static uint MessageType => (uint)NetworkMessageType.PlayerControlledSyncObjectComponent_ObjectMoved;

    public uint ObjectId;
    public Vector3 NewPosition;

    public static void ProcessPayload(in MessageObjectPositionUpdate moved)
    {
        int tickInterval = 50;
        ServerRoomInfo? room = Engine.Multiplayer.InRoom;
        if (room != null)
            tickInterval = room.Value.TickInterval;

        SceneWithMap? currentGame = Engine.SceneManager.Current as SceneWithMap;
        GameObject? obj = currentGame?.Map.GetObjectById(moved.ObjectId);
        if (obj == null) return;
        obj.RotateToFacePoint(moved.NewPosition);
        obj.SetPosition3D(moved.NewPosition, tickInterval);
    }
}

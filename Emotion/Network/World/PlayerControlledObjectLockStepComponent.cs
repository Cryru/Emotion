#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Time;
using Emotion.Game.World.Components;
using Emotion.Game.World.Systems;
using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Network.LockStep;
using Emotion.Network.New.Base;
using Emotion.Network.ServerSide;
using System.Runtime.InteropServices;

namespace Emotion.Network.World;

public class PlayerControlledLockStepSystem<T> : WorldSystem<PlayerControlledObjectLockStepComponent<T>>, IWorldSimulationSystem
    where T : unmanaged, INetworkMessageStruct, IPlayerControlledObjectLockStepComponentData<T>
{
    private ValueTimer _updateInterval = new ValueTimer(50);

    protected override void InitInternal()
    {

    }

    protected override void DoneInternal()
    {

    }

    protected override void OnComponentListChanged()
    {

    }

    public void Update(float dt)
    {
        _updateInterval.Update(dt);
        if (!_updateInterval.Finished) return;
        _updateInterval.Reset();

        for (int i = 0; i < Components.Count; i++)
        {
            PlayerControlledObjectLockStepComponent<T> component = Components[i];

            // This player will send only updates for the objects they control
            if (component.PlayerId != Engine.Multiplayer.PlayerId) continue;

            Engine.Multiplayer.SendLockStepMessage(component.GetPayload());
        }
    }
}

public abstract class PlayerControlledObjectLockStepComponent<T> :
    IGameObjectComponent,
    ISystemicComponent<PlayerControlledObjectLockStepComponent<T>, PlayerControlledLockStepSystem<T>>
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

    public IRoutineWaiter? Init(GameObject obj)
    {
        _obj = obj;
        return null;
    }

    public void Done(GameObject obj)
    {

    }

    public static void RegisterNetFunctions()
    {
        ServerRoom.NetworkFunctions.RegisterLockStepEvent(T.MessageType);
        ClientBase.NetworkFunctions.RegisterLockStepFunc<T>(T.ProcessPayload, "PlayerControlledPayload");
    }

    public abstract T GetPayload();
}

public class PlayerControlledObjectLockStepComponent : PlayerControlledObjectLockStepComponent<MessageObjectPositionUpdate>
{
    public PlayerControlledObjectLockStepComponent(int playerId, uint syncObjectId) : base(playerId, syncObjectId)
    {
    }

    public override MessageObjectPositionUpdate GetPayload()
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

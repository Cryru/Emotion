#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Time;
using Emotion.Game.World.Components;
using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Network.LockStep;
using Emotion.Network.New.Base;
using Emotion.Network.ServerSide;
using System.Runtime.InteropServices;

namespace Emotion.Network.World;

public class PlayerControlledObjectLockStepComponent : IGameObjectComponent, IRenderableComponent, IUpdateableComponent
{
    public int PlayerId { get; init; }
    public Vector3 SyncPosition { get; private set; }

    private Vector3 _nextSyncPos;
    private GameObject? _obj;

    public PlayerControlledObjectLockStepComponent(int playerId)
    {
        PlayerId = playerId;
    }

    public Coroutine? Init(GameObject obj)
    {
        _obj = obj;
        SyncPosition = obj.Position3D;
        _nextSyncPos = SyncPosition;
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

        AssertNotNull(_obj);
        if (_obj.ObjectId == 0) return;

        var msg = new MessageObjectPositionUpdate()
        {
            ObjectId = _obj.ObjectId,
            NewPosition = _obj.Position3D
        };
        Engine.Multiplayer.SendLockStepMessage(msg, OnMsg_ObjectPositionUpdate);
    }

    public static void RegisterFunctionsServer(ServerNetworkFunctionInvoker<ServerRoom, ServerPlayer> invoker)
    {
        LockStepGameRoom.RegisterLockStepEvent(invoker, MessageObjectPositionUpdate.MessageType);
    }

    public static void RegisterFunctions(ClientNetworkFunctionInvoker invoker)
    {
        invoker.RegisterLockStepFunc<MessageObjectPositionUpdate>(OnMsg_ObjectPositionUpdate);
    }

    private static void OnMsg_ObjectPositionUpdate(in MessageObjectPositionUpdate moved)
    {
        SceneWithMap? currentGame = Engine.SceneManager.Current as SceneWithMap;
        GameObject? obj = currentGame?.Map.GetObjectById(moved.ObjectId);
        PlayerControlledObjectLockStepComponent? syncComponent = obj?.GetComponent<PlayerControlledObjectLockStepComponent>();
        syncComponent?._nextSyncPos = moved.NewPosition;
    }

    public void Render(Renderer r)
    {
        r.RenderLine(SyncPosition, SyncPosition + new Vector3(0, 0, 5), Color.Red, 0.2f);
    }

    public void Update(float dt)
    {
        SyncPosition = Interpolation.SmoothLerp(SyncPosition, _nextSyncPos, 5, dt);
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MessageObjectPositionUpdate : INetworkMessageStruct
{
    public static uint MessageType => (uint)NetworkMessageType.PlayerControlledSyncObjectComponent_ObjectMoved;

    public uint ObjectId;
    public Vector3 NewPosition;
}

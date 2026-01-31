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

public class SyncObjectCheckComponent : IGameObjectComponent
{
    public bool SyncAdded = false;

    private GameObject _obj = null!;

    public Coroutine? Init(GameObject obj)
    {
        _obj = obj;
        Engine.Multiplayer.OnServerTick += Client_OnServerTick;
        Engine.Multiplayer.SendLockStepMessage(new SyncObjectAddedMessageData() { ObjectId = obj.ObjectId });

        return null;
    }

    public void Done(GameObject obj)
    {
        Engine.Multiplayer.OnServerTick -= Client_OnServerTick;
    }

    private void Client_OnServerTick()
    {
        if (!SyncAdded) return;
        Engine.Multiplayer.SendMessageToServer(NetworkMessageType.LockStepVerify, new LockStepVerify($"{_obj.ObjectId}, {_obj.Name}, {_obj.Position3D}"));
    }

    public static void RegisterNetFunctions()
    {
        ServerRoom.NetworkFunctions.Register<NetworkMessageType, LockStepVerify>(NetworkMessageType.LockStepVerify, OnTimeSyncHash);

        ServerRoom.NetworkFunctions.RegisterLockStepEvent(SyncObjectAddedMessageData.MessageType);
        ClientBase.NetworkFunctions.RegisterLockStepFunc<SyncObjectAddedMessageData>(OnSyncObjectAdded);
    }

    private static void OnSyncObjectAdded(in SyncObjectAddedMessageData added)
    {
        ServerRoomInfo? room = Engine.Multiplayer.InRoom;
        if (room == null) return;

        SceneWithMap? currentGame = Engine.SceneManager.Current as SceneWithMap;
        GameObject? obj = currentGame?.Map.GetObjectById(added.ObjectId);
        SyncObjectCheckComponent? component = obj?.GetComponent<SyncObjectCheckComponent>();
        if (component == null) return;

        component.SyncAdded = true;
    }

    private static void OnTimeSyncHash(ServerRoom self, ServerPlayer sender, in LockStepVerify hash)
    {
        if (self is not LockStepGameRoom lockStepRoom) return;
        lockStepRoom.PlayerReportedHash(sender, hash);
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SyncObjectAddedMessageData : INetworkMessageStruct
{
    public static uint MessageType => (uint)NetworkMessageType.SyncObjectAdded;

    public uint ObjectId;
}
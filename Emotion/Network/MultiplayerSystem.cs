#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Network.New.Base;
using Emotion.Network.ServerSide;

namespace Emotion.Network;

public class MultiplayerSystem
{
    public int PlayerId { get => Client?.PlayerId ?? 0; }

    public ClientBase? Client { get; private set; }

    public void ConnectToServer(ClientBase client)
    {
        Client = client;
        client.ConnectIfNotConnected();
    }

    public void SendMessageToServer<TData>(in TData data)
        where TData : unmanaged, INetworkMessageStruct
    {
        Client?.SendMessageToServer(data);
    }

    public void SendMessageToServer<TEnum, TData>(TEnum messageType, in TData data)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        Client?.SendMessageToServer(messageType, data);
    }

    public void SendMessageToServerWithoutData<TEnum>(TEnum messageType)
        where TEnum : unmanaged
    {
        Client?.SendMessageToServerWithoutData(messageType);
    }

    #region LockStep

    public void SendSyncMessage<TData>(in TData data)
           where TData : unmanaged, INetworkMessageStruct
    {
        SendMessageToServer(data);
    }

    public void SendSyncMessage<TEnum, TData>(TEnum messageType, in TData data)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        SendMessageToServer(messageType, data);
    }

    public void SendSyncMessageWithoutData<TEnum>(TEnum messageType)
        where TEnum : unmanaged
    {
        SendMessageToServerWithoutData(messageType);
    }

    #endregion

    private ServerBase? _server;

    [Conditional("DEBUG")]
    public void DebugSelfHost(ServerBase server)
    {
        _server = server;
    }

    [Conditional("DEBUG")]
    public void DebugAddFakePlayerToRoom(Func<string, CoroutineManagerGameTime, ClientBase> createFunc)
    {
        if (Client == null) return;
        if (Client.InRoom == null) return;

        ServerRoomInfo room = Client.InRoom.Value;
        if (room.HostId != Client.PlayerId) return;

        CoroutineManagerGameTime secondPlayerManager = new CoroutineManagerGameTime();
        static IEnumerator SecondPlayerLoop(CoroutineManagerGameTime manager)
        {
            while (true)
            {
                manager.Update(Engine.DeltaTime);
                yield return null;
            }
        }
        Engine.CoroutineManager.StartCoroutine(SecondPlayerLoop(secondPlayerManager));
        ClientBase secondPlayerClient = createFunc(Client.EndPoint.ToString(), secondPlayerManager);
        secondPlayerClient.ConnectIfNotConnected();
        secondPlayerClient.SendMessageToServer(NetworkMessageType.JoinRoom, room.RoomId);
    }
}
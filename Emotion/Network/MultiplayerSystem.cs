#nullable enable

using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Network.LockStep;
using Emotion.Network.ServerSide;

namespace Emotion.Network;

public class MultiplayerSystem
{
    public ServerRoomInfo? InRoom { get; internal set; }

    public int PlayerId { get; internal set; }

    public ClientBase? Client { get; private set; }

    public string DebugMetricText
    {
        get
        {
            if (Client == null) return "Offline";
            return $"({Client.MetricText}) {Engine.CoroutineManagerGameTime.GameTimeAdvanceLimit - Engine.CoroutineManagerGameTime.Time}";
        }
    }

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

    public void SendLockStepMessage<TData>(in TData data)
        where TData : unmanaged, INetworkMessageStruct
    {
        if (Client != null)
        {
            SendMessageToServer(data);
            return;
        }

        NetworkFunctionBase? func = ClientBase.NetworkFunctions.GetFunctionFromMessageType(TData.MessageType);
        if (func is LockStepNetworkFunction<TData> lockStepFunc)
            lockStepFunc.InvokeOffline(data);
    }

    public void SendLockStepMessage<TEnum, TData>(TEnum messageType, in TData data)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        if (Client != null)
        {
            SendMessageToServer(messageType, data);
            return;
        }

        NetworkFunctionBase? func = ClientBase.NetworkFunctions.GetFunctionFromMessageType(messageType);
        if (func is LockStepNetworkFunction<TData> lockStepFunc)
            lockStepFunc.InvokeOffline(data);
    }

    public void SendLockStepMessageWithoutData<TEnum>(TEnum messageType)
        where TEnum : unmanaged
    {
        if (Client != null)
        {
            SendMessageToServerWithoutData(messageType);
            return;
        }

        NetworkFunctionBase? func = ClientBase.NetworkFunctions.GetFunctionFromMessageType(messageType);
        if (func is LockStepNetworkFunction lockStepFunc)
            lockStepFunc.InvokeOffline();
    }

    #endregion

    #region Internal

    public bool OnlineGame { get => InRoom != null; }

    internal void ClientRoomJoined(in ServerRoomInfo info)
    {
        InRoom = info;
        NetworkRand.SetNetworkRandState(info.RoomRandom);
    }

    #endregion

    #region Events

    public event Action? OnServerTick;
    public Action<ServerGameInfoList>? OnRoomListReceived;

    #endregion

    private ServerBase? _server;

    public void Debug_StartSelfHostedServer(ServerBase server)
    {
        _server = server;
    }

    internal void ServerTickReceived()
    {
        OnServerTick?.Invoke();
    }

    public void Update()
    {
        // If offline emulate server ticks being received.
        if (Client == null)
            ServerTickReceived();
    }
}
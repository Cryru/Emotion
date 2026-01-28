#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.New.Base;
using Emotion.Network.ServerSide;
using System.Net;
using System.Net.Sockets;

namespace Emotion.Network.ClientSide;

public class ClientBase : NetworkAgentBase
{
    public CoroutineManagerGameTime GameTimeCoroutineManager { get; init; }

    private int _sendMessageIndex = 1;
    private int _receiveMessageIndex = 0;

    public ClientBase(string serverIpAndPort, CoroutineManagerGameTime? gameTimeCoroutineManager) : base(IPEndPoint.Parse(serverIpAndPort), "Client")
    {
        GameTimeCoroutineManager = gameTimeCoroutineManager ?? Engine.CoroutineManagerGameTime;
    }

    public static NetworkMessage PEPEGICH;

    protected override void ProcessMessage(IPEndPoint from, ref Base.NetworkMessage msg)
    {
        if (_receiveMessageIndex - msg.MessageIndex != -1)
        {
            // todo: re-request, keep a buffer etc.
            Engine.Log.Trace($"Received old/future message from server. Discarding!", LogTag);
            return;
        }
        _receiveMessageIndex = msg.MessageIndex;

        uint messageType = msg.Type;
        NetworkFunctionBase<ClientBase, int>? netFunc = _netFuncs.GetFunctionFromMessageType(messageType);
        if (netFunc != null)
        {
            if (!netFunc.TryInvoke(this, 0, ref msg))
            {
                Engine.Log.Warning($"Error executing function of message type {messageType}", LogTag, true);
            }
            PEPEGICH = msg;
        }
        else
        {
            Engine.Log.Warning($"Unknown message type {messageType}", LogTag, true);
        }
    }

    #region Sender Helpers

    public void SendMessageToServer<TData>(in TData data)
        where TData : unmanaged, INetworkMessageStruct
    {
        SendMessageToIP(EndPoint, data, _sendMessageIndex);
        _sendMessageIndex++;
    }

    public void SendMessageToServer<TEnum, TData>(TEnum messageType, in TData data)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        SendMessageToIP(EndPoint, messageType, data, _sendMessageIndex);
        _sendMessageIndex++;
    }

    public void SendMessageToServerWithoutData<TEnum>(TEnum messageType)
        where TEnum : unmanaged
    {
        SendMessageToIPWithoutData(EndPoint, messageType, _sendMessageIndex);
        _sendMessageIndex++;
    }

    #endregion

    #region Connection

    public Action<bool>? OnConnectionChanged;

    public bool ConnectedToServer { get; protected set; }

    public int PlayerId { get; protected set; }

    public void ConnectIfNotConnected()
    {
        if (ConnectedToServer) return;

        _socket?.Dispose();
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(EndPoint);

        SendMessageToServerWithoutData(NetworkMessageType.RequestConnect);
    }

    #endregion

    #region Room

    public ServerRoomInfo? InRoom;
    public event Action? OnServerTick;

    //public Action<ServerRoomInfo>? OnRoomJoined;
    //public Action<ServerRoomInfo, int>? OnPlayerJoinedRoom;
    //public Action<List<ServerRoomInfo>>? OnRoomListReceived;

    protected virtual void InternalOnServerTick(uint gameTime)
    {
        OnServerTick?.Invoke();
    }

    #endregion

    #region Network Functions

    protected static ClientNetworkFunctionInvoker _netFuncs = new();

    static ClientBase()
    {
        RegisterFunctions(_netFuncs);
    }

    private static void RegisterFunctions(ClientNetworkFunctionInvoker invoker)
    {
        _netFuncs.Register<NetworkMessageType, int>(NetworkMessageType.Connected, Msg_Connected);
        _netFuncs.Register<NetworkMessageType, ServerRoomInfo>(NetworkMessageType.RoomJoined, Msg_RoomJoined);
        _netFuncs.Register<NetworkMessageType, ServerRoomInfo>(NetworkMessageType.UserJoinedRoom, Msg_UserJoinedRoom);
        _netFuncs.Register<NetworkMessageType, uint>(NetworkMessageType.ServerTick, Msg_OnServerTick);
    }

    #endregion

    #region Message Handlers

    private static void Msg_Connected(ClientBase self, int senderId, in int playerId)
    {
        self._sendMessageIndex = 1;
        self.PlayerId = playerId;
        self.ConnectedToServer = true;
        Engine.Log.Info($"Connected to server as user {playerId}", self.LogTag);
        self.OnConnectionChanged?.Invoke(true);
    }

    private static void Msg_RoomJoined(ClientBase self, int senderId, in ServerRoomInfo roomInfo)
    {
        self.InRoom = roomInfo;
    }

    private static void Msg_UserJoinedRoom(ClientBase self, int senderId, in ServerRoomInfo roomInfo)
    {
        self.InRoom = roomInfo;
    }

    private static void Msg_OnServerTick(ClientBase self, int senderId, in uint gameTime)
    {
        self.InternalOnServerTick(gameTime);
    }

    #endregion
}

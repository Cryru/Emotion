#nullable enable

using Emotion.Network.Base;
using Emotion.Network.LockStep;
using Emotion.Network.New.Base;
using Emotion.Network.ServerSide;
using System.Net;
using System.Net.Sockets;

namespace Emotion.Network.ClientSide;

public class ClientBase : NetworkAgentBase
{
    private int _sendMessageIndex = 1;
    private int _receiveMessageIndex = 0;

    public ClientBase(string serverIpAndPort) : base(IPEndPoint.Parse(serverIpAndPort), "Client")
    {
    }

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
        NetworkFunctionBase? netFunc = NetworkFunctions.GetFunctionFromMessageType(messageType);
        if (netFunc != null)
        {
            if (!netFunc.TryInvoke(in msg))
            {
                Engine.Log.Warning($"Error executing function of message type {messageType} ({netFunc})", LogTag, true);
            }
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

    //public Action<ServerRoomInfo>? OnRoomJoined;
    //public Action<ServerRoomInfo, int>? OnPlayerJoinedRoom;

    #endregion

    #region Network Functions

    public static ClientNetworkFunctionInvoker NetworkFunctions { get; } = new();

    static ClientBase()
    {
        NetworkFunctions.Register<NetworkMessageType, int>(NetworkMessageType.Connected, Msg_Connected);
        NetworkFunctions.Register<NetworkMessageType, ServerRoomInfo>(NetworkMessageType.RoomJoined, Msg_RoomJoined);
        NetworkFunctions.Register<NetworkMessageType, ServerRoomInfo>(NetworkMessageType.UserJoinedRoom, Msg_UserJoinedRoom);
        NetworkFunctions.Register<NetworkMessageType, ServerGameInfoList>(NetworkMessageType.RoomList, Msg_RoomListReceived);
        NetworkFunctions.RegisterLockStepFunc(NetworkMessageType.ServerTick, Msg_OnServerTick);
    }

    public static LockStepNetworkFunction? IsLockStepMessage(uint messageType)
    {
        NetworkFunctionBase? func = NetworkFunctions.GetFunctionFromMessageType(messageType);
        if (func is LockStepNetworkFunction lockStepFunc) return lockStepFunc;
        return null;
    }

    #endregion

    #region Message Handlers

    private static void Msg_Connected(in int playerId)
    {
        ClientBase? self = Engine.Multiplayer.Client;
        if (self == null) return;

        self._sendMessageIndex = 1; // Reset message index after connection (connection request msg doesn't count)
        Engine.Multiplayer.PlayerId = playerId;
        self.ConnectedToServer = true;
        Engine.Log.Info($"Connected to server as user {playerId}", self.LogTag);
        self.OnConnectionChanged?.Invoke(true);
    }

    private static void Msg_RoomJoined(in ServerRoomInfo roomInfo)
    {
        Engine.Multiplayer.ClientRoomJoined(in roomInfo);
    }

    private static void Msg_UserJoinedRoom(in ServerRoomInfo roomInfo)
    {
        Engine.Multiplayer.InRoom = roomInfo;
    }

    private static void Msg_RoomListReceived(in ServerGameInfoList roomInfo)
    {
        Engine.Multiplayer.OnRoomListReceived?.Invoke(roomInfo);
    }

    private static void Msg_OnServerTick()
    {
        Engine.Multiplayer.ServerTickReceived();
    }

    #endregion
}

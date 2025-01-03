using Emotion.Common.Serialization;
using Emotion.Network.Base;
using Emotion.Network.ServerSide;
using Emotion.Utility;
using System.Net;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

#nullable enable

namespace Emotion.Network.ClientSide;

public class Client : NetworkCommunicator
{
    public bool ConnectedToServer { get; protected set; }
    public int UserId { get; protected set; }

    public ServerRoomInfo? InRoom { get; protected set; }

    public IPEndPoint? _serverEndPoint { get; protected set; }

    public int SendMessageIndex = 1;
    public int ReceiveMessageIndex = 1;

    public Client()
    {
    }

    public static T CreateClient<T>(string serverIpAndPort) where T : Client, new()
    {
        var client = new T();

        IPEndPoint serverEndpoint = IPEndPoint.Parse(serverIpAndPort);
        client._serverEndPoint = serverEndpoint;

        client._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client._socket.Connect(serverEndpoint);

        client.Ip = serverEndpoint.Address.ToString();
        client.Port = serverEndpoint.Port;

        client.LogTag = "Client";

        client.SetupArgs();

        return client;
    }

    public static T CreateClient<T>(string serverIp, int serverPort) where T : Client, new()
    {
        return CreateClient<T>(serverIp + ":" + serverPort);
    }

    public void ConnectIfNotConnected()
    {
        if (ConnectedToServer) return;

        Span<byte> reqConnect = stackalloc byte[1];
        reqConnect[0] = (byte)NetworkMessageType.RequestConnect;
        SendMessageToServer(reqConnect);
    }

    public void SendMessageToServer(Span<byte> data)
    {
        AssertNotNull(_serverEndPoint);
        SendMessage(data, _serverEndPoint, SendMessageIndex);
        SendMessageIndex++;
    }

    public void SendMessageToServer(NetworkMessageType shorthand)
    {
        Span<byte> data = stackalloc byte[1];
        data[0] = (byte)shorthand;
        SendMessageToServer(data);
    }

    public void SendMessageToServer<T>(NetworkMessageType msgType, T msgInfo)
    {
        AssertNotNull(_serverEndPoint);
        SendMessage(_serverEndPoint, msgType, msgInfo, SendMessageIndex);
        SendMessageIndex++;
    }

    protected override void ProcessMessageInternal(NetworkMessage msg)
    {
        var reader = msg.GetContentReader();
        AssertNotNull(reader);
        if (reader.Length < 1) return;

        if (ReceiveMessageIndex > msg.Index)
        {
            Engine.Log.Trace($"Received old message from server. Discarding!", LogTag);
            return;
        }
        ReceiveMessageIndex = msg.Index;

        NetworkMessageType msgType = (NetworkMessageType)reader.ReadInt8();
        msg.MessageType = msgType;
        switch (msgType)
        {
            case NetworkMessageType.Connected:
                Msg_Connected(msg, reader);
                break;
            case NetworkMessageType.RoomJoined:
                Msg_RoomJoined(msg, reader);
                break;
            case NetworkMessageType.RoomList:
                Msg_RoomList(msg, reader);
                break;
            case NetworkMessageType.UserJoinedRoom:
                Msg_PlayerJoinedRoom(msg, reader);
                break;
            default:
                ClientProcessMessage(msg, reader);
                break;
        }
    }

    protected virtual void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        // nop
    }

    protected void Msg_Connected(NetworkMessage msg, ByteReader reader)
    {
        if (!NetworkMessage.TryReadXMLDataFromMessage(reader, out int userId)) return;

        AssertNotNull(msg.Sender);
        ConnectedToServer = true;
        UserId = userId;
        Engine.Log.Info($"Connected to server - {_serverEndPoint} as user {UserId}", LogTag);
        OnConnectionChanged?.Invoke(true);
    }

    protected void Msg_RoomJoined(NetworkMessage msg, ByteReader reader)
    {
        if (!NetworkMessage.TryReadXMLDataFromMessage(reader, out ServerRoomInfo? info)) return;
        AssertNotNull(info);
        InRoom = info;
        OnRoomJoined?.Invoke(info);
    }

    protected void Msg_RoomList(NetworkMessage msg, ByteReader reader)
    {
        if (!NetworkMessage.TryReadXMLDataFromMessage(reader, out List<ServerRoomInfo>? info)) return;
        AssertNotNull(info);
        OnRoomListReceived?.Invoke(info);
    }

    protected void Msg_PlayerJoinedRoom(NetworkMessage msg, ByteReader reader)
    {
        if (!NetworkMessage.TryReadXMLDataFromMessage(reader, out ServerRoomInfo? info)) return;
        AssertNotNull(info);
        InRoom = info;
        OnPlayerJoinedRoom?.Invoke(info, info.UsersInside[^1]);
    }

    public Action<bool> OnConnectionChanged;
    public Action<ServerRoomInfo> OnRoomJoined;
    public Action<ServerRoomInfo, int> OnPlayerJoinedRoom;
    public Action<List<ServerRoomInfo>> OnRoomListReceived;

    public void RequestJoinRoom(int roomId)
    {
        SendMessageToServer(NetworkMessageType.JoinRoom, roomId);
    }

    public void RequestRoomList()
    {
        SendMessageToServer(NetworkMessageType.GetRooms);
    }

    public void RequestHostRoom()
    {
        SendMessageToServer(NetworkMessageType.HostRoom);
    }
}

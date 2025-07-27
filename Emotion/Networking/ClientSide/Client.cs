using Emotion.Network.Base;
using Emotion.Networking.Base;
using System.Net;

#nullable enable

namespace Emotion.Networking.ClientSide;

public class Client : NetworkAgentBase
{
    private int _sendMessageIndex = 1;
    private int _receiveMessageIndex = 1;

    public Client(string serverAddress) : base(serverAddress, Engine.CoroutineManager)
    {
        RegisterFunction<int>((uint) NetworkMessageType.Connected, Msg_Connected);
        RegisterFunction<int>((uint) NetworkMessageType.RoomJoined, Msg_RoomJoined);
    }

    protected override void ProcessMessage(IPEndPoint from, NetworkMessageData msg)
    {
        if (_receiveMessageIndex > msg.MessageIndex)
        {
            // todo: re-request, keep a buffer etc.
            Engine.Log.Trace($"Received old message from server. Discarding!", LogTag);
            return;
        }
        _receiveMessageIndex = msg.MessageIndex;

        CallNetFunc(null, msg);
    }

    #region Connection

    public Action<bool>? OnConnectionChanged;

    public bool ConnectedToServer { get; protected set; }

    public int PlayerId { get; protected set; }

    public void ConnectIfNotConnected()
    {
        if (ConnectedToServer) return;

        NetworkMessageData connectMsg = NetworkMessageHelpers.CreateMessage((uint) NetworkMessageType.RequestConnect);
        SendMessageToServer(connectMsg);
    }

    private void Msg_Connected(int playerId)
    {
        PlayerId = playerId;
        ConnectedToServer = true;
        Engine.Log.Info($"Connected to server as user {PlayerId}", LogTag);
        OnConnectionChanged?.Invoke(true);
    }

    #endregion

    #region Room

    public int InRoom;

    //public Action<ServerRoomInfo>? OnRoomJoined;
    //public Action<ServerRoomInfo, int>? OnPlayerJoinedRoom;
    //public Action<List<ServerRoomInfo>>? OnRoomListReceived;

    private void Msg_RoomJoined(int roomInfo)
    {
        InRoom = roomInfo;
    }

    #endregion

    #region Message Sending

    public void SendMessageToServer(NetworkMessageData msg)
    {
        SendMessageToIP(EndPoint, msg, _sendMessageIndex);
        _sendMessageIndex++;
    }

    #endregion
}

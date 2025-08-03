#nullable enable

using Emotion.Network.Base;
using Emotion.Network.ServerSide;
using Emotion.Network.ServerSide.Gameplay;

namespace Emotion.Network.TimeSyncMessageBroker;

[DontSerialize]
public partial class MsgBrokerTimeSyncGameplay : ServerTimeSyncRoomGameplay
{
    public MsgBrokerTimeSyncServer TimeSyncServer = null!;

    private List<NetworkMessage> _messagesForNextTick = new List<NetworkMessage>();

    public override void BindToServer(Server server, ServerRoom room)
    {
        base.BindToServer(server, room);
        TimeSyncServer = (server as MsgBrokerTimeSyncServer)!;
    }

    public override void OnMessageReceived(ServerUser sender, NetworkMessage msg)
    {
        if (msg.MessageType == NetworkMessageType.TimeSyncHash || msg.MessageType == NetworkMessageType.TimeSyncHashDebug)
        {
            AddHashMessage(sender, msg);
        }
        else
        {
            msg.AutoFree = false;
            _messagesForNextTick.Add(msg);
        }
    }

    protected override void OnGameplayMessageReceived(ServerUser sender, NetworkMessage msg)
    {

    }

    protected override void OnGamelayTimeReset()
    {
        // In case messages are stuck from before.
        // todo: is this needed? I don't think these rooms are reused
        for (int i = 0; i < _messagesForNextTick.Count; i++)
        {
            NetworkMessage msgInstance = _messagesForNextTick[i];
            NetworkMessage.Shared.Return(msgInstance);
        }
        _messagesForNextTick.Clear();
    }

    protected override void OnGameplayTick(float dt)
    {
        VerifyTimeHashes();

        for (int m = 0; m < _messagesForNextTick.Count; m++)
        {
            NetworkMessage msgInstance = _messagesForNextTick[m];
            Assert(msgInstance.Valid);
            TimeSyncServer.ResendMessageWithTime(Room.UsersInside, CurrentGameTime, msgInstance);
            NetworkMessage.Shared.Return(msgInstance);
        }
        _messagesForNextTick.Clear();
    }

    #region Time Hash

    private List<TimeHashPair> _hashPairs = new List<TimeHashPair>();

    private void AddHashMessage(ServerUser sender, NetworkMessage msg)
    {
        ByteReader reader = msg.GetContentReader()!;
        reader.ReadByte(); // message type.

        int hash;
        string hashMeta = string.Empty;
        if (msg.MessageType == NetworkMessageType.TimeSyncHashDebug)
        {
            int methodNameLength = reader.ReadInt32();
            var methodNameBytes = reader.ReadBytes(methodNameLength);
            hashMeta = System.Text.Encoding.ASCII.GetString(methodNameBytes);
            hash = methodNameBytes.GetStableHashCode();
        }
        else
        {
            hash = reader.ReadInt32();
        }

        var found = false;
        for (int i = 0; i < _hashPairs.Count; i++)
        {
            TimeHashPair hashPair = _hashPairs[i];
            if (!hashPair.IsFull() && !hashPair.ContainsUser(sender))
            {
                hashPair.AddHash(sender, hash, hashMeta);
                found = true;
                break;
            }
        }

        if (!found)
        {
            var newPair = new TimeHashPair(Room.UsersInside.Count);
            newPair.AddHash(sender, hash, hashMeta);
            _hashPairs.Add(newPair);
        }

    }

    private void VerifyTimeHashes()
    {
        // todo: investigate why hashes might be out of order
        for (int i = _hashPairs.Count - 1; i >= 0; i--)
        {
            var hashPair = _hashPairs[i];
            if (!hashPair.IsFull()) continue;

            bool success = hashPair.Verify();
            if (!success) Errors++;
            _hashPairs.RemoveAt(i);
        }
    }

    #endregion
}

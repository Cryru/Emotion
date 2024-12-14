using Emotion.Game.Time.Routines;
using Emotion.Network.Base;
using Emotion.Network.ServerSide;
using System.Collections;

#nullable enable

namespace Emotion.Network.TimeSyncMessageBroker;

public partial class TimeSyncedServerRoom
{
    public MsgBrokerServerTimeSync Server;
    public ServerRoom Room;

    public int Errors;

    public int CurrentGameTime = 0;
    public int GameTimeAdvancePerTick = 20;

    private List<NetworkMessage> _messagesForNextTick = new List<NetworkMessage>();
    private Coroutine _gameTimeRoutine = Coroutine.CompletedRoutine;
    private List<TimeHashPair> _hashPairs = new List<TimeHashPair>();

    public TimeSyncedServerRoom(MsgBrokerServerTimeSync server, ServerRoom room)
    {
        Server = server;
        Room = room;
    }

    public void AddMessageForNextTick(NetworkMessage msg)
    {
        Assert(!msg.AutoFree);
        Assert(msg.Valid);

        if (msg.MessageType == NetworkMessageType.TimeSyncHash || msg.MessageType == NetworkMessageType.TimeSyncHashDebug)
        {
            Utility.ByteReader reader = msg.GetContentReader()!;
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

            System.Net.IPEndPoint? userIp = msg.Sender;
            if (userIp != null && Server.IPToUser.TryGetValue(userIp, out ServerUser? user))
            {
                var found = false;
                for (int i = 0; i < _hashPairs.Count; i++)
                {
                    TimeHashPair hashPair = _hashPairs[i];
                    if (!hashPair.IsFull() && !hashPair.ContainsUser(user))
                    {
                        hashPair.AddHash(user, hash, hashMeta);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var newPair = new TimeHashPair(Room.UsersInside.Count);
                    newPair.AddHash(user, hash, hashMeta);
                    _hashPairs.Add(newPair);
                }
            }
            msg.AutoFree = true;
            return;
        }

        _messagesForNextTick.Add(msg);
    }

    public void StartGameTime(CoroutineManager coroutineManager)
    {
        if (!_gameTimeRoutine.Finished) coroutineManager.StopCoroutine(_gameTimeRoutine);

        CurrentGameTime = 0;

        // In case messages are stuck from before.
        // todo: is this needed? I don't think these rooms are reused
        for (int i = 0; i < _messagesForNextTick.Count; i++)
        {
            NetworkMessage msgInstance = _messagesForNextTick[i];
            NetworkMessage.Shared.Return(msgInstance);
        }
        _messagesForNextTick.Clear();

        _gameTimeRoutine = coroutineManager.StartCoroutine(TickRoutine());
    }

    protected IEnumerator TickRoutine()
    {
        while (Room.Active && Room.ServerData == this)
        {
            yield return GameTimeAdvancePerTick;

            // Verify hashes for this slice.
            for (int i = _hashPairs.Count - 1; i >= 0; i--)
            {
                var hashPair = _hashPairs[i];
                if (!hashPair.IsFull()) continue;

                bool success = hashPair.Verify();
                if (!success) Errors++;
                _hashPairs.RemoveAt(i);
            }

            for (int m = 0; m < _messagesForNextTick.Count; m++)
            {
                NetworkMessage msgInstance = _messagesForNextTick[m];
                Assert(msgInstance.Valid);
                Server.BroadcastMessageWithTime(Room.UsersInside, CurrentGameTime, msgInstance);
                NetworkMessage.Shared.Return(msgInstance);
            }
            _messagesForNextTick.Clear();

            int nextTime = CurrentGameTime + GameTimeAdvancePerTick;
            Server.BroadcastAdvanceTimeMessage(Room.UsersInside, nextTime);

            CurrentGameTime += GameTimeAdvancePerTick;
        }
    }
}

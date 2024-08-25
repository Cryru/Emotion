using Emotion.Network.Base;
using Emotion.Network.ServerSide;

#nullable enable

namespace Emotion.Network.TimeSyncMessageBroker;

public struct NetworkMsgWithTime
{
    public ServerUser Sender;
    public int Time;
    public int SortOffset;
    public NetworkMessage Message;

    public string DebugData;

    public override string ToString()
    {
        return $"Msg @ {Time} from {Sender} ({DebugData})";
    }
}

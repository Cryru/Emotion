using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;

namespace Emotion.Network.ServerSide;

public class LockStepGameRoom : TickingServerRoom
{
    private byte[] _messagesThisTickData = new byte[NetworkMessage.MaxContentSize * 10];
    private int _messagesThisTickPointer = 0;
    private List<SyncMessageRecord> _messagesThisTickLengths = new();

    private struct SyncMessageRecord
    {
        public uint MessageType;
        public int Length;
    }

    public LockStepGameRoom(ServerBase server, ServerPlayer? host, uint roomId) : base(server, host, roomId)
    {

    }

    protected override void OnGameplayTick(uint dt)
    {
        base.OnGameplayTick(dt);

        uint currentTime = GameTime;
        int curPtr = 0;
        for (int i = 0; i < _messagesThisTickLengths.Count; i++)
        {
            SyncMessageRecord record = _messagesThisTickLengths[i];
            int length = record.Length;

            // Create message to send to all users
            var data = new ReadOnlySpan<byte>(_messagesThisTickData, curPtr, length);
            NetworkMessage msg = NetworkAgentBase.CreateMessage(record.MessageType, data);
            msg.GameTime = currentTime;

            foreach (ServerPlayer player in UsersInside)
            {
                Server.SendMessageToPlayerRaw(player, msg);
            }

            curPtr += length;
        }

        _messagesThisTickLengths.Clear();
        _messagesThisTickPointer = 0;
    }

    public static void RegisterSyncEvent<TEnum>(NetworkFunctionInvoker<ServerRoom, ServerPlayer> invoker, TEnum messageType)
        where TEnum : unmanaged, Enum
    {
        invoker.RegisterDirect(messageType, SyncMessageGeneric);
    }

    public static void RegisterSyncEvent(NetworkFunctionInvoker<ServerRoom, ServerPlayer> invoker, uint messageType)
    {
        invoker.RegisterDirect(messageType, SyncMessageGeneric);
    }

    private unsafe void SyncMessageGenericFired(in NetworkMessage msg)
    {
        int newEnd = _messagesThisTickPointer + msg.ContentLength;
        if (newEnd > _messagesThisTickData.Length)
        {
            Array.Resize(ref _messagesThisTickData, newEnd + NetworkMessage.MaxContentSize);
        }

        Span<byte> thisTickSpan = _messagesThisTickData.AsSpan().Slice(_messagesThisTickPointer);
        fixed (byte* pContent = msg.Content)
        {
            Span<byte> msgContentSpan = new Span<byte>(pContent, msg.ContentLength);
            msgContentSpan.CopyTo(thisTickSpan);
        }
        _messagesThisTickLengths.Add(new SyncMessageRecord() { Length = msg.ContentLength, MessageType = msg.Type });
        _messagesThisTickPointer = newEnd;
    }

    protected static void SyncMessageGeneric(ServerRoom room, ServerPlayer sender, in NetworkMessage msg)
    {
        if (room is not LockStepGameRoom syncRoom) return;
        syncRoom.SyncMessageGenericFired(msg);
    }
}
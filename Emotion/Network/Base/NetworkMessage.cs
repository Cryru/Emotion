#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Network.Base;

[DontSerialize]
public unsafe struct NetworkMessage
{
    public const int MagicNumber = (byte) 'E' << 24 | (byte)'M' << 16 | (byte)'O' << 8 | (byte)'N';
    public const int MaxMessageSize = 1000 * 10; // 10KB
    public const int SizeWithoutContent = sizeof(int) + sizeof(int) + sizeof(int) + sizeof(uint) + sizeof(uint) + sizeof(uint) + sizeof(int);
    public const int MaxContentSize = MaxMessageSize - SizeWithoutContent;
    public const int HashOffset = sizeof(int);

    public int Magic; // Should match MagicNumber and is used as a sanity check
    public int Hash;
    public int MessageIndex;
    public uint Type;

    public uint GameTime;
    public uint Reserved;

    public int ContentLength;
    public fixed byte Content[NetworkMessage.MaxContentSize];

    public static bool GetContentAs<TMsg>(in NetworkMessage msg, out TMsg castStruct) where TMsg : unmanaged
    {
        castStruct = default;

        int contentLength = msg.ContentLength;
        if (contentLength > NetworkMessage.MaxContentSize) return false;
        if (contentLength != sizeof(TMsg)) return false; // Huh?

        fixed (byte* msgContentPtr = msg.Content)
        {
            var data = new Span<byte>(msgContentPtr, contentLength);
            castStruct = MemoryMarshal.Read<TMsg>(data);
            return true;
        }
    }
}
namespace Emotion.Network.Base;

#nullable enable

public enum NetworkMessageType : byte
{
    None,

    // Server -> Client
    Connected,

    // Client -> Server
    RequestConnect
}

namespace Emotion.Network.Base;

#nullable enable

public enum NetworkMessageType : byte
{
    None,
    Generic, // Wildcard

    // Server -> Client
    Connected,

    // Client -> Server
    RequestConnect
}

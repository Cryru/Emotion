namespace Emotion.Network.Base;

#nullable enable

public enum NetworkMessageType : uint
{
    None,
    GenericGameplay,         // Wildcard
    GenericGameplayWithTime, // Wildcard for Time Sync events

    // Server -> Client
    Connected,      // from RequestConnect
    Error_AlreadyConnected, // from RequestConnect
    Error_NotInRoom,      // from GetRoomInfo
    RoomJoined,     // from HostRoom, JoinRoom              <ServerRoomInfo>
    UserJoinedRoom, // from JoinRoom of another User        <ServerRoomInfo>
    RoomInfo,       // from GetRoomInfo                     <ServerRoomInfo>
    RoomList,       // from GetRooms                        <List<ServerRoomInfo>>

    // Client -> Server
    RequestConnect,
    HostRoom,
    GetRoomInfo,
    GetRooms,
    JoinRoom,       // <int> roomId

    // Specials (Client -> Server)
    TimeSyncHash,        // <int> hash
    TimeSyncHashDebug,    // <string> hash

    // Specials (Server -> Client)
    NIY_AdvanceTime,

    Last
}

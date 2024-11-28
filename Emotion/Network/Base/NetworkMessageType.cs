namespace Emotion.Network.Base;

#nullable enable

public enum NetworkMessageType : byte
{
    None,
    Generic,        // Wildcard

    // Server -> Client
    Connected,      // from RequestConnect
    NotInRoom,      // from GetRoomInfo
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

    // Specials
    TimeSyncHash,        // <int> hash
    TimeSyncHashDebug    // <string> hash
}

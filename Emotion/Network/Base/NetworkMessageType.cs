#nullable enable

namespace Emotion.Network.New.Base;

public enum NetworkMessageType : uint
{
    None,

    // Client -> Server
    RequestConnect,
    HostRoom,
    GetRoomInfo,
    GetRooms,
    JoinRoom,       // <uint> roomId

    // Server -> Client
    Connected,      // from RequestConnect
    Error_AlreadyConnected, // from RequestConnect
    Error_NotInRoom,      // from GetRoomInfo
    Error_RoomNotFound, // from JoinRoom
    RoomJoined,     // from HostRoom, JoinRoom              <ServerRoomInfo>
    UserJoinedRoom, // from JoinRoom of another User        <ServerRoomInfo>
    RoomInfo,       // from GetRoomInfo                     <ServerRoomInfo>
    RoomList,       // from GetRooms                        <ServerGameInfoList>

    // LockStep (Client -> Server)
    LockStepVerify,        // <int> hash
    SyncObjectAdded,     // <int> objectId

    // Gameplay (Server -> Client)
    ServerTick,

    // Implementations
    PlayerControlledSyncObjectComponent_ObjectMoved,

    Last
}

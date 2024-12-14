using Emotion.Game.Time.Routines;
using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ClientSide;
using Emotion.Network.ServerSide;
using Emotion.Network.TimeSyncMessageBroker;
using Emotion.Standard.XML;
using Emotion.Testing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

#nullable enable

namespace Tests.EngineTests;

[Test]
public class NetworkTests
{
    private IEnumerator WaitUntilTrueOrTimeout(Action updateFunc, Func<bool> check, int timeout)
    {
        updateFunc();

        const int timeStep = 20;

        int timeWaited = 0;
        while (!check() && timeWaited < timeout)
        {
            yield return timeStep;
            timeWaited += timeStep;

            updateFunc();
        }
    }

    const int NETWORK_TIMEOUT = 1000;

    [Test]
    public IEnumerator MsgBroker()
    {
        MsgBrokerServer server = Server.CreateServer<MsgBrokerServer>(1337);
        MsgBrokerClient clientHost = Client.CreateClient<MsgBrokerClient>("127.0.0.1:1337");
        MsgBrokerClient? client = null;

        Action updateFunc = () => { server.Update(); clientHost.Update(); client?.Update(); };

        // Connect to server
        {
            clientHost.ConnectIfNotConnected();
            bool connectChangedEventRaised = false;
            clientHost.OnConnectionChanged = (connected) =>
            {
                connectChangedEventRaised = true;
            };

            yield return WaitUntilTrueOrTimeout(updateFunc, () => clientHost.ConnectedToServer, NETWORK_TIMEOUT);
            Assert.True(clientHost.ConnectedToServer);
            Assert.True(connectChangedEventRaised);
        }

        // Create room
        {
            bool roomJoinedEventRaised = false;
            clientHost.OnRoomJoined = (room) =>
            {
                roomJoinedEventRaised = true;
            };
            clientHost.RequestHostRoom();

            yield return WaitUntilTrueOrTimeout(updateFunc, () => clientHost.InRoom != null, NETWORK_TIMEOUT);
            Assert.True(roomJoinedEventRaised);

            ServerRoomInfo room = clientHost.InRoom;
            Assert.NotNull(room);
            Assert.True(room.HostUser == clientHost.UserId);
            Assert.True(room.UsersInside.Length == 1);
        }

        // Make second client, and connect it.
        client = Client.CreateClient<MsgBrokerClient>("127.0.0.1:1337");
        client.ConnectIfNotConnected();
        yield return WaitUntilTrueOrTimeout(updateFunc, () => client.ConnectedToServer, NETWORK_TIMEOUT);
        Assert.True(client.ConnectedToServer);

        // Get room list
        client.RequestRoomList();
        List<ServerRoomInfo>? roomsReceived = null;
        client.OnRoomListReceived = (rooms) =>
        {
            roomsReceived = rooms;
        };
        yield return WaitUntilTrueOrTimeout(updateFunc, () => roomsReceived != null, NETWORK_TIMEOUT);
        Assert.NotNull(roomsReceived);
        Assert.True(roomsReceived.Count == 1);
        Assert.True(roomsReceived[0].HostUser == clientHost.UserId);
        Assert.True(roomsReceived[0].UsersInside.Length == 1);

        // Join the room.
        {
            bool otherPlayerJoinedRoomEventHostSide = false;
            bool otherPlayedJoinedRoomEvent = false;
            clientHost.OnPlayerJoinedRoom = (room, id) =>
            {
                otherPlayerJoinedRoomEventHostSide = true;
            };
            client.OnRoomJoined = (room) =>
            {
                otherPlayedJoinedRoomEvent = true;
            };

            client.RequestJoinRoom(roomsReceived[0].Id);
            yield return WaitUntilTrueOrTimeout(updateFunc, () => otherPlayedJoinedRoomEvent, NETWORK_TIMEOUT);

            Assert.True(otherPlayedJoinedRoomEvent);
            Assert.True(otherPlayerJoinedRoomEventHostSide);

            Assert.NotNull(clientHost.InRoom);
            Assert.NotNull(client.InRoom);
            Assert.Equal(clientHost.InRoom.UsersInside.Length, 2);
            Assert.Equal(client.InRoom.UsersInside.Length, 2);
        }

        Vector3 lastVec3ReceivedByHost = Vector3.Zero;

        clientHost.RegisterFunction<Vector3>("testmethod", (vec3) =>
        {
            lastVec3ReceivedByHost = vec3;
        });

        Vector3 lastVec3ReceivedByGuest = Vector3.Zero;

        client.RegisterFunction<Vector3>("testmethod", (vec3) =>
        {
            lastVec3ReceivedByGuest = vec3;
        });

        client.SendBrokerMsg("testmethod", XMLFormat.To(new Vector3(1, 2, 3)));

        yield return WaitUntilTrueOrTimeout(updateFunc, () => lastVec3ReceivedByHost != Vector3.Zero, NETWORK_TIMEOUT);

        Assert.Equal(lastVec3ReceivedByHost, new Vector3(1, 2, 3));
        Assert.Equal(lastVec3ReceivedByGuest, new Vector3(0, 0, 0)); // In MsgBroker mode only the other clients receive the message.

        server.Dispose();
        client.Dispose();
        clientHost.Dispose();
    }

    [Test]
    public IEnumerator TimeSync()
    {
        MsgBrokerServerTimeSync server = Server.CreateServer<MsgBrokerServerTimeSync>(1337);
        MsgBrokerClientTimeSync clientHost = Client.CreateClient<MsgBrokerClientTimeSync>("127.0.0.1:1337");
        clientHost.CoroutineManager = new CoroutineManagerGameTime();
        MsgBrokerClientTimeSync? client = null;

        Action updateFunc = () => { server.Update(); clientHost.Update(); client?.Update(); };

        // Connect to server
        {
            clientHost.ConnectIfNotConnected();
            bool connectChangedEventRaised = false;
            clientHost.OnConnectionChanged = (connected) =>
            {
                connectChangedEventRaised = true;
            };

            yield return WaitUntilTrueOrTimeout(updateFunc, () => clientHost.ConnectedToServer, NETWORK_TIMEOUT);
            Assert.True(clientHost.ConnectedToServer);
            Assert.True(connectChangedEventRaised);
        }

        // Create room
        {
            bool roomJoinedEventRaised = false;
            clientHost.OnRoomJoined = (room) =>
            {
                roomJoinedEventRaised = true;
            };
            clientHost.RequestHostRoom();

            yield return WaitUntilTrueOrTimeout(updateFunc, () => clientHost.InRoom != null, NETWORK_TIMEOUT);
            Assert.True(roomJoinedEventRaised);

            ServerRoomInfo? room = clientHost.InRoom;
            Assert.NotNull(room);
            Assert.True(room.HostUser == clientHost.UserId);
            Assert.True(room.UsersInside.Length == 1);
        }

        // Make second client, and connect it.
        client = Client.CreateClient<MsgBrokerClientTimeSync>("127.0.0.1:1337");
        client.CoroutineManager = new CoroutineManagerGameTime();
        client.ConnectIfNotConnected();
        yield return WaitUntilTrueOrTimeout(updateFunc, () => client.ConnectedToServer, NETWORK_TIMEOUT);
        Assert.True(client.ConnectedToServer);

        // Get room list
        client.RequestRoomList();
        List<ServerRoomInfo>? roomsReceived = null;
        client.OnRoomListReceived = (rooms) =>
        {
            roomsReceived = rooms;
        };
        yield return WaitUntilTrueOrTimeout(updateFunc, () => roomsReceived != null, NETWORK_TIMEOUT);
        Assert.NotNull(roomsReceived);
        Assert.True(roomsReceived.Count == 1);
        Assert.True(roomsReceived[0].HostUser == clientHost.UserId);
        Assert.True(roomsReceived[0].UsersInside.Length == 1);

        // Join the room.
        {
            bool otherPlayerJoinedRoomEventHostSide = false;
            bool otherPlayedJoinedRoomEvent = false;
            clientHost.OnPlayerJoinedRoom = (room, id) =>
            {
                otherPlayerJoinedRoomEventHostSide = true;
            };
            client.OnRoomJoined = (room) =>
            {
                otherPlayedJoinedRoomEvent = true;
            };

            client.RequestJoinRoom(roomsReceived[0].Id);
            yield return WaitUntilTrueOrTimeout(updateFunc, () => otherPlayedJoinedRoomEvent, NETWORK_TIMEOUT);
            yield return WaitUntilTrueOrTimeout(updateFunc, () => otherPlayerJoinedRoomEventHostSide, NETWORK_TIMEOUT);

            Assert.True(otherPlayedJoinedRoomEvent);
            Assert.True(otherPlayerJoinedRoomEventHostSide);

            Assert.NotNull(clientHost.InRoom);
            Assert.NotNull(client.InRoom);
            Assert.Equal(clientHost.InRoom.UsersInside.Length, 2);
            Assert.Equal(client.InRoom.UsersInside.Length, 2);
        }

        Vector3 lastVec3ReceivedByHost = Vector3.Zero;

        clientHost.RegisterFunction<Vector3>("testmethod", (vec3) =>
        {
            lastVec3ReceivedByHost = vec3;
        });

        Vector3 lastVec3ReceivedByGuest = Vector3.Zero;

        client.RegisterFunction<Vector3>("testmethod", (vec3) =>
        {
            lastVec3ReceivedByGuest = vec3;
        });

        client.SendBrokerMsg("testmethod", XMLFormat.To(new Vector3(1, 2, 3)));

        yield return WaitUntilTrueOrTimeout(updateFunc, () => lastVec3ReceivedByHost != Vector3.Zero, NETWORK_TIMEOUT);

        Assert.Equal(lastVec3ReceivedByHost, Vector3.Zero);
        Assert.Equal(lastVec3ReceivedByGuest, Vector3.Zero);

        client.CoroutineManager.Update(10000);
        clientHost.CoroutineManager.Update(10000);

        Assert.Equal(client.CoroutineManager.GameTimeAdvanceLimit, clientHost.CoroutineManager.GameTimeAdvanceLimit);

        Assert.Equal(lastVec3ReceivedByHost, new Vector3(1, 2, 3));
        Assert.Equal(lastVec3ReceivedByGuest, new Vector3(1, 2, 3));

        ServerRoom roomServerSide = server.ActiveRooms[client.InRoom.Id - 1];
        Assert.NotNull(roomServerSide);

        server.Dispose();
        client.Dispose();
        clientHost.Dispose();
    }
}

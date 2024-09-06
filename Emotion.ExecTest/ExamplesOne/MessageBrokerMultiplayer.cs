#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml.Linq;
using Emotion.Common;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.Time.Routines;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ClientSide;
using Emotion.Network.ServerSide;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Standard.Reflector;
using Emotion.Standard.XML;
using Emotion.Testing;
using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using WinApi.User32;

#endregion

namespace Emotion.ExecTest;
public class MessageBrokerMultiplayer_TestObject : MapObject
{
    public int PlayerId;

    public Color Color;
    public bool PlayerControlled;

    private Vector2 _inputDirection;

    public MessageBrokerMultiplayer_TestObject()
    {
        Size = new Vector2(20);
    }

    public void AttachInput()
    {
        Engine.Host.OnKey.AddListener(KeyInput, KeyListenerType.Game);
        PlayerControlled = true;
    }

    protected bool KeyInput(Key key, Platform.Input.KeyState status)
    {
        Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
        if (keyAxisPart != Vector2.Zero)
        {
            if (status == Platform.Input.KeyState.Down)
                _inputDirection += keyAxisPart;
            else if (status == Platform.Input.KeyState.Up)
                _inputDirection -= keyAxisPart;

            return false;
        }

        return true;
    }

    public override void Update(float dt)
    {
        if (PlayerControlled)
            Position2 += _inputDirection * 0.1f * dt;
    }

    public override void Render(RenderComposer c)
    {
        c.RenderSprite(Position, Size, Color);
    }
}

public class MessageBrokerMultiplayer_TestScene : SceneWithMap
{
    private NetworkCommunicator _networkCom = null;
    private MsgBrokerClient _clientCom = null;
    private MessageBrokerMultiplayer_TestObject _myObj = null;
    private List<MessageBrokerMultiplayer_TestObject> _objects = new ();

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        // Fixes deadlock
        Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");

        UIBaseWindow buttonList = new UIBaseWindow();
        buttonList.LayoutMode = LayoutMode.HorizontalList;
        UIParent.AddChild(buttonList);

        buttonList.AddChild(new EditorButton("Host")
        {
            OnClickedProxy = (_) =>
            {
                _networkCom = Server.CreateServer<MsgBrokerServer>(1337);
                _clientCom = Client.CreateClient<MsgBrokerClient>("127.0.0.1:1337");
                _clientCom.ConnectIfNotConnected();
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestHostRoom();
                _clientCom.OnRoomJoined = OnRoomJoined;
                _clientCom.OnPlayerJoinedRoom = OnPlayerJoinedRoom;
                RegisterFuncs();

                Engine.CoroutineManagerAsync.StartCoroutine(UpdateNetworkAsyncRoutine());
                Engine.CoroutineManager.StartCoroutine(UpdateObjectNetwork());

                buttonList.ClearChildren();
            }
        });
        buttonList.AddChild(new EditorButton("Join")
        {
            OnClickedProxy = (_) =>
            {
                string serverIp = File.ReadAllText("ip.txt");
                _clientCom = Client.CreateClient<MsgBrokerClient>(serverIp);
                _networkCom = _clientCom;
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestRoomList();
                _clientCom.OnRoomListReceived = (list) => _clientCom.RequestJoinRoom(list[0].Id);
                _clientCom.OnRoomJoined = OnRoomJoined;
                _clientCom.OnPlayerJoinedRoom = OnPlayerJoinedRoom;
                _clientCom.ConnectIfNotConnected();
                RegisterFuncs();

                Engine.CoroutineManagerAsync.StartCoroutine(UpdateNetworkAsyncRoutine());
                Engine.CoroutineManager.StartCoroutine(UpdateObjectNetwork());

                buttonList.ClearChildren();
            }
        });

        Map = new GameMap();
        //Map.AddAndInitObject(new MapObjectSprite()
        //{
        //    EntityFile = "Test/Character.em2"
        //});

        

        //throw new System.Exception("haa");
        yield break;
    }

    private void RegisterFuncs()
    {
        _clientCom.RegisterFunction<Vector3>("MoveObj", MoveObj);
    }

    private void OnRoomJoined(ServerRoomInfo info)
    {
        _objects = new List<MessageBrokerMultiplayer_TestObject>();
        for (int i = 0; i < info.UsersInside.Length; i++)
        {
            var userId = info.UsersInside[i];

            var playerObject = new MessageBrokerMultiplayer_TestObject();
            playerObject.PlayerId = userId;
            playerObject.Color = Color.PrettyPurple;
            _objects.Add(playerObject);
            Map.AddAndInitObject(playerObject);
        }

        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            if (obj.PlayerId == _clientCom.UserId)
            {
                _myObj = obj;
                _myObj.Color = Color.PrettyYellow;
                _myObj.AttachInput();
            }
        }
    }

    private void OnPlayerJoinedRoom(ServerRoomInfo info)
    {
        for (int i = 0; i < info.UsersInside.Length; i++)
        {
            var userId = info.UsersInside[i];

            bool foundObject = false;
            for (int ii = 0; ii < _objects.Count; ii++)
            {
                var obj = _objects[ii];
                if (obj.PlayerId == userId)
                {
                    foundObject = true;
                    break;
                }
            }

            if (!foundObject)
            {
                var playerObject = new MessageBrokerMultiplayer_TestObject();
                playerObject.PlayerId = userId;
                playerObject.Color = Color.PrettyPurple;
                _objects.Add(playerObject);
                Map.AddAndInitObject(playerObject);
            }
        }
    }

    public override void UpdateScene(float dt)
    {
        base.UpdateScene(dt);
    }

    public override void RenderScene(RenderComposer c)
    {
        c.SetUseViewMatrix(false);
        c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.PrettyGreen);
        c.ClearDepth();
        c.SetUseViewMatrix(true);

        c.RenderCircle(Vector3.Zero, 20, Color.White, true);
        base.RenderScene(c);
    }

    public IEnumerator UpdateNetworkAsyncRoutine()
    {
        while (true)
        {
            if (_networkCom != null)
            {
                _networkCom.Update();
                _networkCom.PumpMessages();
            }

            if (_clientCom != null && _networkCom != _clientCom)
            {
                _clientCom.Update();
                _clientCom.PumpMessages();
            }

            yield return null;
        }
    }

    public IEnumerator UpdateObjectNetwork()
    {
        while (true)
        {
            if (_clientCom != null && _myObj != null)
            {
                _clientCom.SendBrokerMsg("MoveObj", XMLFormat.To(new Vector3(_myObj.Position2, _clientCom.UserId)));
            }
            yield return null;
        }
    }

    public void MoveObj(Vector3 pos)
    {
        int senderIdx = (int) pos.Z;
        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            if (obj.PlayerId == senderIdx)
            {
                obj.Position2 = Vector2.Lerp(obj.Position2, pos.ToVec2(), 0.5f);
                break;
            }
        }
    }
}
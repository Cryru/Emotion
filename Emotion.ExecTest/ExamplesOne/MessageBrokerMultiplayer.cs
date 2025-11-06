#region Using

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Emotion.Core;
using Emotion.Core.Systems.Input;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Scenography;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;
using Emotion.Graphics;
using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ClientSide;
using Emotion.Network.ServerSide;
using Emotion.Primitives;
using Emotion.Standard.Extensions;
using Emotion.Game.World;
using Emotion.Game.World.Components;
using Emotion.Game.Systems.UI2;

#endregion

namespace Emotion.ExecTest;

public class MessageBrokerMultiplayer_TestObject : GameObject
{
    public int PlayerId;

    public Color Color
    {
        get => GetComponent<SolidColorComponent>().Color;
        set => GetComponent<SolidColorComponent>().Color = value;
    }
    public bool PlayerControlled;

    private Vector2 _inputDirection;

    public MessageBrokerMultiplayer_TestObject()
    {
        Scale2D = new Vector2(20);
        AddComponent(new SolidColorComponent(Color.White));
    }

    public void AttachInput()
    {
        Engine.Input.OnKey.AddListener(KeyInput, KeyListenerType.Game);
        PlayerControlled = true;
    }

    protected bool KeyInput(Key key, KeyState status)
    {
        Vector2 keyAxisPart = Engine.Host.GetKeyAxisPart(key, Key.AxisWASD);
        if (keyAxisPart != Vector2.Zero)
        {
            if (status == KeyState.Down)
                _inputDirection += keyAxisPart;
            else if (status == KeyState.Up)
                _inputDirection -= keyAxisPart;

            return false;
        }

        return true;
    }

    public override void Update(float dt)
    {
        if (PlayerControlled)
            Position2D += _inputDirection * 0.1f * dt;
    }
}

public class MessageBrokerMultiplayer_TestScene : SceneWithMap
{
    private NetworkCommunicator _networkCom = null;
    private Client _clientCom = null;
    private MessageBrokerMultiplayer_TestObject _myObj = null;
    private List<MessageBrokerMultiplayer_TestObject> _objects = new ();

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        // Fixes deadlock
        Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");

        UIBaseWindow buttonList = new UIBaseWindow();
        buttonList.Layout.LayoutMethod = UILayoutMethod.HorizontalList(0);
        SceneUI.AddChild(buttonList);

        buttonList.AddChild(new EditorButton("Host")
        {
            OnClickedProxy = (_) =>
            {
                _networkCom = Server.CreateServer<MsgBrokerServer>(1337);
                _clientCom = Client.CreateClient<Client>("127.0.0.1:1337");
                _clientCom.ConnectIfNotConnected();
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestHostRoom();
                _clientCom.OnRoomJoined = OnRoomJoined;
                _clientCom.OnPlayerJoinedRoom = OnPlayerJoinedRoom;
                RegisterFuncs();

                Engine.CoroutineManager.StartCoroutine(UpdateObjectNetwork());

                buttonList.ClearChildren();
            }
        });
        buttonList.AddChild(new EditorButton("Join")
        {
            OnClickedProxy = (_) =>
            {
                string serverIp = File.ReadAllText("ip.txt");
                _clientCom = Client.CreateClient<Client>(serverIp);
                _networkCom = _clientCom;
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestRoomList();
                _clientCom.OnRoomListReceived = (list) => _clientCom.RequestJoinRoom(list[0].Id);
                _clientCom.OnRoomJoined = OnRoomJoined;
                _clientCom.OnPlayerJoinedRoom = OnPlayerJoinedRoom;
                _clientCom.ConnectIfNotConnected();
                RegisterFuncs();

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

    private void OnPlayerJoinedRoom(ServerRoomInfo info, int newUserId)
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
        if (_clientCom != null && _networkCom != _clientCom)
            _clientCom.Update();
        if (_networkCom != null)
            _networkCom.Update();
    }

    public override void RenderScene(Renderer c)
    {
        c.SetUseViewMatrix(false);
        c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.PrettyGreen);
        c.ClearDepth();
        c.SetUseViewMatrix(true);

        c.RenderCircle(Vector3.Zero, 20, Color.White, true);
        base.RenderScene(c);
    }

    public IEnumerator UpdateObjectNetwork()
    {
        while (true)
        {
            if (_clientCom != null && _myObj != null)
            {
                _clientCom.SendMessageToServer("MoveObj", new Vector3(_myObj.Position2D, _clientCom.UserId));
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
                obj.Position2D = Vector2.Lerp(obj.Position2D, pos.ToVec2(), 0.5f);
                break;
            }
        }
    }
}
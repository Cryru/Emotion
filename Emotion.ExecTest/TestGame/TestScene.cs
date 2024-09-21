using Emotion.Editor.EditorHelpers;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Network.ServerSide;
using Emotion.Network.TimeSyncMessageBroker;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.TimeUpdate;
using System;
using System.Collections;
using System.IO;
using System.Numerics;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class TestScene : SceneWithMap
{
    public GamePlayer LocalPlayer = new GamePlayer();

    private List<NetworkCharacter> _networkCharacters = new();
    private Character? _lastMouseoverChar;



    private void SetupTestLevel()
    {
        {
            var badGuy = new EnemyCharacter() { Position2 = new Vector2(0, -100) };
            Map.AddObject(badGuy);
        }
        {
            var badGuy = new EnemyCharacter() { Position2 = new Vector2(50, -200) };
            Map.AddObject(badGuy);
        }
        {
            var badGuy = new EnemyCharacter() { Position2 = new Vector2(-50, -200) };
            Map.AddObject(badGuy);
        }
        {
            var badGuy = new EnemyCharacter() { Position2 = new Vector2(0, -200) };
            Map.AddObject(badGuy);
        }
    }

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        Engine.Host.OnKey.AddListener(KeyInput, KeyListenerType.Game);
        Engine.Renderer.Camera = new Camera2D(Vector3.Zero, 1, KeyListenerType.None);

        SetupOnlineButtons();

        SpellButton spell = new SpellButton();
        spell.AnchorAndParentAnchor = UIAnchor.BottomLeft;
        UIParent.AddChild(spell);

        yield break;
    }

    public override void UpdateScene(float dt)
    {
        base.UpdateScene(dt);

        if (_networkCom != null)
            _networkCom.Update();

        if (_clientCom != null && _networkCom != _clientCom)
            _clientCom.Update();

        UpdateInputLocal(dt);
        if (LocalPlayer.Character != null && LocalPlayer.Character.IsDead())
            SpawnPlayerCharacter();

        if (LocalPlayer.Character != null)
        {
            if (_lastMouseoverChar != null)
            {
                _lastMouseoverChar.SetRenderMouseover(false);
            }

            Character? underMouse = GetCharacterUnderMouse();
            if (underMouse != null)
            {
                _lastMouseoverChar = underMouse;
                underMouse.SetRenderMouseover(true);
            }
        }
    }

    public override void RenderScene(RenderComposer c)
    {
        c.SetUseViewMatrix(false);
        c.RenderSprite(Vector3.Zero, c.CurrentTarget.Size, Color.PrettyGreen);
        c.ClearDepth();
        c.SetUseViewMatrix(true);

        //c.RenderCircle(Vector3.Zero, 20, Color.White, true);

        base.RenderScene(c);
    }

    #region Input

    protected Character? GetCharacterUnderMouse()
    {
        Vector2 mousePos = Engine.Host.MousePosition;
        Vector2 worldClick = Engine.Renderer.Camera.ScreenToWorld(mousePos).ToVec2();

        foreach (MapObject? obj in Map.ForEachObject())
        {
            Character? ch = obj as Character;
            if (ch == null) continue;
            if (obj is not EnemyCharacter) continue;

            Rectangle bounds = ch.Bounds;
            bounds.Center = ch.Position2;
            if (bounds.Contains(worldClick))
            {
                return ch;
            }
        }
        return null;
    }

    private Vector2 _inputDirection;

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

        if (_clientCom != null && LocalPlayer.Character != null)
        {
            if (key == Key.MouseKeyLeft && status == KeyState.Down)
            {
                var charUnderMouse = GetCharacterUnderMouse();
                if (charUnderMouse != null)
                {
                    LocalPlayer.Character.SetTarget(charUnderMouse);

                    UIBaseWindow? targetNameplate = UIParent.GetWindowById("TargetNameplate");
                    targetNameplate?.Close();

                    var nameplate = new Nameplate(charUnderMouse)
                    {
                        Offset = new Vector2(350, 0),
                        Margins = new Rectangle(5, 5, 5, 5),
                        Id = "TargetNameplate"
                    };
                    UIParent.AddChild(nameplate);
                }

                return false;
            }

            if (key == Key.Num4 && status == KeyState.Down)
            {
                //_clientCom.GameTimeRunner.StartCoroutine(UseThrash(LocalPlayer.Character));
                return false;
            }
        }

        return true;
    }

    private void UpdateInputLocal(float dt)
    {
        if (LocalPlayer.Character == null) return;
        LocalPlayer.Character.Position2 += _inputDirection * 0.1f * dt;
        Engine.Renderer.Camera.Position = LocalPlayer.Character.Position;
    }

    private IEnumerator UseThrash(Character user)
    {
        Vector3 pos = user.Position;
        foreach (MapObject? obj in Map.ForEachObject())
        {
            Character? ch = obj as Character;
            if (ch == null) continue;
            if (obj is not EnemyCharacter) continue;
            if (ch.IsDead()) continue;

            var dist = Vector3.Distance(obj.Position, user.Position);
            if (dist < 100)
                ch.TakeDamage(15);
        }
        yield return 15;
    }

    #endregion

    #region Systems

    public IEnumerator ServerTick()
    {
        AssertNotNull(_clientCom);
        while (true)
        {


            yield return 16;
        }
    }

    public IEnumerator UpdateMyCharacterPosition()
    {
        while (true)
        {
            AssertNotNull(_clientCom);

            Character? myCharacter = LocalPlayer.Character;
            if (myCharacter != null)
            {
                string? meta = XMLFormat.To(new Vector3(myCharacter.X, myCharacter.Y, _clientCom.UserId));
                if (meta != null)
                    _clientCom.SendBrokerMsg("UpdateObjectPosition", meta);
            }

            yield return 16;
        }
    }

    public IEnumerator UpdateCharactersSystem(CoroutineManager manager)
    {
        while (true)
        {
            AssertNotNull(_clientCom);

            foreach (MapObject? obj in Map.ForEachObject())
            {
                Character? ch = obj as Character;
                if (ch == null) continue;
                ch.UpdateCharacter(manager);
            }

            yield return 16;
        }
    }

    private void UpdateObjectPosition(Vector3 data)
    {
        int userId = (int)data.Z;
        for (int i = 0; i < _networkCharacters.Count; i++)
        {
            NetworkCharacter networkChar = _networkCharacters[i];
            if (networkChar.NetworkId == userId)
            {
                networkChar.X = data.X;
                networkChar.Y = data.Y;
                return;
            }
        }
    }

    private void CreateServerObject(ServerAuthorityCharacter ch)
    {
        Map.AddAndInitObject(ch);
    }

    #endregion

    private NetworkCommunicator? _networkCom = null;
    public MsgBrokerClientTimeSync? _clientCom = null;

    private void SetupOnlineButtons()
    {
        UIBaseWindow buttonList = new UIBaseWindow
        {
            LayoutMode = LayoutMode.VerticalList,
            AnchorAndParentAnchor = UIAnchor.BottomRight,
            ListSpacing = new Vector2(0, 5)
        };
        UIParent.AddChild(buttonList);

        buttonList.AddChild(new EditorButton("I am Server")
        {
            OnClickedProxy = (_) =>
            {
                _networkCom = Server.CreateServer<MsgBrokerServerTimeSync>(1337);
                _clientCom = Client.CreateClient<MsgBrokerClientTimeSync>("127.0.0.1:1337");
                _clientCom.ConnectIfNotConnected();
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestHostRoom();
                _clientCom.OnRoomJoined = OnRoomJoined;
                _clientCom.OnPlayerJoinedRoom = OnPlayerJoinedRoom;
                InitServerGame();

                buttonList.ClearChildren();
            }
        });
        buttonList.AddChild(new EditorButton("I am Player")
        {
            OnClickedProxy = (_) =>
            {
                string serverIp = File.ReadAllText("ip.txt");
                _clientCom = Client.CreateClient<MsgBrokerClientTimeSync>(serverIp);
                _networkCom = _clientCom;
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestRoomList();
                _clientCom.OnRoomListReceived = (list) => _clientCom.RequestJoinRoom(list[0].Id);
                _clientCom.OnRoomJoined = OnRoomJoined;
                _clientCom.OnPlayerJoinedRoom = OnPlayerJoinedRoom;
                _clientCom.ConnectIfNotConnected();
                InitNetworkGame();

                buttonList.ClearChildren();
            }
        });
    }

    private void OnRoomJoined(ServerRoomInfo info)
    {
        AssertNotNull(_clientCom);

        SpawnPlayerCharacter();
        for (int i = 0; i < info.UsersInside.Length; i++)
        {
            int id = info.UsersInside[i];
            if (id == _clientCom.UserId) continue;

            NetworkCharacter newChar = new(id);
            Map.AddAndInitObject(newChar);
            _networkCharacters.Add(newChar);
        }
    }

    private void OnPlayerJoinedRoom(ServerRoomInfo info, int newUserId)
    {
        NetworkCharacter newChar = new(newUserId);
        Map.AddAndInitObject(newChar);
        _networkCharacters.Add(newChar);
    }

    private void InitNetworkGame()
    {
        AssertNotNull(_clientCom);
        _clientCom.RegisterFunction<Vector3>("UpdateObjectPosition", UpdateObjectPosition);
        _clientCom.RegisterFunction<ServerAuthorityCharacter>("CreateServerObject", CreateServerObject);

        LocalPlayer = new GamePlayer();

        //_clientCom.GameTimeRunner.StartCoroutine(UpdateMyCharacterPosition());
        //_clientCom.GameTimeRunner.StartCoroutine(UpdateCharactersSystem(_clientCom.GameTimeRunner));
    }

    private void InitServerGame()
    {
        AssertNotNull(_clientCom);
        _clientCom.RegisterFunction<Vector3>("UpdateObjectPosition", UpdateObjectPosition);
        _clientCom.RegisterFunction<ServerAuthorityCharacter>("CreateServerObject", CreateServerObject);

        LocalPlayer = new GamePlayer();
        SetupTestLevel();

        //_clientCom.GameTimeRunner.StartCoroutine(ServerTick());
    }

    private void SpawnPlayerCharacter()
    {
        // Cleanup
        Character? localCharacter = LocalPlayer.Character;
        if (localCharacter != null)
        {
            
        }
        UIBaseWindow? targetNameplate = UIParent.GetWindowById("PlayerNameplate");
        targetNameplate?.Close();

        // Make new
        Character newChar = new MyCharacter();
        Map.AddAndInitObject(newChar);
        LocalPlayer.Character = newChar;

        var nameplate = new Nameplate(newChar)
        {
            Margins = new Rectangle(5, 5, 5, 5),
            Id = "PlayerNameplate"
        };
        UIParent.AddChild(nameplate);
    }

    public static void SendMsg<T>(string method, T data)
    {
        var currentScene = Engine.SceneManager.Current;
        if (currentScene is not TestScene ts) return;
        if (ts._clientCom == null) return;

        string? dataXML = XMLFormat.To(data);
        if (dataXML == null) return;
        ts._clientCom.SendBrokerMsg(method, dataXML);
    }
}

public class ServerAuthorityCharacter : Character
{
    public Guid Id { get; } = Guid.NewGuid();

    public ServerAuthorityCharacter()
    {

    }

    public override void Init()
    {
        base.Init();
        TestScene.SendMsg("CreateServerObject", this);
    }
}
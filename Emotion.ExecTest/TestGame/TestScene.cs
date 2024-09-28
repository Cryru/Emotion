using Emotion.Editor.EditorHelpers;
using Emotion.ExecTest.TestGame.Abilities;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class TestScene : SceneWithMap
{
    public static MsgBrokerClientTimeSync NetworkCom;

    public PlayerCharacter? MyCharacter;
    private Character? _lastMouseoverChar;

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
        UpdateInputLocal(dt);
        ClientTick();

        if (MyCharacter != null)
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

        _serverCom?.Update();
        _clientCom?.Update();

        base.UpdateScene(dt);
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

        if (_clientCom != null && MyCharacter != null)
        {
            if (key == Key.MouseKeyLeft && status == KeyState.Down)
            {
                var charUnderMouse = GetCharacterUnderMouse();
                if (charUnderMouse != null)
                {
                    MyCharacter.SetTarget(charUnderMouse);

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
        if (MyCharacter == null) return;
        if (_inputDirection == Vector2.Zero) return;
        if (_clientCom == null) return;

        MyCharacter.Position2 += _inputDirection * 0.1f * dt;
        Engine.Renderer.Camera.Position = MyCharacter.Position;
        MyCharacter.SendMovementUpdate();
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

    #region Network

    private void UpdateObjectPosition(MovementUpdate data)
    {
        // todo
        // Ignore updates about my character
        // broker except me?

        // todo
        // Better way of getting it
        foreach (MapObject? obj in Map.ForEachObject())
        {
            if (obj is Character ch && ch.ObjectId == data.ObjectId)
            {
                if (ch.LocallyControlled) break;

                ch.Position2 = data.Pos;
                break;
            }
        }
    }

    private void UseAbility(AbillityUsePacket packet)
    {
        var ability = packet.AbilityInstance;

        uint caster = packet.UserId;
        Character? casterUnit = null;

        foreach (MapObject? obj in Map.ForEachObject())
        {
            if (obj is Character ch && ch.ObjectId == caster)
            {
                casterUnit = ch;
                break;
            }
        }

        AssertNotNull(casterUnit);
        if (casterUnit.IsDead()) return;

        uint target = packet.TargetId;
        Character? targetUnit = null;

        if (target != 0)
        {
            foreach (MapObject? obj in Map.ForEachObject())
            {
                if (obj is Character ch && ch.ObjectId == target)
                {
                    targetUnit = ch;
                    break;
                }
            }
        }

        casterUnit.Network_UseAbility(ability, targetUnit);
    }

    #endregion

    private MsgBrokerServerTimeSync? _serverCom = null;
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
                _serverCom = Server.CreateServer<MsgBrokerServerTimeSync>(1337);
                _clientCom = Client.CreateClient<MsgBrokerClientTimeSync>("127.0.0.1:1337");
                _clientCom.ConnectIfNotConnected();
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestHostRoom();
                _clientCom.OnRoomJoined = OnServerRoomJoined;
                _clientCom.OnPlayerJoinedRoom = OnServerPlayerJoinedRoom;
                NetworkCom = _clientCom;
                buttonList.ClearChildren();

                buttonList.AddChild(new EditorButton("Launch Player")
                {
                    OnClickedProxy = (_) =>
                    {
                        string exePath = Process.GetCurrentProcess().MainModule.FileName;
                        Process.Start(exePath);
                    }
                });
            }
        });

        buttonList.AddChild(new EditorButton("I am Player")
        {
            OnClickedProxy = (_) =>
            {
                string serverIp = File.ReadAllText("ip.txt");
                _clientCom = Client.CreateClient<MsgBrokerClientTimeSync>(serverIp);
                _clientCom.OnConnectionChanged = (_) => _clientCom.RequestRoomList();
                _clientCom.OnRoomJoined = (_) => InitClientGame();
                _clientCom.OnRoomListReceived = (list) => _clientCom.RequestJoinRoom(list[0].Id);
                _clientCom.ConnectIfNotConnected();
                NetworkCom = _clientCom;

                buttonList.ClearChildren();
            }
        });
    }

    #region Server

    private void InitServerGame()
    {
        AssertNotNull(_clientCom);
        _clientCom.RegisterFunction<MovementUpdate>("UpdateObjectPosition", UpdateObjectPosition);
        _clientCom.RegisterFunction<AbillityUsePacket>("UseAbility", UseAbility);

        SetupTestLevel();
        Engine.CoroutineManager.StartCoroutine(ServerTick());
    }

    private IEnumerator ServerTick()
    {
        yield return null; // eager

        while (true)
        {
            if (_clientCom == null) break;

            foreach (MapObject? obj in Map.ForEachObject())
            {
                Character? ch = obj as Character; // todo: how bad is converting this?
                if (ch == null) continue;
                ch.ServerUpdate(Engine.DeltaTime, _clientCom);
            }

            yield return null;
        }
    }

    private void OnServerPlayerJoinedRoom(ServerRoomInfo info, int newUserId)
    {
        AssertNotNull(_clientCom);
        AssertNotNull(_serverCom);

        PlayerCharacter newChar = new((uint) newUserId);
        Map.AddAndInitObject(newChar);
        _clientCom.SendBrokerMsg("UnitSpawned", XMLFormat.To(newChar));

        // Sync current state
        List<Character> charsToSend = new List<Character>();
        foreach (var obj in Map.ForEachObject())
        {
            if (obj is Character ch)
                charsToSend.Add(ch);
        }

        string data = XMLFormat.To(charsToSend) ?? string.Empty;
        _clientCom.SendBrokerMsg("StartMap", data);
    }

    private void OnServerRoomJoined(ServerRoomInfo info)
    {
        AssertNotNull(_clientCom);
        InitServerGame();
    }
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

    #endregion

    #region Client

    // todo: server send this only to new user
    // todo: user id as handle rather than sequential index 
    private bool _mapStarted;

    private void InitClientGame()
    {
        AssertNotNull(_clientCom);
        _clientCom.RegisterFunction<MovementUpdate>("UpdateObjectPosition", UpdateObjectPosition);
        _clientCom.RegisterFunction<AbillityUsePacket>("UseAbility", UseAbility);

        _clientCom.RegisterFunction<Character>("UnitSpawned", (c) =>
        {
            if (!_mapStarted) return;
            Map.AddAndInitObject(c);
        });
        _clientCom.RegisterFunction<List<Character>>("StartMap", (characters) =>
        {
            if (_mapStarted) return;
            _mapStarted = true;

            for (int i = 0; i < characters.Count; i++)
            {
                var ch = characters[i];
                if (ch is PlayerCharacter pChar && pChar.PlayerId == _clientCom.UserId)
                {
                    var myChar = new MyCharacter(pChar.PlayerId);
                    Map.AddAndInitObject(myChar);
                    MyCharacter = myChar;
                    SetupUIForMyCharacter();
                }
                else
                {
                    Map.AddAndInitObject(ch);
                }
            }
        });
    }

    private void SetupUIForMyCharacter()
    {
        UIBaseWindow? targetNameplate = UIParent.GetWindowById("PlayerNameplate");
        targetNameplate?.Close();

        if (MyCharacter == null) return;

        var nameplate = new Nameplate(MyCharacter)
        {
            Margins = new Rectangle(5, 5, 5, 5),
            Id = "PlayerNameplate"
        };
        UIParent.AddChild(nameplate);
    }

    private void ClientTick()
    {

    }

    #endregion

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
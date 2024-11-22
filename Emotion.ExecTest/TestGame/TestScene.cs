using Emotion.Editor.EditorHelpers;
using Emotion.ExecTest.TestGame.Packets;
using Emotion.ExecTest.TestGame.UI;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.IO;
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

    public PlayerUnit? MyUnit;
    private Unit? _lastMouseoverUnit;

    protected List<FloatingText> _floatingTexts = new List<FloatingText>();

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        Engine.Host.OnKey.AddListener(KeyInput, KeyListenerType.Game);
        Engine.Renderer.Camera = new Camera2D(Vector3.Zero, 1, KeyListenerType.None);

        SetupOnlineButtons();

        yield break;
    }

    public override void UpdateScene(float dt)
    {
        for (int i = _floatingTexts.Count - 1; i >= 0; i--)
        {
            var text = _floatingTexts[i];
            text.Timer.Update(Engine.DeltaTime);
            if (text.Timer.Finished)
            {
                _floatingTexts.Remove(text);
            }
        }

        UpdateInputLocal(dt);
        ClientTick();

        if (MyUnit != null)
        {
            if (_lastMouseoverUnit != null)
            {
                _lastMouseoverUnit.SetRenderMouseover(false);
            }

            Unit? underMouse = GetCharacterUnderMouse();
            if (underMouse != null)
            {
                _lastMouseoverUnit = underMouse;
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

        for (int i = 0; i < _floatingTexts.Count; i++)
        {
            var textInstance = _floatingTexts[i];

            float y = 30 * textInstance.Timer.Progress;
            float opacity = 1f;
            if (textInstance.Timer.Progress > 0.5f)
            {
                opacity = 1.0f - ((textInstance.Timer.Progress - 0.5f) / 0.5f);
            }

            c.RenderString(textInstance.Position - new Vector3(0, y, 0), textInstance.Color * opacity, textInstance.Text,
                FontAsset.GetDefaultBuiltIn().GetAtlas(13), null, Emotion.Graphics.Text.FontEffect.Outline, 0.6f, Color.Black * opacity);
        }
    }

    #region Input

    protected Unit? GetCharacterUnderMouse()
    {
        Vector2 mousePos = Engine.Host.MousePosition;
        Vector2 worldClick = Engine.Renderer.Camera.ScreenToWorld(mousePos).ToVec2();

        foreach (MapObject? obj in Map.ForEachObject())
        {
            Unit? ch = obj as Unit;
            if (ch == null) continue;
            if (obj is not EnemyUnit) continue;

            Rectangle bounds = ch.Bounds2D;
            bounds.Center = ch.Position2D;
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

        if (_clientCom != null && MyUnit != null)
        {
            if (key == Key.MouseKeyLeft && status == KeyState.Down)
            {
                var charUnderMouse = GetCharacterUnderMouse();
                if (charUnderMouse != null)
                {
                    MyUnit.SetTarget(charUnderMouse);
                }

                return false;
            }
        }

        return true;
    }

    private void UpdateInputLocal(float dt)
    {
        if (MyUnit == null) return;
        if (_inputDirection == Vector2.Zero) return;
        if (_clientCom == null) return;

        MyUnit.Position2D += _inputDirection * 0.1f * dt;
        Engine.Renderer.Camera.Position = MyUnit.Position;
        MyUnit.SendMovementUpdate();
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
            if (obj is Unit ch && ch.ObjectId == data.ObjectId)
            {
                ch.MovedEvent();

                if (ch.LocallyControlled) break;
                ch.Position2D = data.Pos;
                break;
            }
        }
    }

    private void UseAbility(AbillityUsePacket packet)
    {
        uint caster = packet.UserId;
        Unit? casterUnit = null;

        foreach (MapObject? obj in Map.ForEachObject())
        {
            if (obj is Unit ch && ch.ObjectId == caster)
            {
                casterUnit = ch;
                break;
            }
        }

        AssertNotNull(casterUnit);
        if (casterUnit.IsDead()) return;

        uint target = packet.TargetId;
        Unit? targetUnit = null;

        if (target != 0)
        {
            foreach (MapObject? obj in Map.ForEachObject())
            {
                if (obj is Unit ch && ch.ObjectId == target)
                {
                    targetUnit = ch;
                    break;
                }
            }
        }

        string abilityId = packet.AbilityId;
        Abilities.Ability? ability = casterUnit.GetMyAbilityById(abilityId);
        if (ability == null) return;

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
                Unit? ch = obj as Unit; // todo: how bad is converting this?
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

        PlayerUnit newChar = new((uint) newUserId);
        Map.AddAndInitObject(newChar);
        _clientCom.SendBrokerMsg("UnitSpawned", XMLFormat.To(newChar));

        // Sync current state
        List<Unit> charsToSend = new List<Unit>();
        foreach (var obj in Map.ForEachObject())
        {
            if (obj is Unit ch)
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
            var badGuy = new EnemyUnit() { Position2D = new Vector2(0, -100) };
            Map.AddObject(badGuy);
        }
        {
            var badGuy = new EnemyUnit() { Position2D = new Vector2(50, -200) };
            Map.AddObject(badGuy);
        }
        {
            var badGuy = new EnemyUnit() { Position2D = new Vector2(-50, -200) };
            Map.AddObject(badGuy);
        }
        {
            var badGuy = new EnemyUnit() { Position2D = new Vector2(0, -200) };
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

        _clientCom.RegisterFunction<Unit>("UnitSpawned", (c) =>
        {
            if (!_mapStarted) return;
            Map.AddAndInitObject(c);
        });
        _clientCom.RegisterFunction<List<Unit>>("StartMap", (characters) =>
        {
            if (_mapStarted) return;
            _mapStarted = true;

            for (int i = 0; i < characters.Count; i++)
            {
                var ch = characters[i];
                if (ch is PlayerUnit pChar && pChar.PlayerId == _clientCom.UserId)
                {
                    var myChar = new MyUnit(pChar.PlayerId);
                    Map.AddAndInitObject(myChar);
                    MyUnit = myChar;
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

        if (MyUnit == null) return;

        var nameplate = new Nameplate(MyUnit)
        {
            Margins = new Rectangle(5, 5, 5, 5),
            Id = "PlayerNameplate"
        };
        UIParent.AddChild(nameplate);

        var skillBar = new UnitSkillBar(MyUnit);
        UIParent.AddChild(skillBar);
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

    public static void SendHash(string hash)
    {
        var currentScene = Engine.SceneManager.Current;
        if (currentScene is not TestScene ts) return;
        if (ts._clientCom == null) return;

        //ts._clientCom.SendTimeSyncHash(hash);
        ts._clientCom.SendTimeSyncHashDebug(hash);
    }

    public static void AddFloatingText(string text, Unit source, Unit target, Color? color)
    {
        TestScene? scene = Engine.SceneManager.Current as TestScene;
        if (scene == null) return;

        Vector2 midPoint = target.Position2D;
        if (source != target)
        {
            Vector2 dirTowardsSource = Vector2.Normalize(source.Position2D - target.Position2D);
            midPoint = midPoint + dirTowardsSource * target.Size2D / 2.3f;
        }

        scene._floatingTexts.Add(new FloatingText(text, midPoint.ToVec3(source.Z), color));
    }
}
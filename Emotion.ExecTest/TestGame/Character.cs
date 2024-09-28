using Emotion.Common.Serialization;
using Emotion.ExecTest.TestGame.Abilities;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.Network.TimeSyncMessageBroker;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.TimeUpdate;
using System;
using System.Linq;
using System.Reflection;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public enum CharacterState
{
    NotInCombat,
    InCombat,
    CombatAI_MoveToTargetOffset
}

public partial class Character : MapObject
{
    public const uint PLAYER_OBJECT_OFFSET = 2_000_000;

    public CharacterState State = CharacterState.NotInCombat;
    public int MeleeRange = 20;
    public int LastMeleeAttack = 0;
    public int MeleeSpeed = 500;

    public uint ObjectId;
    private static uint _nextObjectIdLocal = 1;

    [DontSerialize]
    public bool LocallyControlled;

    public string Image = string.Empty;
    private Texture _image = Texture.EmptyWhiteTexture;

    public string Name = "???";
    public int Health;
    public int MaxHealth = 100;

    public int MeleeDamage = 5;

    [DontSerialize]
    public Character? Target;

    public Vector2 VisualPosition;
    public Vector3 VisualPosition3 => VisualPosition.ToVec3();

    public Character()
    {
        Size = new Vector2(32);
    }

    protected Character(uint id) : this()
    {
        ObjectId = id;
    }

    public override void Init()
    {
        base.Init();
        Health = MaxHealth;
        VisualPosition = Position2;
        UpdateRenderMode();

        if (ObjectId == 0)
        {
            Engine.Log.Warning("Creating object id!", "Game");
            ObjectId = _nextObjectIdLocal;
            _nextObjectIdLocal++;
            LocallyControlled = true;
        }
    }

    public override void Update(float dt)
    {
        if (LocallyControlled)
            VisualPosition = Position2;
        else
            VisualPosition = Interpolation.SmoothLerp(VisualPosition, Position2, 10, Engine.DeltaTime / 1000f);
    }

    public virtual void ServerUpdate(float dt, MsgBrokerClientTimeSync clientCom)
    {

    }

    #region Combat

    public void TakeDamage(int damage)
    {
        if (IsDead()) return;
        Health -= damage;
        if (Health < 0) Health = 0;
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    public virtual void SetTarget(Character? ch)
    {
        Target = ch;
    }

    #endregion

    #region Melee Attacker Spread

    private List<Character>? _attackers = null;

    public void RegisterMeleeAttacker(Character ch)
    {
        if (_attackers == null)
            _attackers = new List<Character>();

        Assert(!_attackers.Contains(ch));
        _attackers.Add(ch);
    }

    public void UnregisterMeleeAttacker(Character ch)
    {
        if (_attackers == null) return;
        _attackers.Remove(ch);
    }

    public Vector2? MeleeAttackerGetNonOverlappingOffset(Character ch)
    {
        if (_attackers == null) return null;

        var overlapDist = ch.MeleeRange / 2f;

        float angleIncrement = 0.15f; // Amount to rotate in each step (radians)

        for (int i = 0; i < _attackers.Count; i++)
        {
            Character att = _attackers[i];
            if (att == ch) continue;
            if (att.State == CharacterState.CombatAI_MoveToTargetOffset) continue;

            var attPos = att.Position2;
            var chPos = ch.Position2;

            var dist = Vector2.Distance(attPos, chPos);
            if (dist < overlapDist)
            {
                Vector2 directionToAttacker = Vector2.Normalize(chPos - Position2);
                float currentAngle = (float)Math.Atan2(directionToAttacker.Y, directionToAttacker.X);
                currentAngle += angleIncrement;

                Vector2 offset = new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle)) * ch.MeleeRange;
                return offset;
            }
        }

        return null;
    }

    #endregion

    #region Movement

    public void SendMovementUpdate()
    {
        var movement = new MovementUpdate()
        {
            ObjectId = ObjectId,
            Pos = Position2
        };
        string? meta = XMLFormat.To(movement);
        TestScene.NetworkCom.SendBrokerMsg("UpdateObjectPosition", meta);
    }

    #endregion

    #region Abilities

    protected Ability? _usingAbility;
    protected Coroutine _usingAbilityRoutine;

    public void SendUseAbility(Ability ability, Character? target)
    {
        if (LocallyControlled)
            _usingAbility = ability;

        var packet = new AbillityUsePacket()
        {
            UserId = ObjectId,
            TargetId = target?.ObjectId ?? 0,
            AbilityInstance = ability
        };
        TestScene.NetworkCom.SendBrokerMsg("UseAbility", XMLFormat.To(packet));
    }

    public void Network_UseAbility(Ability ability, Character? target)
    {
        _usingAbility = ability; // GameTime routines are not eager
        _usingAbilityRoutine = Engine.CoroutineManagerGameTime.StartCoroutine(UseAbilityLocalRoutine(ability, target));
    }

    public IEnumerator UseAbilityLocalRoutine(Ability ability, Character? target)
    {
        yield return ability.OnUseAbility(this, target);
        _usingAbility = null;
    }

    protected bool CombatAI_BusyUsingAbility()
    {
        return _usingAbility != null;
    }

    #endregion

    #region Render

    private bool _mouseOver = false;
    private bool _targetMode = false;

    public void SetRenderMouseover(bool mouseover)
    {
        _mouseOver = mouseover;
        UpdateRenderMode();
    }

    public void SetRenderTargetMode(bool targetMode)
    {
        _targetMode = targetMode;
        UpdateRenderMode();
    }

    private void UpdateRenderMode()
    {
        if (_mouseOver)
        {
            var modeImage = Engine.AssetLoader.Get<TextureAsset>(Image + "_Target.png");
            if (modeImage != null)
            {
                _image = modeImage.Texture;
                return;
            }
        }

        if (_targetMode)
        {
            var modeImage = Engine.AssetLoader.Get<TextureAsset>(Image + "_Target_Current.png");
            if (modeImage != null)
            {
                _image = modeImage.Texture;
                return;
            }
        }

        var normalImage = Engine.AssetLoader.Get<TextureAsset>(Image + ".png");
        _image = normalImage?.Texture ?? Texture.EmptyWhiteTexture;
    }

    public override void Render(RenderComposer c)
    {
        base.Render(c);
        c.RenderSprite((VisualPosition - Size / 2f).ToVec3(), Size, Color.White, _image);

        // if (LocallyControlled) c.RenderSprite((VisualPosition - Size / 2f).ToVec3(), Size, Color.Red);
    }

    #endregion
}

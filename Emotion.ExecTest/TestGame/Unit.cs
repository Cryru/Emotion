using Emotion.Common.Serialization;
using Emotion.ExecTest.TestGame.Abilities;
using Emotion.ExecTest.TestGame.Packets;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.Network.TimeSyncMessageBroker;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Utility;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.TimeUpdate;
using System;
using System.Linq;
using System.Reflection;

#nullable enable

namespace Emotion.ExecTest.TestGame;

[Flags]
public enum CharacterState
{
    None = 0,
    InCombat = 2 << 0,
    CombatAI_MoveToTargetOffset = 2 << 1
}

public class AbilityMeta
{
    public int LastTimeCast;
    public int CooldownTimeStamp;
}

public partial class Unit : MapObject
{
    public const uint PLAYER_OBJECT_OFFSET = 2_000_000;

    public float CastProgress = 0;

    public int GlobalCooldownLastActivated = 0;
    public int GlobalCooldownTimestamp = 0;

    public List<Ability> Abilities = new List<Ability>();
    private Dictionary<string, AbilityMeta> _abilityMeta = new Dictionary<string, AbilityMeta>();

    public CharacterState State = CharacterState.None;

    public uint ObjectId;
    private static uint _nextObjectIdLocal = 1;

    [DontSerialize]
    public bool LocallyControlled;

    public string Image = string.Empty;
    private Texture _image = Texture.EmptyWhiteTexture;

    public string Name = "???";
    public int Health;
    public int MaxHealth = 100;

    public int MeleeRange = 20;
    public int MeleeDamage = 5;
    public int MeleeSpeed = 500;

    [DontSerialize]
    public Unit? Target;

    public Vector2 VisualPosition;
    public Vector3 VisualPosition3 => VisualPosition.ToVec3();

    public Unit()
    {
        Size = new Vector2(32);
    }

    protected Unit(uint id) : this()
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

    public void TakeDamage(int damage, Unit? source)
    {
        if (IsDead()) return;

        if (Target == null)
        {
            SetTarget(source);
        }

        if (!State.EnumHasFlag(CharacterState.InCombat))
        {
            State |= CharacterState.InCombat;
        }

        Health -= damage;
        if (Health < 0) Health = 0;
        TestScene.AddFloatingText($"-{damage}", this, source ?? this, Color.PrettyRed);
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    public virtual void SetTarget(Unit? ch)
    {
        Target = ch;
    }

    #endregion

    #region Melee Attacker Spread

    private List<Unit>? _attackers = null;

    public void RegisterMeleeAttacker(Unit ch)
    {
        if (_attackers == null)
            _attackers = new List<Unit>();

        Assert(!_attackers.Contains(ch));
        _attackers.Add(ch);
    }

    public void UnregisterMeleeAttacker(Unit ch)
    {
        if (_attackers == null) return;
        _attackers.Remove(ch);
    }

    public Vector2? MeleeAttackerGetNonOverlappingOffset(Unit ch)
    {
        if (_attackers == null) return null;

        var overlapDist = ch.MeleeRange / 2f;

        float angleIncrement = 0.15f; // Amount to rotate in each step (radians)

        for (int i = 0; i < _attackers.Count; i++)
        {
            Unit att = _attackers[i];
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

    public AbilityMeta? GetAbilityMeta(Ability ability)
    {
        string abilityId = ability.Id;
        if (_abilityMeta.TryGetValue(abilityId, out AbilityMeta? meta))
        {
            return meta;
        }

        bool hasAbility = false;
        for (int i = 0; i < Abilities.Count; i++)
        {
            var ab = Abilities[i];
            var abId = ab.Id;
            if (abId == abilityId)
            {
                hasAbility = true;
                break;
            }
        }
        
        if (hasAbility)
        {
            AbilityMeta newMeta = new AbilityMeta();
            _abilityMeta.Add(abilityId, newMeta);
            return newMeta;
        }

        return null;
    }

    public Ability? GetMyAbilityById(string abilityId)
    {
        for (int i = 0; i < Abilities.Count; i++)
        {
            var ab = Abilities[i];
            var abId = ab.Id;
            if (abId == abilityId)
            {
                return ab;
            }
        }
        return null;
    }

    public void SendUseAbility(Ability ability)
    {
        Unit? target = Target;

        if (LocallyControlled)
            _usingAbility = ability;

        string abilityId = ability.Id;
        var packet = new AbillityUsePacket()
        {
            UserId = ObjectId,
            TargetId = target?.ObjectId ?? 0,
            AbilityId = abilityId
        };
        TestScene.NetworkCom.SendBrokerMsg("UseAbility", XMLFormat.To(packet));
    }

    public void Network_UseAbility(Ability ability, Unit? target)
    {
        _usingAbility = ability; // GameTime routines are not eager
        _usingAbilityRoutine = Engine.CoroutineManagerGameTime.StartCoroutine(UseAbilityLocalRoutine(ability, target));
    }

    public IEnumerator UseAbilityLocalRoutine(Ability ability, Unit? target)
    {
        if (!ability.CanUse(this, target))
        {
            _usingAbility = null;
            yield break;
        }

        AbilityMeta? meta = GetAbilityMeta(ability);
        AssertNotNull(meta);
        if (meta != null)
        {
            int timeNow = (int)Engine.CurrentGameTime;
            if (ability.Flags.EnumHasFlag(AbilityFlags.StartsGlobalCooldown))
            {
                GlobalCooldownTimestamp = timeNow + 1000;
                GlobalCooldownLastActivated = timeNow;
            }
            
            meta.CooldownTimeStamp = timeNow + ability.GetCooldown(this, target);
            meta.LastTimeCast = timeNow;
        }

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

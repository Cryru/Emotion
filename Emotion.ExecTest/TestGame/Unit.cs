using Emotion.Common.Serialization;
using Emotion.ExecTest.TestGame.Abilities;
using Emotion.ExecTest.TestGame.Combat;
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
using System.Reflection.Metadata;

#nullable enable

namespace Emotion.ExecTest.TestGame;

[Flags]
public enum CharacterState
{
    None = 0,
    InCombat = 2 << 0,
    CombatAI_MoveToMeleeRangeSpot = 2 << 1
}

public class AbilityMeta
{
    public int LastTimeCast;
    public int CooldownTimeStamp;
}

public partial class Unit : MapObject
{
    public const uint PLAYER_OBJECT_OFFSET = 2_000_000;
    public const int AI_MOVE_RANGE_IMPRECISION = 1;

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
    public AssetHandle<TextureAsset> _image;

    public string Name = "???";
    public int Health;
    public int MaxHealth = 100;

    public int MeleeRange = 20;
    public int MeleeDamage = 5;
    public int MeleeSpeed = 500;

    public int LastMoved;
    public bool IsMoving => LastMoved != 0 && (Engine.CurrentGameTime - LastMoved < 50);

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

        if (HasAuraWithId(GodModeToggleAbility.AURA_ID) != null)
        {
            damage = 0;
        }

        Health -= damage;
        //Console.WriteLine($"{Health} (-{damage}) of {ObjectId}");
        TestScene.SendHash($"{Health} (-{damage}) of {ObjectId}");
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

    private const int _meleeSpotAngleDiff = 30;
    protected bool[] _freeMeleeRangeSpots = new bool[(360 / _meleeSpotAngleDiff)]; 

    public Vector2 GetFreeMeleeRangeSpot(Unit ch)
    {
        Vector2 chPos = ch.Position2;
        Vector2 directionToAttacker = Vector2.Normalize(chPos - Position2);
        float currentAngle = (float)Math.Atan2(directionToAttacker.Y, directionToAttacker.X);
        float currentAngleInDegrees = Maths.RadiansToDegrees(currentAngle);
        int closestAngleInDegrees = (int)(MathF.Round(currentAngleInDegrees / _meleeSpotAngleDiff) * _meleeSpotAngleDiff);
        closestAngleInDegrees = (int)Maths.ClampAngle(closestAngleInDegrees);

        int closestAngleIndex = closestAngleInDegrees / _meleeSpotAngleDiff;
        if (_freeMeleeRangeSpots[closestAngleIndex])
        {
            _freeMeleeRangeSpots[closestAngleIndex] = false;
            return GetMeleeRangeSpotOfIndex(closestAngleIndex, ch.MeleeRange - AI_MOVE_RANGE_IMPRECISION);
        }

        // Check to the left and right of the closest angle index
        for (int i = 1; i < _freeMeleeRangeSpots.Length; i++)
        {
            int leftIndex = (closestAngleIndex - i + _freeMeleeRangeSpots.Length) % _freeMeleeRangeSpots.Length; // Left index
            int rightIndex = (closestAngleIndex + i) % _freeMeleeRangeSpots.Length; // Right index

            // Check left
            if (_freeMeleeRangeSpots[leftIndex])
            {
                _freeMeleeRangeSpots[leftIndex] = false;
                return GetMeleeRangeSpotOfIndex(leftIndex, ch.MeleeRange - AI_MOVE_RANGE_IMPRECISION);
            }

            // Check right
            if (_freeMeleeRangeSpots[rightIndex])
            {
                _freeMeleeRangeSpots[rightIndex] = false;
                return GetMeleeRangeSpotOfIndex(rightIndex, ch.MeleeRange - AI_MOVE_RANGE_IMPRECISION);
            }
        }

        // No free spots, just take closest with some derivation
        float xDeriv = Helpers.GenerateRandomNumber(-100, 100) / 100f;
        float yDeriv = Helpers.GenerateRandomNumber(-100, 100) / 100f;
        return GetMeleeRangeSpotOfIndex(closestAngleIndex, ch.MeleeRange - AI_MOVE_RANGE_IMPRECISION) + new Vector2(xDeriv, yDeriv);
    }

    private Vector2 GetMeleeRangeSpotOfIndex(int index, float range)
    {
        int angleDeg = index * _meleeSpotAngleDiff;
        var angleRad = Maths.DegreesToRadians(angleDeg);
        Vector2 offset = new Vector2((float)Math.Cos(angleRad), (float)Math.Sin(angleRad)) * range;
        return Position2 + offset;
    }

    #endregion

    #region Movement

    public void MovedEvent()
    {
        LastMoved = (int)Engine.CurrentGameTime;

        for (int i = 0; i < _freeMeleeRangeSpots.Length; i++)
        {
            _freeMeleeRangeSpots[i] = true;
        }
    }

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
        if (ability.CanUse(this, target) != AbilityCanUseResult.CanUse)
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

    #region Aura

    private List<Aura> _auras = new List<Aura>();
    
    public IEnumerable<Aura> ForEachAura()
    {
        return _auras;
    }

    public bool ApplyAura(Aura aura)
    {
        _auras.Add(aura);
        aura.OnAttach(this);
        return true;
    }

    public bool RemoveAura(Aura aura)
    {
        bool success = _auras.Remove(aura);
        if (success)
        {
            aura.OnDetach(this);
            return true;
        }
        return false;
    }

    public Aura? HasAuraWithId(string id)
    {
        for (int i = 0; i < _auras.Count; i++)
        {
            var aura = _auras[i];
            if (aura.Id == id) return aura;
        }

        return null;
    }

    public void UpdateAuras()
    {
        for (int i = 0; i < _auras.Count; i++)
        {
            var aura = _auras[i];
            if (aura.IsFinished()) aura.OnDetach(this);
        }
        _auras.RemoveAll(x => x.IsFinished());
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

    private AssetHandle<TextureAsset> _mouseOverImage;
    private AssetHandle<TextureAsset> _targetCurrentImage;
    private AssetHandle<TextureAsset> _normalImage;

    public override void LoadAssets(AssetLoader assetLoader)
    {
        _normalImage = assetLoader.ONE_Get<TextureAsset>(Image + ".png");
        _mouseOverImage = assetLoader.ONE_Get<TextureAsset>(Image + "_Target.png");
        _targetCurrentImage = assetLoader.ONE_Get<TextureAsset>(Image + "_Target_Current.png");
    }

    private void UpdateRenderMode()
    {
        if (_mouseOver)
        {
            _image = _mouseOverImage.AssetExists ? _mouseOverImage : _normalImage;
            return;
        }

        if (_targetMode)
        {
            _image = _targetCurrentImage.AssetExists ? _targetCurrentImage : _normalImage;
            return;
        }

        _image = _normalImage;
    }

    public override void Render(RenderComposer c)
    {
        base.Render(c);
        c.RenderSprite((VisualPosition - Size / 2f).ToVec3(), Size, _image);

        if (this is PlayerUnit)
        {
            //Vector2 chPos = new Vector2(0f);
            //Vector2 directionToAttacker = Vector2.Normalize(chPos - Position2);
            //float currentAngle = (float)Math.Atan2(directionToAttacker.Y, directionToAttacker.X);
            //float currentAngleInDegrees = Maths.RadiansToDegrees(currentAngle);
            //int closestAngleInDegrees = (int)(MathF.Round(currentAngleInDegrees / _meleeSpotAngleDiff) * _meleeSpotAngleDiff);
            //closestAngleInDegrees = (int) Maths.ClampAngle(closestAngleInDegrees);

            //int closestAngleIndex = closestAngleInDegrees / _meleeSpotAngleDiff;

            //c.RenderLine(chPos.ToVec3(), Position2.ToVec3(), Color.Blue);

            int angle = 0;
            for (int i = 0; i < (360 / 30); i++)
            {
                var angleRad = Maths.DegreesToRadians(angle);
                Vector2 offset = new Vector2((float)Math.Cos(angleRad), (float)Math.Sin(angleRad)) * MeleeRange;

                bool free = _freeMeleeRangeSpots[i];

                c.RenderCircle((Position2 + offset).ToVec3(), 2, free ? Color.Blue : Color.Red, true);

                angle += 30;
            }
        }

        // if (LocallyControlled) c.RenderSprite((VisualPosition - Size / 2f).ToVec3(), Size, Color.Red);
    }

    #endregion
}

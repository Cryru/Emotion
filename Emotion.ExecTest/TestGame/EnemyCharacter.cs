using Emotion.ExecTest.TestGame.Abilities;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.Network.TimeSyncMessageBroker;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class EnemyCharacter : Character
{
    public int AggroRange = 50;


    private bool _showSword;
    private Texture _swordAsset = Texture.EmptyWhiteTexture;
    private Vector2 _moveDir;

    private bool _inMeleeRangeOfTarget;
    private Vector2 _spawnPos;
    private Vector2 _targetingOffset;

    private MeleeAttack _attackSkill = new();

    public EnemyCharacter()
    {
        Name = "Bad Guy";
        Image = "Test/proto/enemy";
        Size = new Vector2(16);
    }

    public override void Init()
    {
        base.Init();
        _spawnPos = Position2;

        _swordAsset = Engine.AssetLoader.Get<TextureAsset>("Test/proto/sword.png")?.Texture ?? Texture.EmptyWhiteTexture;
        _swordAsset.Smooth = true;
    }

    public override void Update(float dt)
    {
        base.Update(dt);
    }

    public override void ServerUpdate(float dt, MsgBrokerClientTimeSync clientCom)
    {
        AssertNotNull(Map);

        if (IsDead())
            return;

        if (_moveDir != Vector2.Zero)
        {
            Position2 += _moveDir * 0.1f * dt;
            SendMovementUpdate();
        }

        if (State == CharacterState.NotInCombat)
        {
            foreach (var obj in Map.ForEachObject())
            {
                if (obj == this) continue;
                if (obj is EnemyCharacter) continue;

                Character? ch = obj as Character;
                if (ch == null) continue;
                if (ch.IsDead()) continue;

                var objPos = ch.Position;
                if (Vector3.Distance(objPos, Position) < AggroRange)
                {
                    Target = ch;
                    State = CharacterState.InCombat;
                    break;
                }
            }
        }
        if (State == CharacterState.InCombat || State == CharacterState.CombatAI_MoveToTargetOffset)
        {
            if (CombatAI_BusyUsingAbility()) return;
            _showSword = false;

            if (Target == null || Target.IsDead())
            {
                State = CharacterState.NotInCombat;
                return;
            }

            if (Vector2.Distance(Target.Position2, Position2) > MeleeRange)
            {
                _moveDir = Vector2.Normalize(Target.Position2 - Position2);

                if (_inMeleeRangeOfTarget)
                {
                    _targetingOffset = Vector2.Zero;
                    Target.UnregisterMeleeAttacker(this);
                    _inMeleeRangeOfTarget = false;
                }
            }
            else if (State == CharacterState.CombatAI_MoveToTargetOffset && _targetingOffset != Vector2.Zero && Vector2.Distance(Position2, _targetingOffset) > 1f)
            {
                _moveDir = Vector2.Normalize(_targetingOffset - Position2);
            }
            else
            {
                if (!_inMeleeRangeOfTarget)
                {
                    _inMeleeRangeOfTarget = true;
                    Target.RegisterMeleeAttacker(this);
                    _moveDir = Vector2.Zero;
                }
               
                if (State == CharacterState.CombatAI_MoveToTargetOffset)
                {
                    State = CharacterState.InCombat;
                    _targetingOffset = Vector2.Zero;
                    _moveDir = Vector2.Zero;
                }


                Vector2? offset = Target.MeleeAttackerGetNonOverlappingOffset(this);
                if (offset != null)
                {
                    _targetingOffset = Target.Position2 + offset.Value;
                    State = CharacterState.CombatAI_MoveToTargetOffset;
                }
                else
                {
                    SendUseAbility(_attackSkill, Target);
                    _showSword = true;
                }
            }
        }
    }

    public override void Render(RenderComposer c)
    {
        base.Render(c);

        Vector3 pos = VisualPosition3;

        float healthPercent = (float) Health / MaxHealth;
        var bar = new Rectangle(pos.X - 20, pos.Y - 15, 40 * healthPercent, 5);
        c.RenderSprite(bar, Color.PrettyRed * 0.5f);

        c.RenderCircleOutline(pos, AggroRange, Color.Red, true);

        if (_showSword)
            c.RenderSprite(pos - new Vector3(8, 8, 0), new Vector2(16), _swordAsset);
    }
}

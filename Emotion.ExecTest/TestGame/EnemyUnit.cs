using Emotion.ExecTest.TestGame.Abilities;
using Emotion.IO;
using Emotion.Network.TimeSyncMessageBroker;
using Emotion.Utility;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class EnemyUnit : Unit
{
    public int AggroRange = 50;

    private Vector2 _moveDir;

    private bool _inMeleeRangeOfTarget;
    private Vector2 _spawnPos;
    private Vector2 _targetingOffset;

    private MeleeAttack _attackSkill = new();

    public EnemyUnit()
    {
        Name = "Bad Guy";
        Image = "Test/proto/enemy";
        Size = new Vector2(16);

        Abilities.Add(new MeleeAttack());
    }

    public override void Init()
    {
        base.Init();
        _spawnPos = Position2;
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

        if (CombatAI_BusyUsingAbility())
            return;

        if (_moveDir != Vector2.Zero)
        {
            Position2 += _moveDir * 0.1f * dt;
            SendMovementUpdate();
        }

        if (!State.EnumHasFlag(CharacterState.InCombat))
        {
            foreach (var obj in Map.ForEachObject())
            {
                if (obj == this) continue;
                if (obj is EnemyUnit) continue;

                Unit? ch = obj as Unit;
                if (ch == null) continue;
                if (ch.IsDead()) continue;

                var objPos = ch.Position;
                if (Vector3.Distance(objPos, Position) < AggroRange)
                {
                    Target = ch;
                    State |= CharacterState.InCombat;
                    break;
                }
            }
        }
        if (State.EnumHasFlag(CharacterState.InCombat) || State.EnumHasFlag(CharacterState.CombatAI_MoveToTargetOffset))
        {
            if (Target == null || Target.IsDead())
            {
                State = State.EnumRemoveFlag(CharacterState.InCombat);
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
            else if (State.EnumHasFlag(CharacterState.CombatAI_MoveToTargetOffset) && _targetingOffset != Vector2.Zero && Vector2.Distance(Position2, _targetingOffset) > 1f)
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
               
                if (State.EnumHasFlag(CharacterState.CombatAI_MoveToTargetOffset))
                {
                    State = State.EnumRemoveFlag(CharacterState.CombatAI_MoveToTargetOffset);
                    _targetingOffset = Vector2.Zero;
                    _moveDir = Vector2.Zero;
                }


                Vector2? offset = Target.MeleeAttackerGetNonOverlappingOffset(this);
                if (offset != null)
                {
                    _targetingOffset = Target.Position2 + offset.Value;
                    State |= CharacterState.CombatAI_MoveToTargetOffset;
                }
                else if(_attackSkill.CanUse(this, Target))
                {
                    SendUseAbility(_attackSkill);
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
    }
}

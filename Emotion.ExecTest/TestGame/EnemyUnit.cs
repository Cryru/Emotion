using Emotion.ExecTest.TestGame.Abilities;
using Emotion.IO;
using Emotion.Network.TimeSyncMessageBroker;
using Emotion.Utility;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class EnemyUnit : Unit
{
    public int AggroRange = 50;

    private Vector2 _spawnPos;
    private Vector2 _aiMoveTo;
    private Vector2 _aiTargetLastPosition;

    private MeleeAttack _attackSkill;

    public EnemyUnit()
    {
        Name = "Bad Guy";
        Image = "Test/proto/enemy";
        Size = new Vector2(16);

        _attackSkill = new MeleeAttack();
        Abilities.Add(_attackSkill);
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

    private void AI_MoveTo(Vector2 point)
    {
        Assert(point != Vector2.Zero);
        _aiMoveTo = point;
    }

    private void AI_StopMoving()
    {
        _aiMoveTo = Vector2.Zero;
    }

    private void AI_ProcessMovement(float dt)
    {
        if (_aiMoveTo == Vector2.Zero) return;

        Vector2 diff = _aiMoveTo - Position2;
        float length = diff.Length();
        if (length < AI_MOVE_RANGE_IMPRECISION)
        {
            AI_StopMoving();
            return;
        }

        Vector2 moveDir = Vector2.Normalize(diff);
        if (moveDir != Vector2.Zero)
        {
            Position2 += moveDir * 0.1f * dt;
            SendMovementUpdate();
        }
    }

    public override void ServerUpdate(float dt, MsgBrokerClientTimeSync clientCom)
    {
        AssertNotNull(Map);

        if (IsDead())
            return;

        if (CombatAI_BusyUsingAbility())
            return;

        AI_ProcessMovement(dt);

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

        if (State.EnumHasFlag(CharacterState.InCombat))
        {
            if (Target == null || Target.IsDead())
            {
                AI_StopMoving();
                State = State.EnumRemoveFlag(CharacterState.InCombat);
                State = State.EnumRemoveFlag(CharacterState.CombatAI_MoveToMeleeRangeSpot);
                return;
            }

            AbilityCanUseResult canUse = _attackSkill.CanUse(this, Target);
            if (canUse == AbilityCanUseResult.CanUse)
            {
                SendUseAbility(_attackSkill);
                return;
            }
            else if (canUse == AbilityCanUseResult.OutOfRange)
            {
                if (_aiTargetLastPosition != Target.Position2)
                {
                    _aiTargetLastPosition = Target.Position2;

                    Vector2 pointInMelee = Target.GetFreeMeleeRangeSpot(this);
                    AI_MoveTo(pointInMelee);
                }
                return;
            }
        }
    }

    public override void Render(RenderComposer c)
    {
        base.Render(c);

        Vector2 pos = VisualPosition;

        float healthPercent = (float)Health / MaxHealth;
        var bar = new Rectangle(pos.X - 20, pos.Y - 15, 40 * healthPercent, 5);
        var barBg = new Rectangle(pos.X - 21, pos.Y - 16, 42, 7);
        c.RenderSprite(barBg, Color.Black * 0.75f);
        c.RenderSprite(bar, Color.PrettyRed);

        //c.RenderCircleOutline(pos, AggroRange, Color.Red, true);
    }
}

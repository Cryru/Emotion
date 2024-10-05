#nullable enable

namespace Emotion.ExecTest.TestGame.Abilities;

public class MeleeAttack : Ability
{
    public MeleeAttack()
    {
        Icon = "Test/proto/abilities/melee_attack.png";
        Flags = AbilityFlags.RequireTarget;
    }

    // todo: return error such as out of range
    public override bool CanUse(Unit caster, Unit? target)
    {
        if (target == null) return false;

        // todo: get range + flag
        float dist = Vector2.Distance(caster.Position2, target.Position2);
        if (dist > caster.MeleeRange) return false;

        return base.CanUse(caster, target);
    }

    public override int GetCooldown(Unit caster, Unit? target)
    {
        return caster.MeleeSpeed;
    }

    public override IEnumerator OnUseAbility(Unit caster, Unit? target)
    {
        target!.TakeDamage(caster.MeleeDamage, caster);
        yield break;
    }
}

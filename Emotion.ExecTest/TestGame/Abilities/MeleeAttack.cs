#nullable enable

namespace Emotion.ExecTest.TestGame.Abilities;

public class MeleeAttack : Ability
{
    public MeleeAttack()
    {
        Icon = "Test/proto/abilities/melee_attack.png";
        Flags = AbilityFlags.RequireTarget;
    }

    public override AbilityCanUseResult CanUse(Unit caster, Unit? target)
    {
        if (target == null) return AbilityCanUseResult.NoTarget;

        float dist = Vector2.Distance(caster.Position2D, target.Position2D);
        int casterMeleeRange = caster.MeleeRange;
        //if (target.IsMoving) casterMeleeRange += casterMeleeRange / 2;
        if (dist > casterMeleeRange) return AbilityCanUseResult.OutOfRange;

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

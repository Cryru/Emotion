using Emotion.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.ExecTest.TestGame.Abilities;

public class MeleeAttack : Ability
{
    public override IEnumerator OnUseAbility(Character caster, Character target)
    {
        float time = Engine.CurrentGameTime;
        int lastMeleeAttack = caster.LastMeleeAttack;
        int nextAttackPossibleAt = lastMeleeAttack + caster.MeleeSpeed;
        if (nextAttackPossibleAt > time)
        {
            yield return nextAttackPossibleAt - time;
        }

        if (Vector2.Distance(target.Position2, caster.Position2) > caster.MeleeRange) yield break;
        target.TakeDamage(caster.MeleeDamage);
        caster.LastMeleeAttack = (int) Engine.CurrentGameTime;
    }
}

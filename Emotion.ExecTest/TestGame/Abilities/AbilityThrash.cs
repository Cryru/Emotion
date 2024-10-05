using Emotion.WIPUpdates.One.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.ExecTest.TestGame.Abilities;

public class AbilityThrash : Ability
{
    public int DamageCoeff = 4;

    public AbilityThrash()
    {
        Icon = "Test/proto/abilities/thrash.png";
        Flags = AbilityFlags.StartsGlobalCooldown | AbilityFlags.CantBeUsedOnGlobalCooldown;
    }

    public override int GetCooldown(Unit caster, Unit target)
    {
        return 4000;
    }

    public override IEnumerator OnUseAbility(Unit caster, Unit target)
    {
        Vector3 pos = caster.Position;
        foreach (MapObject obj in caster.Map.ForEachObject())
        {
            Unit? ch = obj as Unit;
            if (ch == null) continue;
            if (obj is not EnemyUnit) continue;
            if (ch.IsDead()) continue;

            var dist = Vector3.Distance(obj.Position, caster.Position);
            if (dist < 100)
                ch.TakeDamage(caster.MeleeDamage * DamageCoeff, caster);
        }
        yield break;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.ExecTest.TestGame.Abilities;

public class Ability
{
    public virtual IEnumerator OnUseAbility(Character caster, Character target)
    {
        yield break;
    }
}

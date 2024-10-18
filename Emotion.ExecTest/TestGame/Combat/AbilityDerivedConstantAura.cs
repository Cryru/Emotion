using Emotion.ExecTest.TestGame.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.ExecTest.TestGame.Combat;

public class AbilityDerivedConstantAura : Aura
{
    public override string Id => _auraId;

    public override string Icon => _ability.Icon;

    private Ability _ability;
    private string _auraId;

    public AbilityDerivedConstantAura(Ability ability, string? idOverride = null)
    {
        Type = AuraType.Constant;
        _ability = ability;
        _auraId = idOverride ?? ability.Id;
    }
}

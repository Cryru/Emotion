#nullable enable
using Emotion.ExecTest.TestGame.Combat;

namespace Emotion.ExecTest.TestGame.Abilities;

public class GodModeToggleAbility : Ability
{
    public static string AURA_ID = "GODMODE";

    public GodModeToggleAbility()
    {
        Flags = AbilityFlags.None;
    }

    public override IEnumerator OnUseAbility(Unit caster, Unit? target)
    {
        Aura? myAura = caster.HasAuraWithId(AURA_ID);
        if (myAura == null)
            caster.ApplyAura(new AbilityDerivedConstantAura(this, AURA_ID));
        else
            caster.RemoveAura(myAura);

        yield break;
    }
}

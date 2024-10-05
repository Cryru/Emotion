#nullable enable

using Emotion.Utility;

namespace Emotion.ExecTest.TestGame.Abilities;

public enum AbilityFlags
{
    None = 0,
    RequireTarget = 2 << 0,
    StartsGlobalCooldown = 2 << 1,
    CantBeUsedOnGlobalCooldown = 2 << 2,

    Default = RequireTarget | StartsGlobalCooldown | CantBeUsedOnGlobalCooldown
}

public class Ability
{
    public string Id => GetType().Name;

    public string Icon = string.Empty;

    public AbilityFlags Flags = AbilityFlags.Default;

    public virtual bool CanUse(Unit caster, Unit? target)
    {
        AbilityMeta? meta = caster.GetAbilityMeta(this);
        AssertNotNull(meta);
        if (meta != null)
        {
            float timeNow = Engine.CurrentGameTime;
            if (meta.CooldownTimeStamp > timeNow)
                return false;
        }

        if (Flags.EnumHasFlag(AbilityFlags.RequireTarget) && target == null)
            return false;

        if (Flags.EnumHasFlag(AbilityFlags.CantBeUsedOnGlobalCooldown))
        {
            float timeNow = Engine.CurrentGameTime;
            if (timeNow < caster.GlobalCooldownTimestamp)
                return false;
        }

        return true;
    }

    public virtual int GetCooldown(Unit caster, Unit? target)
    {
        return 0;
    }

    public virtual IEnumerator OnUseAbility(Unit caster, Unit? target)
    {
        yield break;
    }
}

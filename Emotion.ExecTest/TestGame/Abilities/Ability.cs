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

public enum AbilityCanUseResult
{
    CanUse,
    OnCooldown,
    NoTarget,
    OutOfRange,
    OnGlobalCooldown
}

public class Ability
{
    public string Id => GetType().Name;

    public string Icon = "Test/proto/abilities/placeholder.png";

    public AbilityFlags Flags = AbilityFlags.Default;

    public virtual AbilityCanUseResult CanUse(Unit caster, Unit? target)
    {
        AbilityMeta? meta = caster.GetAbilityMeta(this);
        AssertNotNull(meta);
        if (meta != null)
        {
            float timeNow = Engine.CurrentGameTime;
            if (meta.CooldownTimeStamp > timeNow)
                return AbilityCanUseResult.OnCooldown;
        }

        if (Flags.EnumHasFlag(AbilityFlags.RequireTarget) && (target == null || target.IsDead()))
            return AbilityCanUseResult.NoTarget;

        if (Flags.EnumHasFlag(AbilityFlags.CantBeUsedOnGlobalCooldown))
        {
            float timeNow = Engine.CurrentGameTime;
            if (timeNow < caster.GlobalCooldownTimestamp)
                return AbilityCanUseResult.OnGlobalCooldown;
        }

        return AbilityCanUseResult.CanUse;
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

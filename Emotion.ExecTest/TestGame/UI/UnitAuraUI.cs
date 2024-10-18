using Emotion.ExecTest.TestGame.Abilities;
using Emotion.ExecTest.TestGame.Combat;
using Emotion.Platform.Input;
using Emotion.UI;

namespace Emotion.ExecTest.TestGame.UI;

public class UnitAuraUI : UIBaseWindow
{
    public Unit Unit;
    public Aura Aura;

    public UnitAuraUI(Unit unit, Aura aura)
    {
        Unit = unit;
        Aura = aura;
    }

    protected override Vector2 NEW_InternalMeasure(Vector2 space)
    {
        return new Vector2(32, 32) * GetScale();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        AbilityIcon.RenderAura(c, Aura, Position, Size);

        return base.RenderInternal(c);
    }
}

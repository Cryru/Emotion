using Emotion.ExecTest.TestGame.Abilities;
using Emotion.Platform.Input;
using Emotion.UI;

namespace Emotion.ExecTest.TestGame.UI;

public class UnitSkillBarAbility : UIBaseWindow
{
    public Key Hotkey;

    public Unit Unit;
    public Ability Ability;

    public UnitSkillBarAbility(Unit unit, Ability ability)
    {
        Unit = unit;
        Ability = ability;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIRichText hotkey = new UIRichText
        {
            Text = Hotkey.ToString().Replace("Num", ""),
            WindowColor = Color.White,
            OutlineColor = Color.Black,
            OutlineSize = 2,
            FontSize = 28,
            Offset = new Vector2(3, -3),
            AnchorAndParentAnchor = UIAnchor.TopLeft,
            TextHeightMode = Game.Text.GlyphHeightMeasurement.NoDescent
        };
        AddChild(hotkey);
    }

    protected override Vector2 NEW_InternalMeasure(Vector2 space)
    {
        return new Vector2(64, 64) * GetScale();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        AbilityIcon.RenderAbility(c, Unit, Ability, Position, Size);

        return base.RenderInternal(c);
    }
}

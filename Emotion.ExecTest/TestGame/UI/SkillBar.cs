using Emotion.ExecTest.TestGame.Abilities;
using Emotion.Platform.Input;
using Emotion.UI;
using System.Globalization;
using System.Linq;

namespace Emotion.ExecTest.TestGame.UI;

public class UnitSkillBar : UIBaseWindow
{
    public Unit Unit;

    public UnitSkillBar(Unit u)
    {
        Unit = u;
        Anchor = UIAnchor.BottomCenter;
        ParentAnchor = UIAnchor.BottomCenter;
        Margins = new Rectangle(0, 0, 0, 5);

        FillX = false;
        FillY = false;
        HandleInput = true;
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        UIBaseWindow list = GetWindowById("List");
        foreach (UIBaseWindow wnd in list.WindowChildren())
        {
            if (wnd is UnitSkillBarAbility ab)
            {
                if (ab.Hotkey == key && status == KeyState.Down)
                {
                    Unit.SendUseAbility(ab.Ability);
                    return false;
                }
            }
        }

        return base.OnKey(key, status, mousePos);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var abilityList = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            Paddings = new Emotion.Primitives.Rectangle(1, 1, 1, 1),
            Margins = new Emotion.Primitives.Rectangle(0, 5, 0, 0),
            Id = "List"
        };
        AddChild(abilityList);

        List<Ability> abilities = Unit.Abilities;
        for (int i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];

            var abilityIcon = new UnitSkillBarAbility(Unit, ability)
            {
                Hotkey = (Key)((int)Key.Num0 + i + 1)
            };
            abilityList.AddChild(abilityIcon);
        }
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        float scale = GetScale();

        float casting = Unit.CastProgress;
        if (casting != 0f)
        {
            Rectangle barRect = new Rectangle(Position2, new Vector2(Width, 5 * scale));

            c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size - new Vector2(1f), new Color(32, 32, 32));
            c.RenderSprite(barRect.Position + new Vector2(0.5f), barRect.Size * new Vector2(casting, 1f) - new Vector2(1f), Color.PrettyYellow);
            c.RenderOutline(barRect, Color.Black);
        }

        return base.RenderInternal(c);
    }
}
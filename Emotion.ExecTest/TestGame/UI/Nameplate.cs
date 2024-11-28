using Emotion.ExecTest.TestGame.Combat;
using Emotion.UI;

#nullable enable

namespace Emotion.ExecTest.TestGame.UI;

public class Nameplate : UIBaseWindow
{
    private Unit _unit;

    public Nameplate(Unit ch)
    {
        _unit = ch;
        FillX = false;
        FillY = false;
        LayoutMode = LayoutMode.VerticalList;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIRichText name = new UIRichText
        {
            Text = _unit.Name,
            WindowColor = Color.Black,
            FontSize = 20,
            MinSizeY = 30
        };
        AddChild(name);

        HpBar hpBar = new HpBar(_unit);
        AddChild(hpBar);

        UIBaseWindow auraList = new UIBaseWindow()
        {
            Id = "AuraList",
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            FillX = false,
        };
        AddChild(auraList);
    }

    protected override bool UpdateInternal()
    {
        UIBaseWindow? auraList = GetWindowById("AuraList");
        if (auraList != null)
        {
            // Add missing ones
            foreach (Aura aura in _unit.ForEachAura())
            {
                bool found = false;
                foreach (UnitAuraUI auraWnd in auraList.EnumerateChildren())
                {
                    if (auraWnd.Aura == aura)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    auraList.AddChild(new UnitAuraUI(_unit, aura));
                }
            };

            // Remove ones not present anymore
            List<UnitAuraUI> toClose = new List<UnitAuraUI>();
            foreach (UnitAuraUI auraWnd in auraList.EnumerateChildren())
            {
                bool found = false;
                foreach (Aura aura in _unit.ForEachAura())
                {
                    if (auraWnd.Aura == aura)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    toClose.Add(auraWnd);
                }
            }

            for (int i = 0; i < toClose.Count; i++)
            {
                UnitAuraUI auraWnd = toClose[i];
                auraWnd.Close();
            }
        }

        return base.UpdateInternal();
    }
}

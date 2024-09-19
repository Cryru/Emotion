using Emotion.UI;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class Nameplate : UIBaseWindow
{
    private Character _char;

    public Nameplate(Character ch)
    {
        _char = ch;
        FillX = false;
        FillY = false;
        LayoutMode = LayoutMode.VerticalList;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIRichText name = new UIRichText
        {
            Text = _char.Name,
            WindowColor = Color.Black,
            FontSize = 20,
            MinSizeY = 30
        };
        AddChild(name);

        HpBar hpBar = new HpBar(_char);
        AddChild(hpBar);
    }
}

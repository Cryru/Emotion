﻿using Emotion.UI;

#nullable enable

namespace Emotion.ExecTest.TestGame.UI;

public class HpBar : UIBaseWindow
{
    private Unit _char;

    public HpBar(Unit ch)
    {
        _char = ch;
        FillX = false;
        FillY = false;
    }

    protected override Vector2 NEW_InternalMeasure(Vector2 space)
    {
        return new Vector2(200, 30) * GetScale();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.RenderSprite(Bounds, new Color(120, 120, 120));

        float hpPercent = (float)_char.Health / _char.MaxHealth;

        c.RenderSprite(Position, new Vector2(Width * hpPercent, Height), Color.PrettyRed);
        c.RenderOutline(Bounds, Color.Black, 2);
        return base.RenderInternal(c);
    }
}
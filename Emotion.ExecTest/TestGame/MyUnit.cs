using Emotion.Common.Serialization;
using Emotion.ExecTest.TestGame.UI;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.ExecTest.TestGame;

[DontSerialize]
public class MyUnit : PlayerUnit
{
    public MyUnit(uint id) : base(id)
    {
        Name = "You";
        Image = "Test/proto/person_you";
        LocallyControlled = true;
    }

    public override void SetTarget(Unit? ch)
    {
        Target?.SetRenderTargetMode(false);
        ch?.SetRenderTargetMode(true);

        TestScene? scene = Engine.SceneManager.Current as TestScene;
        if (scene != null)
        {
            var uiParent = scene.UIParent;

            UIBaseWindow? targetNameplate = uiParent.GetWindowById("TargetNameplate");
            targetNameplate?.Close();

            if (ch != null)
            {
                var nameplate = new Nameplate(ch)
                {
                    Offset = new Vector2(350, 0),
                    Margins = new Rectangle(5, 5, 5, 5),
                    Id = "TargetNameplate"
                };
                uiParent.AddChild(nameplate);
            }
        }

        base.SetTarget(ch);
    }
}

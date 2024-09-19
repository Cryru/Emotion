using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.ExecTest.TestGame;

public class MyCharacter : Character
{
    public MyCharacter()
    {
        Name = "You";
        Image = "Test/proto/person_you";
    }

    public override void SetTarget(Character? ch)
    {
        Target?.SetRenderTargetMode(false);
        ch?.SetRenderTargetMode(true);

        base.SetTarget(ch);
    }
}

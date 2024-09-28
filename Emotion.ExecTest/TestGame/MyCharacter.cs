using Emotion.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.ExecTest.TestGame;

[DontSerialize]
public class MyCharacter : PlayerCharacter
{
    public MyCharacter(uint id) : base(id)
    {
        Name = "You";
        Image = "Test/proto/person_you";
        LocallyControlled = true;
    }

    public override void SetTarget(Character? ch)
    {
        Target?.SetRenderTargetMode(false);
        ch?.SetRenderTargetMode(true);

        base.SetTarget(ch);
    }
}

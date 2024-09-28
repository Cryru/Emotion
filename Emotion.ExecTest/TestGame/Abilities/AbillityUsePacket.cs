using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.ExecTest.TestGame.Abilities;

public struct AbillityUsePacket
{
    public Ability AbilityInstance; // todo: id or something
    public uint UserId;
    public uint TargetId;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.ExecTest.TestGame.Abilities;

namespace Emotion.ExecTest.TestGame.Packets;

public struct AbillityUsePacket
{
    public string AbilityId;
    public uint UserId;
    public uint TargetId;
}

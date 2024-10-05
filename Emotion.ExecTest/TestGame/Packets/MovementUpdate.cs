#nullable enable

using Emotion;

namespace Emotion.ExecTest.TestGame.Packets;

public struct MovementUpdate
{
    public uint ObjectId;
    public Vector2 Pos;
}
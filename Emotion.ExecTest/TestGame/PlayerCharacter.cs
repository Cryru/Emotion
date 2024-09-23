#nullable enable

namespace Emotion.ExecTest.TestGame;

public class PlayerCharacter : Character
{
    public uint PlayerId { get; set; }

    public PlayerCharacter(uint networkId) : base(PLAYER_OBJECT_OFFSET + networkId)
    {
        PlayerId = networkId;

        Name = $"Player {networkId}";
        Image = "Test/proto/person";
    }

    // serialization
    protected PlayerCharacter()
    {

    }
}
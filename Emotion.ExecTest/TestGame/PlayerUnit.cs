#nullable enable

using Emotion.ExecTest.TestGame.Abilities;

namespace Emotion.ExecTest.TestGame;

public class PlayerUnit : Unit
{
    public uint PlayerId { get; set; }

    public PlayerUnit(uint networkId) : base(PLAYER_OBJECT_OFFSET + networkId)
    {
        PlayerId = networkId;

        Name = $"Player {networkId}";
        Image = "Test/proto/person";

        MeleeDamage = 10;

        Abilities.Add(new MeleeAttack());
        Abilities.Add(new AbilityThrash());
        Abilities.Add(new GodModeToggleAbility());
    }

    // serialization
    protected PlayerUnit()
    {

    }
}
#nullable enable

namespace Emotion.ExecTest.TestGame;

public class NetworkCharacter : Character
{
    public int NetworkId { get; }

    public NetworkCharacter(int networkId)
    {
        NetworkId = networkId;
    }
}
#region Using

using Emotion.Network.Game;
using Emotion.Network.Infrastructure;

#endregion

namespace Emotion.Network.Dbg
{
    public class TestGameServer : GameServer
    {
        protected override NetworkScene GetNewSceneInstance()
        {
            return new TestScene(this, 30);
        }

        protected override bool CanAddPlayer(GameClient client)
        {
            return Clients.Count < 2;
        }

        protected override void OnPlayerAdded(GameClient client)
        {
            
        }

        protected override bool GameShouldStart()
        {
            return Clients.Count > 0;
        }
    }
}
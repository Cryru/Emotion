#region Using

using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Network.Game;
using Emotion.Network.Infrastructure;

#endregion

namespace Emotion.Network.Dbg
{
    public class TestGameClient : GameClient
    {
        public TestGameClient(string playerId) : base(playerId)
        {
        }

        protected override async Task<NetworkScene> JoinedGame()
        {
            var scene = new TestScene(this);
            await Engine.SceneManager.SetScene(scene);
            _scene = scene;
            return scene;
        }
    }
}
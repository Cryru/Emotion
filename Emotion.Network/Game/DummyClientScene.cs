#region Using

using System;
using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.Network.Infrastructure;

#endregion

namespace Emotion.Network.Game
{
    public class DummyClientScene : NetworkScene
    {
        public DummyClientScene(GameClient player) : base(player)
        {
        }

        public DummyClientScene(GameServer server, int msBetweenTicks) : base(server, msBetweenTicks)
        {
        }

        public override Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        public override void Draw(RenderComposer composer)
        {
        }

        public override void Unload()
        {
        }

        public override void LoadServer()
        {
            throw new NotImplementedException();
        }

        public override void UpdateServer(float advanceTime)
        {
            throw new NotImplementedException();
        }
    }
}
#region Using

using Emotion.Network.Infrastructure;

#endregion

namespace Emotion.Network.Game
{
    public abstract partial class NetworkScene
    {
        public GameServer Server;
        public int TimeBetweenTicks;

        protected NetworkScene(GameServer server, int msBetweenTicks)
        {
            Server = server;
            TimeBetweenTicks = msBetweenTicks;
            NetworkHandle = NetworkActorHandle.ServerHandle;
        }

        public abstract void LoadServer();
        public abstract void UpdateServer(float advanceTime);
    }
}
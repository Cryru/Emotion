#region Using

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Network.Game;

#endregion

namespace Emotion.Network.Infrastructure
{
    public abstract class GameClient : NetworkActor
    {
        public TimeSpan Ping;
        public bool Running
        {
            get => _msgThread != null;
        }

        private Thread _msgThread;
        private GameServer _server;
        protected NetworkScene _scene;

        protected GameClient(string playerId)
        {
            Handle = new NetworkActorHandle(playerId);
        }

        public void Run()
        {
            _msgThread = new Thread(RunThread);
            _msgThread.Start();
            while (!_msgThread.IsAlive)
            {
            }
        }

        private void RunThread()
        {
            // We need a scene loaded to process messages.
            Engine.SceneManager.SetScene(new DummyClientScene(this)).Wait();
            Thread.CurrentThread.Name ??= "Client Message Thread";
            while (Engine.Status == EngineStatus.Running) ReceiveMessages();
        }

        protected override bool ProcessMessage(out NetworkMessage msg, out NetworkActor sender)
        {
            if (Program.DbgClientLag != 0)
            {
                msg = null;
                sender = null;
                if (!_receivedQueue.TryPeek(out WrappedNetworkMessage wrappedMsg)) return false;
                if (wrappedMsg.Sent.AddMilliseconds(Program.DbgClientLag) > DateTime.Now) return false;
            }

            if (_receivedQueue.TryPeek(out WrappedNetworkMessage mm))
            {
                Ping = DateTime.Now - mm.Sent;
            }

            return base.ProcessMessage(out msg, out sender);
        }

        public void ProcessMessages()
        {
            while (ProcessMessage(out NetworkMessage msg, out NetworkActor _))
            {
                switch (msg.Type)
                {
                    case MessageType.JoinGameDenied:
                        _server = null;
                        break;
                    case MessageType.JoinGameSuccess:
                        JoinedGame().ContinueWith(x =>
                        {
                            Debug.Assert(_scene != null);
                            _server.SendMessage(this, MessageType.ClientSceneReady);
                        });
                        break;
                    case MessageType.GameInitialSync:
                        _scene.ClientReadSceneFromMessage(msg);
                        _server.SendMessage(this, MessageType.GameInitialSyncReady);
                        break;
                    case MessageType.GameStart:
                        _scene.Running = true;
                        break;
                    case MessageType.GameSync:
                        _scene.ClientReadSceneFromMessage(msg);
                        break;
                }
            }
        }

        public void JoinGame(GameServer server)
        {
            if (_server != null) throw new Exception("Trying to join a game while in a game (or trying to join a game).");

            server.SendMessage(this, MessageType.JoinGameRequest);
            _server = server;
        }

        protected abstract Task<NetworkScene> JoinedGame();
    }
}
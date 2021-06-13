#region Using

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Network.Game;

#endregion

namespace Emotion.Network.Infrastructure
{
    public enum GameState
    {
        NotRan,
        Waiting,
        InitialSync,
        Running,
        Ended
    }

    public class GameClientPlayer
    {
        public enum PlayerState
        {
            Joined,
            Initialized,
            Ready,
            Playing,
            Disconnected
        }

        public GameClient Client;
        public PlayerState State = PlayerState.Joined;
    }

    public abstract class GameServer : NetworkActor
    {
        public static GameServer Server;

        public ConcurrentDictionary<NetworkActor, GameClientPlayer> Clients = new();
        public List<NetworkActor> ClientsList = new();
        public GameState State { get; protected set; } = GameState.NotRan;
        public NetworkScene Scene;
        private Thread _serverThread;

        protected GameServer()
        {
            Handle = NetworkActorHandle.ServerHandle;
            Server = this;
        }

        public void Run()
        {
            State = GameState.Waiting;
            _serverThread = new Thread(RunThread);
            _serverThread.Start();
            while (!_serverThread.IsAlive)
            {
            }
        }

        private void RunThread()
        {
            Thread.CurrentThread.Name ??= "Server Message Thread";
            while (State != GameState.Ended && Engine.Status != EngineStatus.Stopped)
            {
                ReceiveMessages();
                if (State != GameState.Running) // In this state the messages are processed by the game thread.
                    while (ProcessMessage(out NetworkMessage msg, out NetworkActor sender))
                    {
                        switch (msg.Type)
                        {
                            case MessageType.JoinGameRequest:
                                if (AddPlayer((GameClient) sender)) Engine.Log.Info($"Player {sender.Handle} has joined the game.", $"{Handle}");
                                break;
                            case MessageType.ClientSceneReady:
                            {
                                GameClientPlayer player = ResolvePlayer(sender);
                                if (player == null) continue;
                                player.State = GameClientPlayer.PlayerState.Initialized;
                                Engine.Log.Info($"Player {player.Client.Handle} has loaded their scene.", $"{Handle}");
                            }
                                break;
                            case MessageType.GameInitialSyncReady:
                            {
                                GameClientPlayer player = ResolvePlayer(sender);
                                if (player == null) continue;
                                player.State = GameClientPlayer.PlayerState.Ready;
                                Engine.Log.Info($"Player {player.Client.Handle} initial sync ready.", $"{Handle}");
                            }
                                break;
                        }
                    }

                switch (State)
                {
                    case GameState.Waiting:
                        if (AreAllClientsState(GameClientPlayer.PlayerState.Initialized) && GameShouldStart())
                        {
                            Engine.Log.Info("Sending initial sync.", $"{Handle}");
                            Scene = GetNewSceneInstance();
                            Scene.LoadServer();
                            State = GameState.InitialSync;
                            var initialSyncMsg = new NetworkMessage
                            {
                                Type = MessageType.GameInitialSync
                            };
                            Scene.WriteSceneToMessage(initialSyncMsg);
                            SendMessageToAllClients(initialSyncMsg);
                        }

                        break;
                    case GameState.InitialSync:
                        if (AreAllClientsState(GameClientPlayer.PlayerState.Ready))
                        {
                            Engine.Log.Info("All clients ready - starting game.", $"{Handle}");
                            State = GameState.Running;
                            var gameStart = new NetworkMessage
                            {
                                Type = MessageType.GameStart
                            };
                            SendMessageToAllClients(gameStart);
                            Task.Run(GameThread);
                        }

                        break;
                    case GameState.Running:
                        // running in the game thread.
                        break;
                }
            }
        }

        private void GameThread()
        {
            int timeBetweenTicks = Scene.TimeBetweenTicks;
            Thread.CurrentThread.Name ??= "Game Processing Thread";
            Scene.Running = true;
            while (State == GameState.Running)
            {
                // Gather data from clients.
                while (ProcessMessage(out NetworkMessage msg, out NetworkActor sender))
                {
                    switch (msg.Type)
                    {
                    }
                }

                Scene.UpdateServer(timeBetweenTicks);

                var syncMsg = new NetworkMessage
                {
                    Type = MessageType.GameSync
                };
                Scene.WriteSceneToMessage(syncMsg);
                SendMessageToAllClients(syncMsg);

                Task.Delay(timeBetweenTicks).Wait();
            }
        }

        public void Stop()
        {
            State = GameState.Ended;
        }

        public bool AddPlayer(GameClient client)
        {
            lock (Clients)
            {
                if (!CanAddPlayer(client))
                {
                    client.SendMessage(this, MessageType.JoinGameDenied);
                    return false;
                }

                bool added = Clients.TryAdd(client, new GameClientPlayer
                {
                    Client = client
                });
                if (!added)
                {
                    client.SendMessage(this, MessageType.JoinGameDenied);
                    return false;
                }

                ClientsList.Add(client);
                client.SendMessage(this, MessageType.JoinGameSuccess);
                OnPlayerAdded(client);
                return true;
            }
        }

        protected GameClientPlayer ResolvePlayer(NetworkActor actor)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            Clients.TryGetValue(actor, out GameClientPlayer player);
            return player;
        }

        protected void SendMessageToAllClients(NetworkMessage msg)
        {
            lock (Clients)
            {
                foreach ((NetworkActor actor, GameClientPlayer player) in Clients)
                {
                    if (player.State == GameClientPlayer.PlayerState.Disconnected) continue;
                    actor.SendMessage(this, msg);
                }
            }
        }

        protected bool AreAllClientsState(GameClientPlayer.PlayerState state)
        {
            lock (Clients)
            {
                foreach ((NetworkActor _, GameClientPlayer player) in Clients)
                {
                    if (player.State == state) continue;
                    return false;
                }
            }

            return true;
        }

        protected abstract NetworkScene GetNewSceneInstance();
        protected abstract bool CanAddPlayer(GameClient client);
        protected abstract void OnPlayerAdded(GameClient client);
        protected abstract bool GameShouldStart();
    }
}
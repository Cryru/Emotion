using System.Collections.Generic;
using Emotion.Network.Game;

namespace Emotion.Network.Infrastructure
{
    public enum MessageType : byte
    {
        JoinGameRequest, // Client requests to join game.
        JoinGameDenied,
        JoinGameSuccess,

        ClientSceneReady, // Client reports that it has initialized its scene and is ready for initial sync.

        GameInitialSync, // Initial scene synchronization.
        GameInitialSyncReady,

        GameStart,
        GameSync,
    }

    public class NetworkMessage
    {
        public MessageType Type;

        // local debug
        public float Timestamp;
        public List<NetworkTransform> Data = new ();
    }
}
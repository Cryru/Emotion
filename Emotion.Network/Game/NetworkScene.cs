#region Using

using System.Collections.Generic;
using Emotion.Network.Infrastructure;

#endregion

namespace Emotion.Network.Game
{
    public abstract partial class NetworkScene
    {
        public const int MAX_TICK_BACK = 2; // The faster the server ticks the larger this should be.
        public bool Running;
        public float Time;
        public NetworkActorHandle NetworkHandle;
        public List<NetworkTransform> SyncedObjects = new();
        public Dictionary<string, NetworkTransform> IdToObject = new();
        public List<NetworkTransform> OwnedObjects = new();

        public void AddObject(NetworkTransform transform)
        {
            transform.Parent = this;
            SyncedObjects.Add(transform);
            IdToObject.Add(transform.ObjectId, transform);
            if (transform.Owner == NetworkHandle) OwnedObjects.Add(transform);
        }

        public void RemoveObject(NetworkTransform transform)
        {
            SyncedObjects.Remove(transform);
            IdToObject.Remove(transform.ObjectId);
            OwnedObjects.Remove(transform);
        }

        public void WriteSceneToMessage(NetworkMessage msg)
        {
            msg.Timestamp = Time;
            msg.Data.Clear();
            for (var i = 0; i < SyncedObjects.Count; i++)
            {
                NetworkTransform obj = SyncedObjects[i];
                var transform = new NetworkTransform(obj.ObjectId)
                {
                    Owner = obj.Owner,
                    Position = obj.Position,
                    Size = obj.Size
                };

                msg.Data.Add(transform);
            }
        }
    }
}
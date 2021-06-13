#region Using

using System.Collections.Generic;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Network.Infrastructure;
using Emotion.Scenography;
using Emotion.Standard.XML;
using Emotion.Utility;

#endregion

namespace Emotion.Network.Game
{
    public abstract partial class NetworkScene : Scene
    {
        public GameClient Client;
        public float TimeDelta;
        public float MaxDelta;
        public float ServerTime;

        protected NetworkScene(GameClient player)
        {
            NetworkHandle = player.Handle;
            Client = player;
        }

        public void ClientReadSceneFromMessage(NetworkMessage msg)
        {
            float msgTime = msg.Timestamp;
            MaxDelta = (msgTime - ServerTime) * MAX_TICK_BACK;
            ServerTime = msgTime;
            List<NetworkTransform> objs = msg.Data;

            for (var i = 0; i < objs.Count; i++)
            {
                NetworkTransform obj = objs[i];
                IdToObject.TryGetValue(obj.ObjectId, out NetworkTransform myObj);
                if (myObj == null)
                {
                    var fakeXml = $"<?xml><NetworkTransform type=\"{obj.GetType()}\"></NetworkTransform></xml>";
                    var newObj = XMLFormat.From<NetworkTransform>(fakeXml);
                    newObj.Owner = new NetworkActorHandle(obj.Owner.Id);
                    newObj.ObjectId = obj.ObjectId;
                    AddObject(newObj);
                    newObj.ApplyFullData(msgTime, obj);
                    continue;
                }

                myObj.AddStatePoint(msgTime, obj);
            }
        }

        public override void Update()
        {
            Client.ProcessMessages();
            if (!Running) return;

            Time += Engine.DeltaTime;
            TimeDelta = ServerTime - Time;
            // Recovering from a lag spike.
            if (TimeDelta > MaxDelta) Time += TimeDelta * 0.2f;
            Time = Maths.Clamp(Time, 0, ServerTime);

            // Interpolate states.
            for (var i = 0; i < SyncedObjects.Count; i++)
            {
                NetworkTransform obj = SyncedObjects[i];
                obj.ApplyStateAtTime(Time);
            }
        }
    }
}
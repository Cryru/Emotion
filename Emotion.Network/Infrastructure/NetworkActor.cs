#region Using

using System;
using System.Collections.Concurrent;
using System.Threading;

#endregion

namespace Emotion.Network.Infrastructure
{
    public class WrappedNetworkMessage
    {
        public NetworkMessage Msg;
        public NetworkActor Sender;
        public DateTime Sent;
    }

    public class NetworkActor
    {
        public NetworkActorHandle Handle;
        protected ConcurrentQueue<WrappedNetworkMessage> _msgQueue = new();
        protected ConcurrentQueue<WrappedNetworkMessage> _receivedQueue = new();
        protected AutoResetEvent _msgReceive = new(false);

        public virtual void SendMessage(NetworkActor sender, NetworkMessage msg)
        {
            _msgQueue.Enqueue(new WrappedNetworkMessage
                {
                    Msg = msg,
                    Sender = sender,
                    Sent = DateTime.Now
                }
            );
            _msgReceive.Set();
        }

        public void SendMessage(NetworkActor sender, MessageType msgType)
        {
            SendMessage(sender, new NetworkMessage
            {
                Type = msgType
            });
            _msgReceive.Set();
        }

        protected void ReceiveMessages()
        {
            while (!_msgQueue.IsEmpty)
            {
                if (_msgQueue.TryDequeue(out WrappedNetworkMessage msg))
                {
                    _receivedQueue.Enqueue(msg);
                }
            }
            _msgReceive.WaitOne(1000);
        }

        protected virtual bool ProcessMessage(out NetworkMessage msg, out NetworkActor sender)
        {
            msg = null;
            sender = null;

            bool gotMsg = _receivedQueue.TryDequeue(out WrappedNetworkMessage wrapped);
            if (!gotMsg) return false;

            msg = wrapped.Msg;
            sender = wrapped.Sender;
            return true;
        }
    }
}
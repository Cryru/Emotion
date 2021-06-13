namespace Emotion.Network.Infrastructure
{
    public class NetworkActorHandle
    {
        public static NetworkActorHandle ServerHandle = new("Server");

        public string Id { get; }

        public NetworkActorHandle()
        {
            Id = NetworkUtil.GenerateId("Player");
        }

        public NetworkActorHandle(string id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"<H{Id}>";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NetworkActorHandle);
        }

        public bool Equals(NetworkActorHandle obj)
        {
            if (obj == null) return false;
            return obj.Id == Id;
        }

        public static bool operator ==(NetworkActorHandle lhs, NetworkActorHandle rhs)
        {
            if (lhs is not null) return lhs.Equals(rhs);
            return rhs is null;
        }

        public static bool operator !=(NetworkActorHandle lhs, NetworkActorHandle rhs)
        {
            return !(lhs == rhs);
        }
    }
}
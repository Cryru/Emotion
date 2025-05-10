using Emotion.Network.Base;
using Emotion.Network.ServerSide;
using Emotion.Network.ServerSide.Gameplay;

#nullable enable

namespace Emotion.Network.ServerAuthoritative;

public class AuthoritativeTimeSyncGameplay : ServerTimeSyncRoomGameplay
{
    protected Dictionary<int, ServerFunction> _functions = new Dictionary<int, ServerFunction>();

    protected void RegisterServerFunction<T>(string name, Action<ServerUser, T> func)
    {
        var funcDef = new ServerFunction<T>(name, func);
        _functions.Add(name.GetStableHashCodeASCII(), funcDef);
    }

    protected override void OnGameplayMessageReceived(ServerUser sender, NetworkMessage msg)
    {
        Utility.ByteReader? reader = msg.GetContentReader();
        if (reader == null) return;

        reader.ReadByte(); // message type

        int methodNameLength = reader.ReadInt32();
        var methodNameBytes = reader.ReadBytes(methodNameLength);
        int methodNameHash = methodNameBytes.GetStableHashCode();

        if (_functions.TryGetValue(methodNameHash, out ServerFunction? func))
        {
            var metaDataLength = reader.ReadInt32();
            var metaDataBytes = reader.ReadBytes(metaDataLength);
            func.Invoke(sender, metaDataBytes);
        }
    }
}

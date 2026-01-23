#nullable enable

using Emotion.Network.ServerSide;

namespace Emotion.Network.TimeSyncMessageBroker;

public class TimeHashPair
{
    struct TimeHashPairStruct
    {
        public ServerUser User;
        public int Hash;
        public string Meta;
    }

    private List<TimeHashPairStruct> _timeHashes = new(6);
    private int _playerCountAtCreation;

    public TimeHashPair(int players)
    {
        _playerCountAtCreation = players;
    }

    public void AddHash(ServerUser user, int hash, string meta)
    {
        TimeHashPairStruct str = new TimeHashPairStruct()
        {
            User = user,
            Hash = hash,
            Meta = meta
        };

        _timeHashes.Add(str);
    }

    public bool ContainsUser(ServerUser user)
    {
        for (int i = 0; i < _timeHashes.Count; i++)
        {
            if (_timeHashes[i].User == user) return true;
        }
        return false;
    }

    public bool IsFull()
    {
        return _playerCountAtCreation == _timeHashes.Count;
    }

    public bool Verify()
    {
        TimeHashPairStruct hashStructFirstPlayer = _timeHashes[0];

        bool hasMeta = hashStructFirstPlayer.Meta != string.Empty;
        bool hasError = false;

        int hashFirst = hashStructFirstPlayer.Hash;
        for (int i = 1; i < _timeHashes.Count; i++)
        {
            TimeHashPairStruct hashOtherStruct = _timeHashes[i];

            int hashOther = hashOtherStruct.Hash;
            bool hashMatch = hashFirst == hashOther;
            if (!hashMatch)
            {
                Engine.Log.Warning($"Hash mismatch: [0] {hashFirst} & [{i}] {hashOther}!", "Server");
                if (hasMeta)
                {
                    Engine.Log.Warning($"   Meta First: {hashStructFirstPlayer.Meta}", "Server");
                    Engine.Log.Warning($"    Meta Fail: {hashOtherStruct.Meta}", "Server");
                }
                hasError = true;
            }
        }

        return hasError;
    }
}
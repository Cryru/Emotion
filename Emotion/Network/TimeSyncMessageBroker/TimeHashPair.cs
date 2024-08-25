using Emotion.Network.ServerSide;

#nullable enable

namespace Emotion.Network.TimeSyncMessageBroker;

public class TimeHashPair
{
    private List<(ServerUser user, int hash)> _timeHashes = new(6);
    private int _playerCountAtCreation;

    public TimeHashPair(int players)
    {
        _playerCountAtCreation = players;
    }

    public void AddHash(ServerUser user, int hash)
    {
        _timeHashes.Add((user, hash));
    }

    public bool ContainsUser(ServerUser user)
    {
        for (int i = 0; i < _timeHashes.Count; i++)
        {
            if (_timeHashes[i].user == user) return true;
        }
        return false;
    }

    public bool IsFull()
    {
        return _playerCountAtCreation == _timeHashes.Count;
    }

    public void Verify()
    {
        int hashFirst = _timeHashes[0].hash;
        for (int i = 1; i < _timeHashes.Count; i++)
        {
            int hashOther = _timeHashes[i].hash;
            bool hashMatch = hashFirst == hashOther;
            if (!hashMatch)
            {
                Engine.Log.Warning($"Hash mismatch: {hashFirst}!", "Server");
            }
        }
    }
}
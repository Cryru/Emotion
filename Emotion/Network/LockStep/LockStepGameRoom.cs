#nullable enable

using Emotion.Network.Base;
using Emotion.Network.ServerSide;
using System.Runtime.InteropServices;

namespace Emotion.Network.LockStep;

public static class GameRoomInvokerExtensions
{
    public static void RegisterLockStepEvent<TEnum>(this ServerNetworkFunctionInvoker<ServerRoom, ServerPlayer> invoker, TEnum messageType)
        where TEnum : unmanaged, Enum
    {
        invoker.RegisterDirect(messageType, LockStepGameRoom.Msg_LockStep);
    }

    public static void RegisterLockStepEvent(this ServerNetworkFunctionInvoker<ServerRoom, ServerPlayer> invoker, uint messageType)
    {
        invoker.RegisterDirect(messageType, LockStepGameRoom.Msg_LockStep);
    }
}

public class LockStepGameRoom : TickingServerRoom
{
    private float _lastTickRealTime;
    private Dictionary<ServerPlayer, LoggingProvider> _loggers = new Dictionary<ServerPlayer, LoggingProvider>();

    public LockStepGameRoom(ServerBase server, ServerPlayer? host, uint roomId) : base(server, host, roomId)
    {

    }

    protected override void OnGameplayTick(uint dt)
    {
        base.OnGameplayTick(dt);
        _lastTickRealTime = Engine.CurrentRealTime;
    }

    internal void OnLockStepMsg(in NetworkMessage msg)
    {
        // Resend message to all players
        float timeDiff = Engine.CurrentRealTime - _lastTickRealTime;
        timeDiff = Math.Min(timeDiff, GameTimeTickInterval);
        uint timeDiffInt = (uint)Math.Floor(timeDiff);

        NetworkMessage msgCopy = msg;
        msgCopy.GameTime = GameTime + timeDiffInt;

        foreach (ServerPlayer player in UsersInside)
        {
            Server.SendMessageToPlayerRaw(player, msgCopy);
        }
    }

    internal static void Msg_LockStep(ServerRoom room, ServerPlayer sender, in NetworkMessage msg)
    {
        if (room is not LockStepGameRoom syncRoom) return;
        syncRoom.OnLockStepMsg(msg);
    }

    #region Desync Logging

    public bool LogForDesyncs = true;
    private bool _desynced;

    protected override void OnUserJoined(ServerPlayer newUser)
    {
        base.OnUserJoined(newUser);

        if (!LogForDesyncs) return;
        _loggers.Add(newUser, new NetUIAsyncLoggerSingleFile($"Logs/Network/Room_{Id}_{_createdAt.ToString("yyyy-dd-M--HH-mm")}", $"{newUser.Id}")
        {
            ExcludeExtraData = true
        });
    }

    public override void Dispose()
    {
        base.Dispose();

        foreach ((ServerPlayer player, LoggingProvider logger) in _loggers)
        {
            logger.Dispose();
        }
        _loggers.Clear();
    }

    private struct LockStepVerifyPair
    {
        public ServerPlayer? Player = null;
        public LockStepVerify Hash;

        public LockStepVerifyPair()
        {

        }
    }

    private struct LockStepVerifyGroup
    {
        public LockStepVerifyPair[] Pairs;

        public LockStepVerifyGroup(int playersInRoom)
        {
            Pairs = new LockStepVerifyPair[playersInRoom];
        }

        public readonly bool AllPlayersReported()
        {
            for (int i = 0; i < Pairs.Length; i++)
            {
                if (Pairs[i].Player == null)
                    return false;
            }
            return true;
        }
    }

    private List<LockStepVerifyGroup> _unverifiedGroups = new List<LockStepVerifyGroup>();

    public void PlayerReportedHash(ServerPlayer player, LockStepVerify hash)
    {
        if (!LogForDesyncs) return;
        if (_desynced) return;

        if (_loggers.TryGetValue(player, out LoggingProvider? logger))
        {
            Span<char> hashSpan = hash.GetSpan();
            logger.ONE_Info(hashSpan.ToString(), hash.GameTime.ToString());
        }

        // Add player to first hash group in which they are missing.
        bool foundFirst = false;
        for (int i = 0; i < _unverifiedGroups.Count; i++)
        {
            LockStepVerifyGroup hashGroup = _unverifiedGroups[i];
            LockStepVerifyPair[] pairs = hashGroup.Pairs;

            bool playerPresentInPair = false;
            int firstFree = -1;
            for (int ii = 0; ii < pairs.Length; ii++)
            {
                LockStepVerifyPair pair = pairs[ii];
                if (pair.Player == player)
                {
                    playerPresentInPair = true;
                    break;
                }
                else if (pair.Player == null)
                {
                    firstFree = ii;
                    break;
                }
            }

            if (!playerPresentInPair)
            {
                pairs[firstFree].Player = player;
                pairs[firstFree].Hash = hash;
                foundFirst = true;
                break;
            }
        }

        // Not missing from any group? That's a new group.
        if (!foundFirst)
        {
            LockStepVerifyGroup newGroup = new LockStepVerifyGroup(UsersInside.Count);
            newGroup.Pairs[0].Player = player;
            newGroup.Pairs[0].Hash = hash;
            _unverifiedGroups.Add(newGroup);
        }

        // Verify full groups.
        for (int i = _unverifiedGroups.Count - 1; i >= 0; i--)
        {
            LockStepVerifyGroup hashGroup = _unverifiedGroups[i];
            if (hashGroup.AllPlayersReported())
            {
                _unverifiedGroups.RemoveAt(i);
                LockStepVerifyPair[] pairs = hashGroup.Pairs;
                bool allMatch = true;
                Span<char> firstHash = pairs[0].Hash.GetSpan();
                Span<char> notMatch = Span<char>.Empty;
                for (int ii = 1; ii < pairs.Length; ii++)
                {
                    LockStepVerifyPair pair = pairs[ii];
                    Span<char> pairHash = pair.Hash.GetSpan();
                    if (!pairHash.SequenceEqual(firstHash))
                    {
                        notMatch = pairHash;
                        allMatch = false;
                        break;
                    }
                }
                if (!allMatch)
                {
                    Engine.Log.Error($"Room {Id} desynced!", nameof(LockStepGameRoom));
                    _desynced = true;
                }
            }
        }
    }

    #endregion
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct LockStepVerify
{
    public const int MAX_LENGTH = 252;

    public float GameTime;
    private int Length;
    private fixed char String[MAX_LENGTH];

    public LockStepVerify(ReadOnlySpan<char> str)
    {
        GameTime = Engine.CoroutineManagerGameTime.Time;
        Length = Math.Min(str.Length, MAX_LENGTH);

        fixed (char* pString = String)
        {
            Span<char> localStr = new Span<char>(pString, Length);
            ReadOnlySpan<char> strCapped = str.Slice(0, Length);
            strCapped.CopyTo(localStr);
        }
    }

    public override string ToString()
    {
        if (Length > MAX_LENGTH || Length < 0) return string.Empty;

        fixed (char* pString = String)
        {
            Span<char> localStr = new Span<char>(pString, Length);
            return localStr.ToString();
        }
    }

    public Span<char> GetSpan()
    {
        if (Length > MAX_LENGTH || Length < 0) return Span<char>.Empty;

        fixed (char* pString = String)
        {
            return new Span<char>(pString, Length);
        }
    }
}
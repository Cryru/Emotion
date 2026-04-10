#nullable enable

using Emotion.Network.Base;
using Emotion.Network.New.Base;
using Emotion.Network.ServerSide;
using Emotion.Network.World;
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
        public LockStepVerify HashPair;

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
            logger.ONE_Info(hash.GameTime.ToString(), hash.Hash.ToString());
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
                pairs[firstFree].HashPair = hash;
                foundFirst = true;
                break;
            }
        }

        // Not missing from any group? That's a new group.
        if (!foundFirst)
        {
            LockStepVerifyGroup newGroup = new LockStepVerifyGroup(UsersInside.Count);
            newGroup.Pairs[0].Player = player;
            newGroup.Pairs[0].HashPair = hash;
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
                int firstHash = pairs[0].HashPair.Hash;
                int notMatch = 0;
                for (int ii = 1; ii < pairs.Length; ii++)
                {
                    LockStepVerifyPair pair = pairs[ii];
                    int pairHash = pair.HashPair.Hash;
                    if (pairHash != firstHash)
                    {
                        notMatch = pairHash;
                        allMatch = false;
                        break;
                    }
                }
                if (!allMatch)
                {
                    Engine.Log.Error($"Room {Id} desynced! Expected hash {firstHash}, got {notMatch}", nameof(LockStepGameRoom));
                    _desynced = true;
                }
            }
        }
    }

    #endregion

    public static void RegisterNetFunctions()
    {
        ServerRoom.NetworkFunctions.Register<NetworkMessageType, LockStepVerify>(NetworkMessageType.LockStepVerify, OnTimeSyncHash);
    }

    private static void OnTimeSyncHash(ServerRoom self, ServerPlayer sender, in LockStepVerify hash)
    {
        if (self is not LockStepGameRoom lockStepRoom) return;
        lockStepRoom.PlayerReportedHash(sender, hash);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct LockStepVerify
{
    public float GameTime;
    public int Hash;

    public LockStepVerify(int hash)
    {
        GameTime = Engine.CoroutineManagerGameTime.Time;
        Hash = hash;
    }

    public override string ToString()
    {
        return Hash.ToString();
    }
}
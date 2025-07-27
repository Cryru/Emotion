using Emotion.Game.Routines;
using Emotion.Network.Base;

#nullable enable

namespace Emotion.Network.ServerSide.Gameplay;

public abstract class ServerTimeSyncRoomGameplay : ServerRoomGameplay
{
    public Server Server { get; protected set; } = null!;

    public int Errors;

    public int CurrentGameTime = 0;
    public int GameTimeAdvancePerTick = 20;

    private Coroutine _gameTimeRoutine = Coroutine.CompletedRoutine;

    public override void BindToServer(Server server, ServerRoom room)
    {
        Server = server;
        base.BindToServer(server, room);
        StartGameTime(Server.CoroutineManager);
    }

    public override void OnMessageReceived(ServerUser sender, NetworkMessage msg)
    {
        Assert(msg.Valid);
        OnGameplayMessageReceived(sender, msg);
    }

    private void StartGameTime(CoroutineManager coroutineManager)
    {
        if (!_gameTimeRoutine.Finished) _gameTimeRoutine.RequestStop();
        CurrentGameTime = 0;
        OnGamelayTimeReset();
        _gameTimeRoutine = coroutineManager.StartCoroutine(TickRoutine());
    }

    private IEnumerator TickRoutine()
    {
        while (Room.Active && Room.ServerGameplay == this)
        {
            yield return GameTimeAdvancePerTick;

            OnGameplayTick(GameTimeAdvancePerTick);

            int nextTime = CurrentGameTime + GameTimeAdvancePerTick;
            Server.BroadcastAdvanceTimeMessage(Room, nextTime);

            CurrentGameTime += GameTimeAdvancePerTick;
        }
    }

    #region API

    protected virtual void OnGameplayMessageReceived(ServerUser sender, NetworkMessage msg)
    {

    }

    protected virtual void OnGamelayTimeReset()
    {

    }

    protected virtual void OnGameplayTick(float dt)
    {

    }

    #endregion
}
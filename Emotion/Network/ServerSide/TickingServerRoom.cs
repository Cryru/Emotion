#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Network.New.Base;

namespace Emotion.Network.ServerSide;

public class TickingServerRoom : ServerRoom
{
    public uint GameTime { get; protected set; } = 0;
    public uint GameTickSpeed = 50;

    private Coroutine _updateRoutine = Coroutine.CompletedRoutine;

    public TickingServerRoom(ServerBase server, ServerPlayer? host, uint roomId) : base(server, host, roomId)
    {
        _updateRoutine = Engine.CoroutineManager.StartCoroutine(GameplayTickRoutine());
    }

    public override void Dispose()
    {
        base.Dispose();
        _updateRoutine.RequestStop();
    }

    private IEnumerator GameplayTickRoutine()
    {
        while (!Disposed)
        {
            yield return GameTickSpeed;
            OnGameplayTick(GameTickSpeed);
            GameTime += GameTickSpeed;
            SendMessageToAll(NetworkMessageType.ServerTick, GameTime);
        }
    }

    protected virtual void OnGameplayTick(uint dt)
    {

    }
}
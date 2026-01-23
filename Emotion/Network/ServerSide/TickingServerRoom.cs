#nullable enable

using Emotion;
using Emotion.Core.Utility.Coroutines;

namespace Emotion.Network.ServerSide;

public class TickingServerRoom : ServerRoom
{
    public int GameTickSpeed = 20;

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
        }
    }

    protected virtual void OnGameplayTick(int dt)
    {

    }
}
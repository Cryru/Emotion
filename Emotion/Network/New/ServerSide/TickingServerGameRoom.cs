#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Network.New.ServerSide;

public class TickingServerGameRoom : ServerGameRoom
{
    public int GameTickSpeed = 20;

    public Coroutine UpdateRoutine { get; private set; } = Coroutine.CompletedRoutine;

    public override void Activate()
    {
        base.Activate();
        if (!Active) return;

        AssertNotNull(Server);
        UpdateRoutine = Server.CoroutineManager.StartCoroutine(RoutineUpdate());
    }

    public override void Deactivate()
    {
        base.Deactivate();
        UpdateRoutine.RequestStop();
    }

    private IEnumerator RoutineUpdate()
    {
        while (Active)
        {
            yield return GameTickSpeed;
            OnGameplayTick(GameTickSpeed);
        }
    }

    protected virtual void OnGameplayTick(int dt)
    {

    }
}
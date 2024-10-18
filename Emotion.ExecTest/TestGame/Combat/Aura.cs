#nullable enable

using Emotion.Game.Time.Routines;

namespace Emotion.ExecTest.TestGame.Combat;

public enum AuraType
{
    Invalid,
    Coroutine,
    Constant
}

public abstract class Aura
{
    public AuraType Type;

    public abstract string Id { get; }

    public abstract string Icon { get; }

    public Coroutine? Routine;

    public void OnAttach(Unit onUnit)
    {
        TestScene.SendHash($"add aura {Id} to {onUnit.ObjectId}");
        if (Type == AuraType.Coroutine)
            Routine = Engine.CoroutineManagerGameTime.StartCoroutine(RunAuraRoutine());
    }

    public void OnDetach(Unit onUnit)
    {
        TestScene.SendHash($"remove aura {Id} from {onUnit.ObjectId}");
        if (Type == AuraType.Coroutine)
            Routine?.RequestStop();
    }

    public bool IsFinished()
    {
        if (Type == AuraType.Coroutine)
        {
            if (Routine == null) return false;
            return Routine.Finished;
        }
        return false;
    }

    protected virtual IEnumerator RunAuraRoutine()
    {
        yield break;
    }
}

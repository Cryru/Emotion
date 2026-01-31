#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Network.Base;
using Emotion.Network.New.Base;

namespace Emotion.Network.ServerSide;

public class TickingServerRoom : ServerRoom
{
    public uint GameTime { get; private set; } = 0;
    public uint GameTimeTickInterval = 50;

    private Coroutine _updateRoutine = Coroutine.CompletedRoutine;

    public TickingServerRoom(ServerBase server, ServerPlayer? host, uint roomId) : base(server, host, roomId)
    {
        StartGameTimeRoutine();
    }

    protected virtual void StartGameTimeRoutine()
    {
        _updateRoutine.RequestStop();

        GameTime = 0;
        _updateRoutine = Engine.CoroutineManager.StartCoroutine(GameplayTickRoutine());
    }

    public override void Dispose()
    {
        base.Dispose();
        _updateRoutine.RequestStop();
    }

    private IEnumerator GameplayTickRoutine()
    {
        NetworkMessage serverTickMsg = NetworkAgentBase.CreateMessageWithoutData(NetworkMessageType.ServerTick);

        while (!Disposed)
        {
            yield return GameTimeTickInterval;
            OnGameplayTick(GameTimeTickInterval);
            GameTime += GameTimeTickInterval;

            // Add server tick msg
            serverTickMsg.GameTime = GameTime;
            foreach (ServerPlayer user in UsersInside)
            {
                Server.SendMessageToPlayerRaw(user, in serverTickMsg);
            }
        }
    }

    protected virtual void OnGameplayTick(uint dt)
    {

    }
}
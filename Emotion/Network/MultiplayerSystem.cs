#nullable enable

using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.ClientSide;
using Emotion.Network.ServerSide;

namespace Emotion.Network;

public class MultiplayerSystem
{
    public int PlayerId { get => Client?.PlayerId ?? 0; }

    public string MetricText
    {
        get
        {
            if (Client != null)
                return $"({Client.MetricText}) {Engine.CoroutineManagerGameTime.GameTimeAdvanceLimit - Engine.CoroutineManagerGameTime.Time}";
            return "Offline";
        }
    }

    public ClientBase? Client { get; private set; }

    public void ConnectToServer(ClientBase client)
    {
        Client = client;
        client.ConnectIfNotConnected();
    }

    public void SendMessageToServer<TData>(in TData data)
        where TData : unmanaged, INetworkMessageStruct
    {
        Client?.SendMessageToServer(data);
    }

    public void SendMessageToServer<TEnum, TData>(TEnum messageType, in TData data)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        Client?.SendMessageToServer(messageType, data);
    }

    public void SendMessageToServerWithoutData<TEnum>(TEnum messageType)
        where TEnum : unmanaged
    {
        Client?.SendMessageToServerWithoutData(messageType);
    }

    #region LockStep

    public void SendLockStepMessage<TData>(in TData data, NetworkFunc<ClientBase, uint, TData> offlineFunc)
           where TData : unmanaged, INetworkMessageStruct
    {
        if (Client != null)
        {
            SendMessageToServer(data);
            return;
        }
        Engine.CoroutineManagerGameTime.StartCoroutine(LockStepFuncOfflineExec(data, offlineFunc));
    }

    public void SendLockStepMessage<TEnum, TData>(TEnum messageType, in TData data, NetworkFunc<ClientBase, uint, TData> offlineFunc)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        if (Client != null)
        {
            SendMessageToServer(messageType, data);
            return;
        }
        Engine.CoroutineManagerGameTime.StartCoroutine(LockStepFuncOfflineExec(data, offlineFunc));
    }

    public void SendLockStepMessageWithoutData<TEnum>(TEnum messageType, NetworkFunc<ClientBase, uint> offlineFunc)
        where TEnum : unmanaged
    {
        if (Client != null)
        {
            SendMessageToServerWithoutData(messageType);
            return;
        }
        Engine.CoroutineManagerGameTime.StartCoroutine(LockStepFuncOfflineExecWithoutData(offlineFunc));
    }

    private static IEnumerator LockStepFuncOfflineExec<TData>(TData data, NetworkFunc<ClientBase, uint, TData> offlineFunc)
    {
        offlineFunc(null, 0, data);
        yield break;
    }

    private static IEnumerator LockStepFuncOfflineExecWithoutData(NetworkFunc<ClientBase, uint> offlineFunc)
    {
        offlineFunc(null, 0);
        yield break;
    }

    #endregion

    #region Events

    public event Action? OnServerTick;

    #endregion

    private ServerBase? _server;

    [Conditional("DEBUG")]
    public void DebugSelfHost(ServerBase server)
    {
        _server = server;
    }

    internal void ServerTickReceived()
    {
        OnServerTick?.Invoke();
    }

    public void Update()
    {
        if (Client == null)
        {
            ServerTickReceived();
        }
    }
}
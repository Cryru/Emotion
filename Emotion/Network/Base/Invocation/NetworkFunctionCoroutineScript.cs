using Emotion.Core.Utility.Coroutines;

namespace Emotion.Network.Base.Invocation;

public struct NetworkFunctionCoroutineScript<TMsg> : ICoroutineScript
{
    public NetworkFunc<TMsg> NetworkFunc;
    public TMsg MsgData;

    public NetworkFunctionCoroutineScript(NetworkFunc<TMsg> func, TMsg msgData)
    {
        NetworkFunc = func;
        MsgData = msgData;
    }

    public CoroutineScriptRunResult RunStep(Coroutine hostingRoutine)
    {
        NetworkFunc(MsgData);
        return CoroutineScriptRunResult.Finished;
    }
}

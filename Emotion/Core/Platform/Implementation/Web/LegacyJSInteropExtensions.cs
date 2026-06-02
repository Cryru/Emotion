#nullable enable

using Microsoft.JSInterop;

namespace Emotion.Core.Platform.Implementation.Web;

internal static class LegacyJSInteropExtensions
{
    public static TResult Invoke<TResult>(this IJSInProcessRuntime jsRuntime, string identifier)
    {
        return jsRuntime.Invoke<TResult>(identifier, Array.Empty<object?>());
    }

    public static TResult Invoke<T0, TResult>(this IJSInProcessRuntime jsRuntime, string identifier, T0 arg0)
    {
        return jsRuntime.Invoke<TResult>(identifier, arg0);
    }

    public static TResult Invoke<T0, T1, TResult>(this IJSInProcessRuntime jsRuntime, string identifier, T0 arg0, T1 arg1)
    {
        return jsRuntime.Invoke<TResult>(identifier, arg0, arg1);
    }

    public static TResult Invoke<T0, T1, T2, TResult>(this IJSInProcessRuntime jsRuntime, string identifier, T0 arg0, T1 arg1, T2 arg2)
    {
        return jsRuntime.Invoke<TResult>(identifier, arg0, arg1, arg2);
    }

    public static TResult Invoke<T0, T1, T2, T3, TResult>(this IJSInProcessRuntime jsRuntime, string identifier, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
    {
        return jsRuntime.Invoke<TResult>(identifier, arg0, arg1, arg2, arg3);
    }

    public static TResult Invoke<T0, T1, T2, T3, T4, TResult>(this IJSInProcessRuntime jsRuntime, string identifier, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        return jsRuntime.Invoke<TResult>(identifier, arg0, arg1, arg2, arg3, arg4);
    }

    public static TResult Invoke<T0, T1, T2, T3, T4, T5, TResult>(this IJSInProcessRuntime jsRuntime, string identifier, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        return jsRuntime.Invoke<TResult>(identifier, arg0, arg1, arg2, arg3, arg4, arg5);
    }
}

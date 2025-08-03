#nullable enable

#region Using

using System.Diagnostics.CodeAnalysis;
using Emotion.Platform.Implementation.Win32;

#endregion

namespace Emotion.Testing;

public static class Assert
{
    public class TestAssertException : Exception
    {
        public TestAssertException(string txt) : base(txt)
        {
        }
    }

    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void Equal(object? a, object? b)
    {
        if (!Helpers.AreObjectsEqual(a, b)) AssertFailed($"Assert equal failed. Left is {a} and right is {b}");
    }

    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void Equal(float a, float b)
    {
        if (a != b) AssertFailed($"Assert equal failed. Left is {a} and right is {b}");
    }

    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void Equal(int a, int b)
    {
        if (a != b) AssertFailed($"Assert equal failed. Left is {a} and right is {b}");
    }

    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void True(bool condition, string? text = null)
    {
        if (!condition) AssertFailed($"Assert failed {text}");
    }

    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void False(bool condition, string? text = null)
    {
        if (condition) AssertFailed($"Assert failed {text}");
    }

    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void NotNull([NotNull] object? obj, string? text = null)
    {
        if (obj == null) AssertFailed($"{text ?? "object"} was null");
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        obj = null!;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }

    private static HashSet<string>? _ignoredAsserts;

    [DebuggerHidden]
    private static void AssertFailed(string msg)
    {
#if AUTOBUILD
		throw new TestAssertException(msg);
#elif DEBUG
        _ignoredAsserts ??= new HashSet<string>();

        string stack = Environment.StackTrace;
        string assertText = msg + "\n" + stack;

        // Get assert id to be msg + function in which it occured in
        string assertId = msg;
        int idx = stack.IndexOf("at Emotion.Testing.Assert", StringComparison.OrdinalIgnoreCase);
        while (idx != -1)
        {
            int newIdx = stack.IndexOf("at Emotion.Testing.Assert", idx + 1, StringComparison.OrdinalIgnoreCase);
            if (newIdx == -1)
            {
                int nextNewLine = stack.IndexOf("\n", idx, StringComparison.OrdinalIgnoreCase);
                int oneAfterThat = nextNewLine != -1 ? stack.IndexOf("\n", nextNewLine + 1, StringComparison.OrdinalIgnoreCase) : -1;
                if (oneAfterThat != -1)
                    assertId += stack.Substring(nextNewLine, oneAfterThat - nextNewLine).Trim();
                else
                    assertId += stack;
                break;
            }

            idx = newIdx;
        }

        if (_ignoredAsserts.Contains(assertId)) return;

        var assertResponse = AssertMessageBoxResponse.Break;
        if (Engine.Host is Win32Platform winPlatform) assertResponse = winPlatform.OpenAssertMessageBox(assertText);

        switch (assertResponse)
        {
            case AssertMessageBoxResponse.IgnoreCurrent:
                return;
            case AssertMessageBoxResponse.IgnoreAll:
                _ignoredAsserts.Add(assertId);
                return;
            case AssertMessageBoxResponse.Break:
                Debugger.Break();
                return;
        }
#else
		Engine.Log.Warning(msg, "ASSERT_FAILED");
#endif
    }

    public static void ClearIgnoredAsserts()
    {
        _ignoredAsserts?.Clear();
    }
}

// Used by GlobalImports to provide the same api as System.Diagnostics in Debug.Assert
public static class AssertWrapper
{
    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void Assert(bool condition, string? text = null)
    {
        Testing.Assert.True(condition, text);
    }

    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void AssertEqual(object? objA, object? objB)
    {
        Testing.Assert.Equal(objA, objB);
    }


    [Conditional("DEBUG")]
    [Conditional("AUTOBUILD")]
    [DebuggerHidden]
    public static void AssertNotNull([NotNull] object? obj)
    {
        Testing.Assert.True(obj != null);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        obj = null!;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
    }
}

public enum AssertMessageBoxResponse
{
    Break,
    IgnoreCurrent,
    IgnoreAll
}
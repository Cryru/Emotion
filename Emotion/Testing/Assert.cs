#region Using

using Emotion.Platform.Implementation.Win32;

#endregion

#nullable enable

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
	public static void Equal(object a, object b)
	{
		if (!a.Equals(b)) AssertFailed($"Assert equal failed. Left is {a} and right is {b}");
	}

	[Conditional("DEBUG")]
	[Conditional("AUTOBUILD")]
	public static void Equal(float a, float b)
	{
		if (a != b) AssertFailed($"Assert equal failed. Left is {a} and right is {b}");
	}

	[Conditional("DEBUG")]
	[Conditional("AUTOBUILD")]
	public static void Equal(int a, int b)
	{
		if (a != b) AssertFailed($"Assert equal failed. Left is {a} and right is {b}");
	}

	[Conditional("DEBUG")]
	[Conditional("AUTOBUILD")]
	public static void True(bool condition, string? text = null)
	{
		if (!condition) AssertFailed($"Assert failed {text}");
	}

	[Conditional("DEBUG")]
	[Conditional("AUTOBUILD")]
	public static void False(bool condition, string? text = null)
	{
		if (condition) AssertFailed($"Assert failed {text}");
	}

	[Conditional("DEBUG")]
	[Conditional("AUTOBUILD")]
	public static void NotNull(object? obj, string? text = null)
	{
		if (obj == null) AssertFailed($"{text ?? "object"} was null");
	}

	private static HashSet<string>? _ignoredAsserts;

	private static void AssertFailed(string msg)
	{
#if AUTOBUILD
		throw new TestAssertException(msg);
#elif DEBUG
		_ignoredAsserts ??= new HashSet<string>();
		string assertId = msg + "\n" + Environment.StackTrace;
		if (_ignoredAsserts.Contains(assertId)) return;

		var assertResponse = AssertMessageBoxResponse.Break;
		if (Engine.Host is Win32Platform winPlatform) assertResponse = winPlatform.OpenAssertMessageBox(assertId);

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
	public static void Assert(bool condition, string? text = null)
	{
		Testing.Assert.True(condition, text);
	}
}

public enum AssertMessageBoxResponse
{
	Break,
	IgnoreCurrent,
	IgnoreAll
}
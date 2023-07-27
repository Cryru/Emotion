#nullable enable

namespace Emotion.Testing;

public static class TestingUtility
{
	/// <summary>
	/// Returns the name of the function a couple invocations back through the current stack.
	/// </summary>
	public static string? GetFunctionBackInStack(int timesBack)
	{
		timesBack += 1; // This one needs to be excluded.

		string stackTrace = Environment.StackTrace ?? "";
		int functionNameStart = stackTrace.IndexOf(" at ", 4, StringComparison.Ordinal);
		for (var i = 0; i < timesBack; i++)
		{
			if (functionNameStart != -1) functionNameStart = stackTrace.IndexOf(" at ", functionNameStart + 1, StringComparison.Ordinal);
		}

		int filePathStart = functionNameStart == -1 ? -1 : stackTrace.IndexOf(" in ", functionNameStart, StringComparison.Ordinal);

		if (filePathStart != -1 && functionNameStart != -1)
		{
			functionNameStart += 4; // " at "
			return stackTrace.Substring(functionNameStart, filePathStart - functionNameStart);
		}

		return null;
	}
}
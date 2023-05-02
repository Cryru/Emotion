#nullable enable

#region Using

using System.Diagnostics;

#endregion

namespace Emotion.Testing;

public static class Assert
{
	public class TestAssertException : Exception
	{
		public TestAssertException(string txt) : base($"Test Assert Failed: {txt}")
		{
		}
	}

	public static void Equal(object a, object b)
	{
		if (!a.Equals(b)) throw new TestAssertException($"Assert equal failed. Left is {a} and right is {b}");
	}

	public static void Equal(float a, float b)
	{
		if (a != b) throw new TestAssertException($"Assert equal failed. Left is {a} and right is {b}");
	}

	public static void Equal(int a, int b)
	{
		if (a != b) throw new TestAssertException($"Assert equal failed. Left is {a} and right is {b}");
	}

	public static void True(bool condition)
	{
		Debug.Assert(condition);

		if (!condition) throw new TestAssertException("Assert failed.");
	}

	public static void False(bool condition)
	{
		Debug.Assert(!condition);

		if (condition) throw new TestAssertException("Assert failed.");
	}

	public static void NotNull(object? obj)
	{
		Debug.Assert(obj != null);

		if (obj == null) throw new TestAssertException("Obj was null");
	}
}
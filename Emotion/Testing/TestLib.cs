namespace Emotion.Testing;

public static class TestLib
{
	public class TestAssertException : Exception
	{
		public TestAssertException(string txt) : base(txt)
		{

		}
	}

	public static void Assert(int got, int expected)
	{
		if (got != expected)
		{
			throw new TestAssertException($"Int didn't match, got {got} expected {expected}");
		}
	}

	public static void Assert(Vector2 got, Vector2 expected)
	{
		if (got != expected)
		{
			throw new TestAssertException($"Vector2 didn't match, got {got} expected {expected}");
		}
	}

	public static void Assert(float got, float expected)
	{
		if (got != expected)
		{
			throw new TestAssertException($"Float didn't match, got {got} expected {expected}");
		}
	}
}
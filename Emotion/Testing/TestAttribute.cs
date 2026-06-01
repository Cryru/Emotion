#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Testing;

public class TestAttribute : Attribute
{
    public TestAttribute([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        FilePath = filePath;
        LineNumber = lineNumber;
    }

    public string FilePath { get; }
    public int LineNumber { get; }
}

public class TestClassRunParallel : Attribute
{
}

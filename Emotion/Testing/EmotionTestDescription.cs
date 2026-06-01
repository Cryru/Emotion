using System.Reflection;

namespace Emotion.Testing;

public enum EmotionTestKind
{
    Class,
    Scene
}

public sealed class EmotionTestDescription
{
    public string Id { get; init; }
    public string DisplayName { get; init; }
    public Type DeclaringType { get; init; }
    public MethodInfo Method { get; init; }
    public EmotionTestKind Kind { get; init; }
    public string? FilePath { get; init; }
    public int LineNumber { get; init; }

    public EmotionTestDescription(MethodInfo method, EmotionTestKind kind)
    {
        Type declaringType = method.DeclaringType!;
        TestAttribute? testAttribute = method.GetCustomAttribute<TestAttribute>(true);

        Id = $"{kind}:{declaringType.FullName}.{method.Name}";
        DisplayName = method.Name;
        DeclaringType = declaringType;
        Method = method;
        Kind = kind;
        FilePath = testAttribute?.FilePath ?? "Unknown source file";
        LineNumber = testAttribute?.LineNumber ?? 0;
    }
}
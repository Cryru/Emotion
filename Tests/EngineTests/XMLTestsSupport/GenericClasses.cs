#nullable enable

using Emotion.Common.Serialization;

namespace Tests.EngineTests.XMLTestsSupport;

public partial class ClassWithNonPublicField
{
    [SerializeNonPublicGetSet]
    public string? Field { get; protected set; }

    public void SetFieldSecretFunction(string s)
    {
        Field = s;
    }
}
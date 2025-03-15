#nullable enable

using Emotion.Common.Serialization;

namespace Tests.EngineTests.SerializationTestsSupport;

public class TestClassWithPrimitiveMember
{
    public int Number;
}

public partial class TestClassWithHiddenPrimitiveMember
{
    [SerializeNonPublicGetSet]
    public int Number { get; private set; }

    public TestClassWithHiddenPrimitiveMember(int number)
    {
        Number = number;
    }

    protected TestClassWithHiddenPrimitiveMember()
    {
    }
}

public class TestClassWithNestedObjectMember
{
    public TestClassWithPrimitiveMember Member = new();
}

public class BaseClass
{

}

public class BaseClassExtended : BaseClass
{

}

public class BaseClassExtendedTwo : BaseClass
{

}

public class BaseClassExtendedExtended : BaseClassExtended
{

}
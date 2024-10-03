using Emotion.Serialization.Base;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Testing;
using System.Text;

namespace Tests.EngineTests;

public class TestClassWithPrimitiveMember
{
    public int Number;
}

[Test]
public class ReflectorTests
{
    [Test]
    public void BasicReflector_StringConvertPrimitive()
    {
        StringBuilder stringBuilder = new StringBuilder();

        ReflectorTypeHandlerBase<int> typedHandler = ReflectorEngine.GetTypeHandler<int>();
        Assert.True(typedHandler.CanGetOrParseValueAsString);

        {
            typedHandler.WriteValueAsString(stringBuilder, 10);
            Assert.Equal(stringBuilder.ToString(), "10");

            stringBuilder.Clear();

            typedHandler.WriteValueAsStringGeneric(stringBuilder, 66);
            Assert.Equal(stringBuilder.ToString(), "66");
        }

        {
            IGenericReflectorTypeHandler genericIntHandler = ReflectorEngine.GetTypeHandler(typeof(int));
            Assert.Equal(genericIntHandler, typedHandler);

            stringBuilder.Clear();

            genericIntHandler.WriteValueAsStringGeneric(stringBuilder, 33);
            Assert.Equal(stringBuilder.ToString(), "33");

            stringBuilder.Clear();

            genericIntHandler.WriteValueAsStringGeneric(stringBuilder, (object)11);
            Assert.Equal(stringBuilder.ToString(), "11");
        }
    }

    [Test]
    public void BasicReflector_ComplexType()
    {
        StringBuilder stringBuilder = new StringBuilder();

        IGenericReflectorTypeHandler genericHandler = ReflectorEngine.GetTypeHandler(typeof(TestClassWithPrimitiveMember));
        Assert.False(genericHandler.CanGetOrParseValueAsString);

        ReflectorTypeHandlerBase<TestClassWithPrimitiveMember> specificHandler = ReflectorEngine.GetTypeHandler<TestClassWithPrimitiveMember>();
        Assert.Equal(genericHandler, specificHandler);

        {
            var typedHandlerComplex = genericHandler as ComplexTypeHandler<TestClassWithPrimitiveMember>;
            ComplexTypeHandlerMember[] members = typedHandlerComplex.GetMembers();
            Assert.Equal(1, members.Length);
            Assert.Equal(members[0].Name, "Number");
            Assert.Equal(members[0].Type, typeof(int));

            ComplexTypeHandlerMember numberMemberHandler = typedHandlerComplex.GetMemberHandler("Number");
            Assert.Equal(numberMemberHandler, members[0]);
        }

        {
            var genericHandlerComplex = genericHandler as IGenericReflectorComplexTypeHandler;
            ComplexTypeHandlerMember[] members = genericHandlerComplex.GetMembers();
            Assert.Equal(1, members.Length);
            Assert.Equal(members[0].Name, "Number");
            Assert.Equal(members[0].Type, typeof(int));

            ComplexTypeHandlerMember numberMemberHandler = genericHandlerComplex.GetMemberHandler("Number");
            Assert.Equal(numberMemberHandler, members[0]);
        }
    }

    [Test]
    public void BasicReflectorSerialization_PrimitiveNumber()
    {
        string serialized = SerializationBase.Serialize(10);

        Assert.Equal(serialized, "10");
    }

    [Test]
    public void BasicReflectorSerialization_ComplexType()
    {
        string serialized = SerializationBase.Serialize(
            new TestClassWithPrimitiveMember()
            {
                Number = 10
            }
        );

        Assert.Equal(serialized, "TestClassWithPrimitiveMember\nNumber\n10\n");
    }
}

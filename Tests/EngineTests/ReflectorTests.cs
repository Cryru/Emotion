using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Testing;
using Emotion.Utility.OptimizedStringReadWrite;
using System;
using System.Linq;
using System.Text;
using Tests.EngineTests.SerializationTestsSupport;

namespace Tests.EngineTests;

[Test]
public class ReflectorTests
{
    [Test]
    public void BasicReflector_StringConvertPrimitive()
    {
        StringBuilder stringBuilder = new StringBuilder();
        ValueStringWriter writer = new ValueStringWriter(stringBuilder);

        ReflectorTypeHandlerBase<int> typedHandler = ReflectorEngine.GetTypeHandler<int>();

        {
            typedHandler.WriteAsCode(10, ref writer);
            Assert.Equal(stringBuilder.ToString(), "10");

            stringBuilder.Clear();

            typedHandler.WriteAsCode(66, ref writer);
            Assert.Equal(stringBuilder.ToString(), "66");
        }

        {
            IGenericReflectorTypeHandler genericIntHandler = ReflectorEngine.GetTypeHandler(typeof(int));
            Assert.Equal(genericIntHandler, typedHandler);

            stringBuilder.Clear();

            genericIntHandler.WriteAsCode(33, ref writer);
            Assert.Equal(stringBuilder.ToString(), "33");

            stringBuilder.Clear();

            genericIntHandler.WriteAsCode((object)11, ref writer);
            Assert.Equal(stringBuilder.ToString(), "11");
        }
    }

    [Test]
    public void BasicReflector_ComplexType()
    {
        StringBuilder stringBuilder = new StringBuilder();

        IGenericReflectorTypeHandler genericHandler = ReflectorEngine.GetTypeHandler(typeof(TestClassWithPrimitiveMember));
        ReflectorTypeHandlerBase<TestClassWithPrimitiveMember> specificHandler = ReflectorEngine.GetTypeHandler<TestClassWithPrimitiveMember>();
        Assert.Equal(genericHandler, specificHandler);

        {
            var typedHandlerComplex = genericHandler as ComplexTypeHandler<TestClassWithPrimitiveMember>;
            ComplexTypeHandlerMemberBase[] members = typedHandlerComplex.GetMembers();
            Assert.Equal(1, members.Length);
            Assert.Equal(members[0].Name, "Number");
            Assert.Equal(members[0].Type, typeof(int));

            ComplexTypeHandlerMemberBase numberMemberHandler = typedHandlerComplex.GetMemberByName("Number");
            Assert.Equal(numberMemberHandler, members[0]);
        }

        {
            var genericHandlerComplex = genericHandler as IGenericReflectorComplexTypeHandler;
            ComplexTypeHandlerMemberBase[] members = genericHandlerComplex.GetMembers();
            Assert.Equal(1, members.Length);
            Assert.Equal(members[0].Name, "Number");
            Assert.Equal(members[0].Type, typeof(int));

            ComplexTypeHandlerMemberBase numberMemberHandler = genericHandlerComplex.GetMemberByName("Number");
            Assert.Equal(numberMemberHandler, members[0]);
        }
    }

    [Test]
    public void BasicReflector_Inheritance()
    {
        Type[] typesDescended = ReflectorEngine.GetTypesDescendedFrom(typeof(BaseClass));
        Assert.Equal(typesDescended.Length, 3);
        Assert.True(typesDescended.Contains(typeof(BaseClassExtended)));
        Assert.True(typesDescended.Contains(typeof(BaseClassExtendedExtended)));
        Assert.True(typesDescended.Contains(typeof(BaseClassExtendedTwo)));

        typesDescended = ReflectorEngine.GetTypesDescendedFrom(typeof(int));
        Assert.Equal(typesDescended.Length, 0);
    }
}

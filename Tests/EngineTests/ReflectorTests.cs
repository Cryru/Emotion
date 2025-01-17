using Emotion.Serialization.Base;
using Emotion.Serialization.XML;
using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.XML;
using Emotion.Testing;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tests.EngineTests;

public class TestClassWithPrimitiveMember
{
    public int Number;
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

[Test]
public class ReflectorTests
{
    [Test]
    public void BasicReflector_StringConvertPrimitive()
    {
        StringBuilder stringBuilder = new StringBuilder();
        ValueStringWriter writer = new ValueStringWriter(stringBuilder);

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

    [Test]
    public void XMLReflectorSerialization_PrimitiveNumber()
    {
        string serialized = XMLSerialize.To(10);
        string oldSerialized = XMLFormat.To(10);

        Assert.Equal(serialized, oldSerialized);

        string serializedWithoutHeader = XMLSerialize.To(10, new XMLConfig() { UseXMLHeader = false });
        Assert.Equal(serializedWithoutHeader, oldSerialized.Substring(XMLSerialize.XMLHeader.Length));

        {
            Span<byte> utf8Text = stackalloc byte[64];
            int bytesWrittenUtf8 = XMLSerialize.To(10, new XMLConfig(), utf8Text);
            Assert.True(bytesWrittenUtf8 != -1); // Success

            string utf8String = Encoding.UTF8.GetString(utf8Text.Slice(0, bytesWrittenUtf8));
            Assert.Equal(utf8String, serialized);
        }

        {
            Span<char> utf16Text = stackalloc char[64];
            int bytesWrittenUtf16 = XMLSerialize.To(10, new XMLConfig(), utf16Text);
            Assert.True(bytesWrittenUtf16 != -1); // Success

            Span<byte> utf16TextAsByte = MemoryMarshal.Cast<char, byte>(utf16Text);
            string utf16String = Encoding.Unicode.GetString(utf16TextAsByte.Slice(0, bytesWrittenUtf16));
            Assert.Equal(utf16String, serialized);
        }
    }

    [Test]
    public void XMLReflectorSerialization_ComplexObjectNumber()
    {
        var obj = new TestClassWithPrimitiveMember()
        {
            Number = 10
        };
        string serialized = XMLSerialize.To(obj);
        string oldSerialized = XMLFormat.To(obj);

        Assert.Equal(serialized, oldSerialized);

        {
            Span<byte> utf8Text = stackalloc byte[128];
            int bytesWrittenUtf8 = XMLSerialize.To(obj, new XMLConfig(), utf8Text);
            Assert.True(bytesWrittenUtf8 != -1); // Success

            string utf8String = Encoding.UTF8.GetString(utf8Text.Slice(0, bytesWrittenUtf8));
            Assert.Equal(utf8String, serialized);
        }

        {
            Span<char> utf16Text = stackalloc char[128];
            int bytesWrittenUtf16 = XMLSerialize.To(obj, new XMLConfig(), utf16Text);
            Assert.True(bytesWrittenUtf16 != -1); // Success

            Span<byte> utf16TextAsByte = MemoryMarshal.Cast<char, byte>(utf16Text);
            string utf16String = Encoding.Unicode.GetString(utf16TextAsByte.Slice(0, bytesWrittenUtf16));
            Assert.Equal(utf16String, serialized);
        }
    }
}

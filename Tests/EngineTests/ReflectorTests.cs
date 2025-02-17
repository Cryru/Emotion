using Emotion.Common.Serialization;
using Emotion.Serialization.PoC;
using Emotion.Serialization.XML;
using Emotion.Standard.OptimizedStringReadWrite;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.XML;
using Emotion.Testing;
using Emotion.Utility;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Tests.EngineTests;

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

            ComplexTypeHandlerMember numberMemberHandler = typedHandlerComplex.GetMemberByName("Number");
            Assert.Equal(numberMemberHandler, members[0]);
        }

        {
            var genericHandlerComplex = genericHandler as IGenericReflectorComplexTypeHandler;
            ComplexTypeHandlerMember[] members = genericHandlerComplex.GetMembers();
            Assert.Equal(1, members.Length);
            Assert.Equal(members[0].Name, "Number");
            Assert.Equal(members[0].Type, typeof(int));

            ComplexTypeHandlerMember numberMemberHandler = genericHandlerComplex.GetMemberByName("Number");
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
        string serialized = PoCSerialization.Serialize(10);

        Assert.Equal(serialized, "10");
    }

    [Test]
    public void BasicReflectorSerialization_ComplexType()
    {
        string serialized = PoCSerialization.Serialize(
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
        string serialized = XMLSerializationVerifyAllTypes(10);
        string oldSerialized = XMLFormat.To(10);

        Assert.Equal(serialized, oldSerialized);

        string serializedWithoutHeader = XMLSerializationVerifyAllTypes(10, new XMLConfig() { UseXMLHeader = false });
        Assert.Equal(serializedWithoutHeader, oldSerialized.Substring(XMLSerialization.XMLHeader.Length + 1));
    }

    [Test]
    public void XMLReflectorSerialization_ComplexObjectNumber()
    {
        var obj = new TestClassWithPrimitiveMember()
        {
            Number = 10
        };
        string serialized = XMLSerializationVerifyAllTypes(obj);
        string oldSerialized = XMLFormat.To(obj);

        Assert.Equal(serialized, oldSerialized);
    }

    [Test]
    public void XMLReflectorSerialization_ComplexObjectWithNestedObjects()
    {
        var obj = new TestClassWithNestedObjectMember()
        {
            Member = new TestClassWithPrimitiveMember()
            {
                Number = 15
            }
        };
        string serialized = XMLSerializationVerifyAllTypes(obj);
        string oldSerialized = XMLFormat.To(obj);

        Assert.Equal(serialized, oldSerialized);

        obj = new TestClassWithNestedObjectMember()
        {
            Member = null
        };
        serialized = XMLSerializationVerifyAllTypes(obj);
        oldSerialized = XMLFormat.To(obj);

        Assert.Equal(serialized, oldSerialized);

        string serializedNonPretty = XMLSerialization.To(obj, new XMLConfig()
        {
            Pretty = false
        });

        Assert.Equal(serializedNonPretty, serialized.Replace("\n", "").Replace("  ", ""));
    }

    [Test]
    public void XMLReflectorDeserialization_PrimitiveNumber()
    {
        string serialized = XMLSerializationVerifyAllTypes(55);
        string oldSerialized = XMLFormat.To(55);

        Assert.Equal(serialized, oldSerialized);

        int oldDeserializedNum = XMLFormat.From<int>(oldSerialized);
        Assert.Equal(oldDeserializedNum, 55);

        int newDeserializedNum = XMLDeserializationVerifyAllTypes<int>(55);
        Assert.Equal(newDeserializedNum, 55);

        string serializedWithoutHeader = XMLSerialization.To(55, new XMLConfig() { UseXMLHeader = false });
        newDeserializedNum = XMLSerialization.From<int>(serializedWithoutHeader);
        Assert.Equal(newDeserializedNum, 55);

        string serializedWithoutNonPretty = XMLSerialization.To(55, new XMLConfig() { UseXMLHeader = false, Pretty = false });
        newDeserializedNum = XMLSerialization.From<int>(serializedWithoutNonPretty);
        Assert.Equal(newDeserializedNum, 55);
    }

    [Test]
    public void XMLReflectorSerialization_ComplexObjectWithHiddenMember()
    {
        var obj = new TestClassWithHiddenPrimitiveMember(87);
        string serialized = XMLSerializationVerifyAllTypes(obj);
        string oldSerialized = XMLFormat.To(obj);

        Assert.Equal(serialized, oldSerialized);
    }

    private static string XMLSerializationVerifyAllTypes<T>(T obj, XMLConfig? config = null)
    {
        if (config == null) config = new XMLConfig();

        string serialized = XMLSerialization.To(obj, config.Value);

        {
            Span<byte> utf8Text = stackalloc byte[256];
            int bytesWrittenUtf8 = XMLSerialization.To(obj, config.Value, utf8Text);
            Assert.True(bytesWrittenUtf8 != -1); // Success

            string utf8String = Encoding.UTF8.GetString(utf8Text.Slice(0, bytesWrittenUtf8));
            Assert.Equal(utf8String, serialized);
        }

        {
            Span<char> utf16Text = stackalloc char[256];
            int bytesWrittenUtf16 = XMLSerialization.To(obj, config.Value, utf16Text);
            Assert.True(bytesWrittenUtf16 != -1); // Success

            Span<byte> utf16TextAsByte = MemoryMarshal.Cast<char, byte>(utf16Text);
            string utf16String = Encoding.Unicode.GetString(utf16TextAsByte.Slice(0, bytesWrittenUtf16));
            Assert.Equal(utf16String, serialized);
        }

        return serialized;
    }

    private static T XMLDeserializationVerifyAllTypes<T>(T obj, XMLConfig? config = null)
    {
        if (config == null) config = new XMLConfig();

        string serialized = XMLSerialization.To(obj, config.Value);
        T deserialized = XMLSerialization.From<T>(serialized);

        {
            Span<byte> utf8Text = stackalloc byte[256];
            int bytesWrittenUtf8 = XMLSerialization.To(obj, config.Value, utf8Text);
            Assert.True(bytesWrittenUtf8 != -1); // Success

            string utf8String = Encoding.UTF8.GetString(utf8Text.Slice(0, bytesWrittenUtf8));
            Assert.Equal(utf8String, serialized);

            T deserializedUtf8 = XMLSerialization.From<T>(utf8Text);
            Assert.True(Helpers.AreObjectsEqual(deserialized, deserializedUtf8));
        }

        {
            Span<char> utf16Text = stackalloc char[256];
            int bytesWrittenUtf16 = XMLSerialization.To(obj, config.Value, utf16Text);
            Assert.True(bytesWrittenUtf16 != -1); // Success

            Span<byte> utf16TextAsByte = MemoryMarshal.Cast<char, byte>(utf16Text);
            string utf16String = Encoding.Unicode.GetString(utf16TextAsByte.Slice(0, bytesWrittenUtf16));
            Assert.Equal(utf16String, serialized);

            T deserializedUtf16 = XMLSerialization.From<T>(utf16Text);
            Assert.True(Helpers.AreObjectsEqual(deserialized, deserializedUtf16));
        }

        return deserialized;
    }
}

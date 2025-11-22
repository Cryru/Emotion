#nullable enable

using System.Runtime.InteropServices;
using System.Text;
using System;
using Tests.EngineTests.SerializationTestsSupport;
using Emotion.Testing;
using System.Text.Json;
using System.Collections;
using Emotion.Standard;
using Emotion.Standard.Parsers.XML;
using Emotion.Standard.Serialization.XML;
using Emotion.Standard.Parsers.GLTF;
using Emotion.Standard.Serialization.Json;
using Emotion.Core;
using Emotion.Core.Systems.IO;

namespace Tests.EngineTests;

[Test]
public class SerializationTests
{
    [Test]
    public IEnumerator JSONReflectorSerialization_Complex_UTF8()
    {
        OtherAsset asset = Engine.AssetLoader.ONE_Get<OtherAsset>("Serialized/example.gltf");
        yield return asset;
        Assert.True(asset.Loaded);

        ReadOnlyMemory<byte> testFile = asset.Content;
        var docEmotion = JSONSerialization.From<GLTFDocument>(testFile.Span);
        var docCsharp = JsonSerializer.Deserialize<SerializationTestsSupport.GLTF.GLTFDocument>(testFile.Span); // Reference

        var eBuffers = docEmotion.Buffers;
        var buffers = docCsharp.Buffers;
        for (int i = 0; i < buffers.Length; i++)
        {
            var a = buffers[i];
            var b = eBuffers[i];

            Assert.Equal(a.Uri, b.Uri);
            Assert.Equal(a.ByteLength, b.ByteLength);
        }

        var eBufferView = docEmotion.BufferViews;
        var bufferView = docCsharp.BufferViews;
        for (int i = 0; i < bufferView.Length; i++)
        {
            var a = bufferView[i];
            var b = eBufferView[i];

            Assert.Equal(a.Buffer, b.Buffer);
            Assert.Equal(a.ByteLength, b.ByteLength);
            Assert.Equal(a.ByteOffset, b.ByteOffset);
            Assert.Equal(a.ByteStride, b.ByteStride);
            Assert.Equal(a.Target, b.Target);
        }

        var eImages = docEmotion.Images;
        var images = docCsharp.Images;
        for (int i = 0; i < images.Length; i++)
        {
            var a = images[i];
            var b = eImages[i];

            Assert.Equal(a.Uri, b.Uri);
        }

        var eTextures = docEmotion.Textures;
        var textures = docCsharp.Textures;
        for (int i = 0; i < textures.Length; i++)
        {
            var a = textures[i];
            var b = eTextures[i];

            Assert.Equal(a.Source, b.Source);
        }

        var eAccessors = docEmotion.Accessors;
        var accessors = docCsharp.Accessors;
        for (int i = 0; i < accessors.Length; i++)
        {
            var a = accessors[i];
            var b = eAccessors[i];

            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.BufferView, b.BufferView);
            Assert.Equal(a.ComponentType, b.ComponentType);
            Assert.Equal(a.Count, b.Count);
            Assert.Equal(a.Type, b.Type);
            Assert.Equal(a.Normalized, b.Normalized);
            Assert.Equal(a.ByteOffset, b.ByteOffset);
        }

        var eAnimations = docEmotion.Animations;
        var animations = docCsharp.Animations;
        for (int i = 0; i < animations.Length; i++)
        {
            var a = animations[i];
            var b = eAnimations[i];

            Assert.Equal(a.Name, b.Name);

            var samplersA = a.Samplers;
            var samplersB = b.Samplers;
            for (int ii = 0; ii < samplersA.Length; ii++)
            {
                var samA = samplersA[ii];
                var samB = samplersB[ii];

                Assert.Equal(samA.Interpolation, samB.Interpolation);
                Assert.Equal(samA.Input, samB.Input);
                Assert.Equal(samA.Output, samB.Output);
            }

            var channelsA = a.Channels;
            var channelsB = b.Channels;
            for (int ii = 0; ii < samplersA.Length; ii++)
            {
                var chanA = channelsA[ii];
                var chanB = channelsB[ii];

                Assert.Equal(chanA.Sampler, chanB.Sampler);
                Assert.Equal(chanA.Target.Path, chanB.Target.Path);
                Assert.Equal(chanA.Target.Node, chanB.Target.Node);
            }
        }

        var eMeshes = docEmotion.Meshes;
        var meshes = docCsharp.Meshes;
        for (int i = 0; i < meshes.Length; i++)
        {
            var a = meshes[i];
            var b = eMeshes[i];

            Assert.Equal(a.Name, b.Name);

            var aPrimitives = a.Primitives;
            var bPrimitives = b.Primitives;

            for (int ii = 0; ii < aPrimitives.Length; ii++)
            {
                var primitiveA = aPrimitives[ii];
                var primitiveB = bPrimitives[ii];

                var attributesA = primitiveA.Attributes;
                var attributesB = primitiveB.Attributes;

                Assert.Equal(attributesA.Count, attributesB.Count);
                foreach (var keyVal in attributesA)
                {
                    Assert.Equal(keyVal.Value, attributesB[keyVal.Key]);
                }

                Assert.Equal(primitiveA.Indices, primitiveB.Indices);
                Assert.Equal(primitiveA.Material, primitiveB.Material);
            }
        }

        var eMaterials = docEmotion.Materials;
        var materials = docCsharp.Materials;
        for (int i = 0; i < materials.Length; i++)
        {
            var a = materials[i];
            var b = eMaterials[i];

            Assert.Equal(a.Name, b.Name);

            var emissiveA = a.EmissiveFactor;
            var emissiveB = b.EmissiveFactor;

            Assert.Equal(emissiveA.Length, emissiveB.Length);
            for (int ii = 0; ii < emissiveA.Length; ii++)
            {
                Assert.Equal(emissiveA[ii], emissiveB[ii]);
            }

            var pbrA = a.PBRMetallicRoughness;
            var pbrB = b.PbrMetallicRoughness;

            Assert.Equal(pbrA.MetallicFactor, pbrB.MetallicFactor);

            var textureA = pbrA.BaseColorTexture;
            var textureB = pbrB.BaseColorTexture;

            Assert.Equal(textureA.Index, textureB.Index);
            Assert.Equal(textureA.TexCoord, textureB.TexCoord);
        }

        var eSkins = docEmotion.Skins;
        var skins = docCsharp.Skins;
        for (int i = 0; i < skins.Length; i++)
        {
            var a = skins[i];
            var b = eSkins[i];

            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.InverseBindMatrices, b.InverseBindMatrices);

            var jointsA = a.Joints;
            var jointsB = b.Joints;

            Assert.Equal(jointsA.Length, jointsB.Length);
            for (int ii = 0; ii < jointsA.Length; ii++)
            {
                Assert.Equal(jointsA[ii], jointsB[ii]);
            }
        }

        var eNodes = docEmotion.Nodes;
        var nodes = docCsharp.Nodes;
        for (int i = 0; i < animations.Length; i++)
        {
            var a = nodes[i];
            var b = eNodes[i];

            Assert.Equal(a.Name, b.Name);

            var childrenA = a.Children ?? Array.Empty<int>();
            var childrenB = b.Children ?? Array.Empty<int>();
            for (int ii = 0; ii < childrenA.Length; ii++)
            {
                var samA = childrenA[ii];
                var samB = childrenB[ii];

                Assert.Equal(samA, samB);
            }
        }
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

    //[Test]
    //public void XMLReflectorSerialization_ComplexObjectWithHiddenMember()
    //{
    //    var obj = new TestClassWithHiddenPrimitiveMember(87);
    //    string serialized = XMLSerializationVerifyAllTypes(obj);
    //    string oldSerialized = XMLFormat.To(obj);

    //    Assert.Equal(serialized, oldSerialized);
    //}

    [Test]
    public void XMLReflectorSerialization_NotEnoughRoom()
    {
        var obj = new TestClassWithPrimitiveMember()
        {
            Number = 50_000
        };

        Span<byte> utf8Text = stackalloc byte[10];
        int charsWrittenUtf8 = XMLSerialization.To(obj, new XMLConfig(), utf8Text);
        Assert.True(charsWrittenUtf8 == 10);

        Span<char> utf16Text = stackalloc char[10];
        int charsWrittenUtf16 = XMLSerialization.To(obj, new XMLConfig(), utf16Text);
        Assert.True(charsWrittenUtf16 == 10);

        // Currently there is no way to tell if the writing had enough space.
    }

    private static string XMLSerializationVerifyAllTypes<T>(T obj, XMLConfig? config = null)
    {
        if (config == null) config = new XMLConfig();

        string serialized = XMLSerialization.To(obj, config.Value);

        {
            Span<byte> utf8Text = stackalloc byte[256];
            int charsWrittenUtf8 = XMLSerialization.To(obj, config.Value, utf8Text);
            Assert.True(charsWrittenUtf8 != 0); // Success

            string utf8String = Encoding.UTF8.GetString(utf8Text.Slice(0, charsWrittenUtf8));
            Assert.Equal(utf8String, serialized);
        }

        {
            Span<char> utf16Text = stackalloc char[256];
            int charsWrittenUtf16 = XMLSerialization.To(obj, config.Value, utf16Text);
            Assert.True(charsWrittenUtf16 != 0); // Success

            string utf16String = utf16Text.Slice(0, charsWrittenUtf16).ToString();
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
            int charsWrittenUtf8 = XMLSerialization.To(obj, config.Value, utf8Text);
            Assert.True(charsWrittenUtf8 != 0); // Success

            string utf8String = Encoding.UTF8.GetString(utf8Text.Slice(0, charsWrittenUtf8));
            Assert.Equal(utf8String, serialized);

            T deserializedUtf8 = XMLSerialization.From<T>(utf8Text);
            Assert.True(Helpers.AreObjectsEqual(deserialized, deserializedUtf8));
        }

        {
            Span<char> utf16Text = stackalloc char[256];
            int charsWrittenUtf16 = XMLSerialization.To(obj, config.Value, utf16Text);
            Assert.True(charsWrittenUtf16 != 0); // Success

            string utf16String = utf16Text.Slice(0, charsWrittenUtf16).ToString();
            Assert.Equal(utf16String, serialized);

            T deserializedUtf16 = XMLSerialization.From<T>(utf16Text);
            Assert.True(Helpers.AreObjectsEqual(deserialized, deserializedUtf16));
        }

        return deserialized;
    }
}

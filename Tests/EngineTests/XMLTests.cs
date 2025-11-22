#region Using

using Emotion.Core;
using Emotion.Core.Systems.IO;
using Emotion.Primitives;
using Emotion.Standard.Parsers.XML;
using Emotion.Standard.Serialization;
using Emotion.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Tests.EngineTests.XMLTestsSupport;

#endregion

#nullable enable

namespace Tests.EngineTests;

/// <summary>
/// Tests connected to XML serializing and deserializing.
/// Due to Emotion rolling its own parser we need to test it :P
/// </summary>
[Test]
[TestClassRunParallel]
public class XMLTests
{
    [Test]
    public void BasicValueType()
    {
        string v2 = ToXMLForTest(new Vector2(100, 100));
        var restored = XMLFormat.From<Vector2>(v2);
        Assert.Equal(restored.X, 100);
        Assert.Equal(restored.Y, 100);
    }

    public class StringContainer
    {
        public string? Test;
    }

    [Test]
    public void BasicString()
    {
        string str = ToXMLForTest(new StringContainer {Test = "Hello"});
        var restored = XMLFormat.From<StringContainer>(str);
        Assert.NotNull(restored);
        Assert.Equal(restored.Test, "Hello");
    }

    [Test]
    public void BasicStringNull()
    {
        string str = ToXMLForTest(new StringContainer {Test = null});
        var restored = XMLFormat.From<StringContainer>(str);
        Assert.NotNull(restored);
        Assert.True(restored.Test == null);
    }

    [Test]
    public void ComplexValueType()
    {
        string r = ToXMLForTest(new Rectangle(100, 100, 200, 200));
        var restored = XMLFormat.From<Rectangle>(r);
        Assert.Equal(restored.X, 100);
        Assert.Equal(restored.Y, 100);
        Assert.Equal(restored.Width, 200);
        Assert.Equal(restored.Height, 200);
    }

    [Test]
    public void ComplexType()
    {
        string p = ToXMLForTest(new Positional(100, 200, 300));
        var restored = XMLFormat.From<Positional>(p);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.Equal(restored.Y, 200);
        Assert.Equal(restored.Z, 300);
    }

    [Test]
    public void ComplexInheritedType()
    {
        string t = ToXMLForTest(new Transform(100, 200, 300, 400, 500));
        var restored = XMLFormat.From<Transform>(t);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.Equal(restored.Y, 200);
        Assert.Equal(restored.Z, 300);
        Assert.Equal(restored.Width, 400);
        Assert.Equal(restored.Height, 500);
    }

    public class TransformLink : Transform
    {
        public Transform? Left { get; set; }
        public Transform? Right { get; set; }

        public TransformLink()
        {
        }

        public TransformLink(float x, float y, float z, float w, float h) : base(x, y, z, w, h)
        {
        }
    }

    [Test]
    public void ComplexTypeRecursiveType()
    {
        string tl = ToXMLForTest(new TransformLink(100, 200, 300, 400, 500)
        {
            Left = new Transform(600, 700, 800, 900, 1000)
        });
        var restored = XMLFormat.From<TransformLink>(tl);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.Equal(restored.Y, 200);
        Assert.Equal(restored.Z, 300);
        Assert.Equal(restored.Width, 400);
        Assert.Equal(restored.Height, 500);

        Assert.NotNull(restored.Left);
        Assert.Equal(restored.Left.X, 600);
        Assert.Equal(restored.Left.Y, 700);
        Assert.Equal(restored.Left.Z, 800);
        Assert.Equal(restored.Left.Width, 900);
        Assert.Equal(restored.Left.Height, 1000);

        Assert.True(restored.Right == null);
    }

    public class TransformInherited : Transform
    {
        public bool CoolStuff { get; set; }
    }

    [Test]
    public void ComplexTypeRecursiveTypeInherited()
    {
        string tld = ToXMLForTest(new TransformLink(100, 200, 300, 400, 500) {Left = new TransformInherited {CoolStuff = true, Height = 1100}});
        var restored = XMLFormat.From<TransformLink>(tld);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.Equal(restored.Y, 200);
        Assert.Equal(restored.Z, 300);
        Assert.Equal(restored.Width, 400);
        Assert.Equal(restored.Height, 500);

        Assert.NotNull(restored.Left);
        Assert.Equal(restored.Left.Height, 1100);
        Assert.True(((TransformInherited) restored.Left).CoolStuff);
    }

    public class TransformArrayHolder : Transform
    {
        public Transform[]? Children { get; set; }
    }

    [Test]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public void ComplexTypeRecursiveTypeArrayWithInherited()
    {
        string tlda = ToXMLForTest(new TransformArrayHolder
        {
            X = 100,
            Children = new[]
            {
                new Transform(1, 2, 3, 4, 5),
                new TransformInherited
                {
                    Width = 6,
                    CoolStuff = true
                }
            }
        });

        var restored = XMLFormat.From<TransformArrayHolder>(tlda);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.NotNull(restored.Children);
        Assert.Equal(restored.Children.Length, 2);

        {
            Transform child = restored.Children[0];
            Assert.NotNull(child);
            Assert.Equal(child.X, 1);
            Assert.Equal(child.Y, 2);
            Assert.Equal(child.Z, 3);
            Assert.Equal(child.Width, 4);
            Assert.Equal(child.Height, 5);
        }

        {
            Transform child = restored.Children[1];
            Assert.NotNull(child);
            Assert.Equal(child.Width, 6);
            Assert.True(((TransformInherited) child).CoolStuff);
        }
    }

    public class TransformListHolder : Transform
    {
        public List<Transform>? Children { get; set; }
    }

    [Test]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public void ComplexTypeRecursiveListWitInherited()
    {
        string tldl = ToXMLForTest(new TransformListHolder
        {
            X = 100,
            Children = new List<Transform>
            {
                new Transform(1, 2, 3, 4, 5),
                new TransformInherited
                {
                    Width = 6,
                    CoolStuff = true
                }
            }
        });

        var restored = XMLFormat.From<TransformListHolder>(tldl);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.NotNull(restored.Children);
        Assert.Equal(restored.Children.Count, 2);

        {
            Transform child = restored.Children[0];
            Assert.NotNull(child);
            Assert.Equal(child.X, 1);
            Assert.Equal(child.Y, 2);
            Assert.Equal(child.Z, 3);
            Assert.Equal(child.Width, 4);
            Assert.Equal(child.Height, 5);
        }

        {
            Transform child = restored.Children[1];
            Assert.NotNull(child);
            Assert.Equal(child.Width, 6);
            Assert.True(((TransformInherited) child).CoolStuff);
        }
    }

    public class TransformRecursiveRef : Transform
    {
        public TransformRecursiveRef? Other { get; set; }
    }

    /// <summary>
    /// When an object references itself.
    /// </summary>
    [Test]
    public void ComplexTypeRecursiveReferenceError()
    {
        var transformLink = new TransformRecursiveRef {X = 100};
        transformLink.Other = transformLink;
        string re = ToXMLForTest(transformLink);

        var restored = XMLFormat.From<TransformRecursiveRef>(re);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.True(restored.Other == null);
    }

    public class TransformRecursiveRefArray : Transform
    {
        public TransformRecursiveRefArray[]? Others { get; set; }
    }

    /// <summary>
    /// When an object references itself in an array.
    /// </summary>
    [Test]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public void ComplexTypeRecursiveReferenceArrayError()
    {
        var transformLink = new TransformRecursiveRefArray {X = 100};
        transformLink.Others = new[] {transformLink};
        string re = ToXMLForTest(transformLink);

        var restored = XMLFormat.From<TransformRecursiveRefArray>(re);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.NotNull(restored.Others);
        Assert.True(restored.Others.Length == 1);

        var transformLinkNull = new TransformRecursiveRefArray {X = 100};
        string reTwo = ToXMLForTest(transformLinkNull);
        restored = XMLFormat.From<TransformRecursiveRefArray>(reTwo);
        Assert.NotNull(restored);
        Assert.Equal(restored.X, 100);
        Assert.True(restored.Others == null);
    }

    public class ClassWithExcluded
    {
        [DontSerialize] public bool NotMe;

        public bool Me;
    }

    [Test]
    public void ComplexTypeExcludedProperties()
    {
        var classWithExcluded = new ClassWithExcluded {NotMe = true, Me = true};
        string ex = ToXMLForTest(classWithExcluded);

        var restored = XMLFormat.From<ClassWithExcluded>(ex);
        Assert.NotNull(restored);
        Assert.True(restored.Me);
        Assert.False(restored.NotMe);
    }

    [DontSerialize]
    public class ExcludedClass
    {
        public bool Hello;
    }

    public class ContainingExcludedClass
    {
        public ExcludedClass? NotMe;
        public bool JustMe;
    }

    [Test]
    public void ComplexTypeExcluded()
    {
        var excludedContainer = new ContainingExcludedClass
        {
            NotMe = new ExcludedClass {Hello = true},
            JustMe = true
        };

        string ex = ToXMLForTest(excludedContainer);
        var restored = XMLFormat.From<ContainingExcludedClass>(ex);
        Assert.NotNull(restored);
        Assert.True(restored.JustMe);
        Assert.True(restored.NotMe == null);

        string exTwo = ToXMLForTest(new ExcludedClass {Hello = true});
        var restoredTwo = XMLFormat.From<ExcludedClass>(exTwo);
        Assert.True(restoredTwo == null);
    }

    public enum TestEnum
    {
        This,
        Is,
        A,
        Test
    }

    public class EnumContainer
    {
        public TestEnum Hello;
    }

    [Test]
    public void EnumSerialize()
    {
        var enumContainer = new EnumContainer
        {
            Hello = TestEnum.Test
        };

        string enm = ToXMLForTest(enumContainer);
        var restored = XMLFormat.From<EnumContainer>(enm);
        Assert.NotNull(restored);
        Assert.Equal(restored.Hello, TestEnum.Test);
    }

    [Test]
    public void StringSanitizeSerialize()
    {
        string str = ToXMLForTest("Test test <<<:O<< Whaaa");
        var restored = XMLFormat.From<string>(str);
        Assert.Equal(restored, "Test test <<<:O<< Whaaa");
    }

    public class GenericTypeContainer<T>
    {
        public T? Stuff;
    }

    [Test]
    public void GenericSerialize()
    {
        var genericContainer = new GenericTypeContainer<Transform>
        {
            Stuff = new Transform(100, 200, 300, 400, 500)
        };

        string gen = ToXMLForTest(genericContainer);
        var restored = XMLFormat.From<GenericTypeContainer<Transform>>(gen);
        Assert.NotNull(restored);
        Assert.NotNull(restored.Stuff);
        Assert.Equal(restored.Stuff.X, 100);
        Assert.Equal(restored.Stuff.Y, 200);
        Assert.Equal(restored.Stuff.Z, 300);
        Assert.Equal(restored.Stuff.Width, 400);
        Assert.Equal(restored.Stuff.Height, 500);
    }

    [Test]
    public void GenericArraySerialize()
    {
        var genericContainers = new[]
        {
            new GenericTypeContainer<Transform>
            {
                Stuff = new Transform(100, 200, 300, 400, 500)
            }
        };

        string gen = ToXMLForTest(genericContainers);
        GenericTypeContainer<Transform>[]? restored = XMLFormat.From<GenericTypeContainer<Transform>[]>(gen);
        Assert.NotNull(restored);
        Assert.Equal(restored.Length, 1);

        var firstTime = restored[0];
        Assert.NotNull(firstTime);
        Assert.NotNull(firstTime.Stuff);
        Assert.Equal(firstTime.Stuff.X, 100);
        Assert.Equal(firstTime.Stuff.Y, 200);
        Assert.Equal(firstTime.Stuff.Z, 300);
        Assert.Equal(firstTime.Stuff.Width, 400);
        Assert.Equal(firstTime.Stuff.Height, 500);
    }

    public class GenericTypesContainer<T, T2, T3>
    {
        public T? Stuff;
        public T2? StuffTwo;
        public T3? StuffThree;
    }

    [Test]
    public void GenericsSerialize()
    {
        var generics = new GenericTypesContainer<Transform, Rectangle, string>
        {
            Stuff = new Transform(100, 200, 300, 400, 500),
            StuffTwo = new Rectangle(1, 2, 3, 4),
            StuffThree = "Dudeee"
        };

        string gen = ToXMLForTest(generics);
        var restored = XMLFormat.From<GenericTypesContainer<Transform, Rectangle, string>>(gen);
        Assert.NotNull(restored);
        Assert.NotNull(restored.Stuff);
        Assert.NotNull(restored.StuffTwo);
        Assert.NotNull(restored.StuffThree);

        Assert.Equal(restored.Stuff.X, 100);
        Assert.Equal(restored.Stuff.Y, 200);
        Assert.Equal(restored.Stuff.Z, 300);
        Assert.Equal(restored.Stuff.Width, 400);
        Assert.Equal(restored.Stuff.Height, 500);

        Assert.Equal(restored.StuffTwo.X, 1);
        Assert.Equal(restored.StuffTwo.Y, 2);
        Assert.Equal(restored.StuffTwo.Width, 3);
        Assert.Equal(restored.StuffTwo.Height, 4);

        Assert.Equal(restored.StuffThree, "Dudeee");
    }

    public struct ComplexNullableSubject
    {
        public int Number;
    }

    public class NullableComplexContainer
    {
        public ComplexNullableSubject? Stuff;
    }

    [Test]
    public void NullableComplex()
    {
        var nullableComplex = new NullableComplexContainer
        {
            Stuff = new ComplexNullableSubject
            {
                Number = 1
            }
        };

        string nul = ToXMLForTest(nullableComplex);
        var restored = XMLFormat.From<NullableComplexContainer>(nul);
        Assert.NotNull(restored);
        Assert.NotNull(restored.Stuff);
        Assert.Equal(restored.Stuff.Value.Number, 1);

        nullableComplex = new NullableComplexContainer
        {
            Stuff = new ComplexNullableSubject
            {
                Number = 0
            }
        };

        nul = ToXMLForTest(nullableComplex);
        restored = XMLFormat.From<NullableComplexContainer>(nul);
        Assert.NotNull(restored);
        Assert.NotNull(restored.Stuff);
        Assert.Equal(restored.Stuff.Value.Number, 0);

        nullableComplex = new NullableComplexContainer
        {
            Stuff = new ComplexNullableSubject()
        };

        nul = ToXMLForTest(nullableComplex);
        restored = XMLFormat.From<NullableComplexContainer>(nul);
        Assert.NotNull(restored);
        Assert.NotNull(restored.Stuff);
        Assert.Equal(restored.Stuff.Value.Number, 0);

        nullableComplex = new NullableComplexContainer
        {
            Stuff = null
        };

        nul = ToXMLForTest(nullableComplex);
        restored = XMLFormat.From<NullableComplexContainer>(nul);
        Assert.NotNull(restored);
        Assert.True(restored.Stuff == null);
    }

    public class NullableTrivialContainer
    {
        public int? Number;
    }

    [Test]
    public void NullableTrivial()
    {
        var nullableTrivial = new NullableTrivialContainer
        {
            Number = default(int) // This is intentionally the default value of int.
        };

        string nul = ToXMLForTest(nullableTrivial);
        var restored = XMLFormat.From<NullableTrivialContainer>(nul);
        Assert.NotNull(restored);
        Assert.Equal(restored.Number, 0);

        nullableTrivial = new NullableTrivialContainer
        {
            Number = 11 // This is intentionally the default value of int.
        };

        nul = ToXMLForTest(nullableTrivial);
        restored = XMLFormat.From<NullableTrivialContainer>(nul);
        Assert.NotNull(restored);
        Assert.Equal(restored.Number, 11);
    }

    [Test]
    public void NullableTrivialNull()
    {
        var nullableTrivial = new NullableTrivialContainer
        {
            Number = null
        };

        string nul = ToXMLForTest(nullableTrivial);
        var restored = XMLFormat.From<NullableTrivialContainer>(nul);
        Assert.NotNull(restored);
        Assert.True(restored.Number == null);
    }

    [Test]
    public void PrimitiveDictionary()
    {
        var primitiveDict = new Dictionary<string, int> {{"testOne", 1}, {"testTwo", 2}, {"", 4}, {" ", 0}};

        string xml = ToXMLForTest(primitiveDict);
        var restored = XMLFormat.From<Dictionary<string, int>>(xml);
        Assert.NotNull(restored);
        Assert.Equal(restored.Count, 4);
        Assert.Equal(restored["testOne"], 1);
        Assert.Equal(restored["testTwo"], 2);
        Assert.Equal(restored[""], 4);
        Assert.Equal(restored[" "], 0);
    }

    [Test]
    public void ComplexDictionary()
    {
        var complexDict = new Dictionary<TestEnum, Transform?> {{TestEnum.Test, new Transform(1, 2, 3, 4)}, {TestEnum.This, null}};

        string xml = ToXMLForTest(complexDict);
        var restored = XMLFormat.From<Dictionary<TestEnum, Transform>>(xml);
        Assert.NotNull(restored);
        Assert.Equal(restored.Count, 2);
        Assert.Equal(restored[TestEnum.Test].X, 1);
        Assert.Equal(restored[TestEnum.Test].Y, 2);
        Assert.Equal(restored[TestEnum.Test].Z, 3);
        Assert.Equal(restored[TestEnum.Test].Width, 4);
        Assert.True(restored[TestEnum.This] == null);
    }

    [Test]
    public void ArrayWithDefaultHoles()
    {
        var array = new[]
        {
            new Rectangle(1, 2, 3, 4),
            new Rectangle(),
            new Rectangle(5, 6, 7, 8)
        };

        string xml = ToXMLForTest(array);
        Rectangle[]? restored = XMLFormat.From<Rectangle[]>(xml);
        Assert.NotNull(restored);
        Assert.Equal(restored.Length, 3);
        Assert.Equal(restored[0].Width, 3);
        Assert.Equal(restored[1].Width, 0);
        Assert.Equal(restored[2].Width, 7);
    }

    [Test]
    [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    public void NonOpaqueArrayWithDefaultHoles()
    {
        var array = new Rectangle?[]
        {
            new Rectangle(1, 2, 3, 4),
            new Rectangle(),
            null,
            new Rectangle(5, 6, 7, 8),
            null
        };

        string xml = ToXMLForTest(array);
        Rectangle?[]? restored = XMLFormat.From<Rectangle?[]>(xml);
        Assert.NotNull(restored);
        Assert.Equal(restored.Length, 5);

        var firstItem = restored[0];
        Assert.NotNull(firstItem);
        Assert.Equal(firstItem.Value.Width, 3);

        var secondItem = restored[1];
        Assert.NotNull(secondItem);
        Assert.Equal(secondItem.Value.Width, 0);

        Assert.True(restored[2] == null);

        var fourthItem = restored[3];
        Assert.NotNull(fourthItem);
        Assert.Equal(fourthItem.Value.Width, 7);

        Assert.True(restored[4] == null);
    }

    public class CustomDefaultsComplex
    {
        public TestEnum Test = TestEnum.This;
        public int Number = 13;
        public bool Bool = true;
        public string Str = "Test";
        public Rectangle Rect = new Rectangle(1, 2, 3, 4);
        public Transform? Transform = new Transform(5, 6, 7, 8, 9);
        public float[]? Array = {10, 20.5f, 30};
        public Positional? Inherited = new Transform(10, 11, 12, 13);
        public double? Nullable = 10;
    }

    [Test]
    public void ComplexTypeWithCustomDefaults()
    {
        string xml = ToXMLForTest(new CustomDefaultsComplex
        {
            Test = TestEnum.A,
            Number = 0,
            Bool = false,
            Str = "",
            Rect = new Rectangle(),
            Transform = null,
            Array = null,
            Inherited = null,
            Nullable = null
        });
        var restored = XMLFormat.From<CustomDefaultsComplex>(xml);
        Assert.NotNull(restored);
        Assert.Equal(restored.Test, TestEnum.A);
        Assert.Equal(restored.Number, 0);
        Assert.Equal(restored.Bool, false);
        Assert.Equal(restored.Str, "");
        Assert.Equal(restored.Rect.X, 0);
        Assert.True(restored.Transform == null);
        Assert.True(restored.Array == null);
        Assert.True(restored.Inherited == null);
        Assert.True(restored.Nullable == null);
    }

    [DontSerializeMembers("StructInt")]
    public struct StructMemberWithExclusion
    {
        public int StructInt;
        public bool StructBool;
        public string StructString;
    }

    public class TypeWithExcludedMembersGrandparent
    {
        public int GrandparentNum;
        public string? ExcludedDeepField;
        public bool GrandparentBool;
    }

    [DontSerializeMembers("GrandparentBool")]
    public class TypeWithExcludedMembersDirectParent : TypeWithExcludedMembersGrandparent
    {
        public int ParentNum;
        public string? ExcludedInheritedField;
    }

    [DontSerializeMembers("ExcludedDirectField", "ExcludedInheritedField", "ExcludedDeepField")]
    public class TypeWithExcludedMembers : TypeWithExcludedMembersDirectParent
    {
        public int Num;
        public string? ExcludedDirectField;

        [DontSerializeMembers("GrandparentNum")]
        public TypeWithExcludedMembersDirectParent? NestedClassExclusion;

        [DontSerializeMembers("StructBool")] public StructMemberWithExclusion StructMember;
    }

    [Test]
    public void Exclusions()
    {
        string xml = ToXMLForTest(new TypeWithExcludedMembers
        {
            Num = 1,
            ParentNum = 2,
            GrandparentNum = 3,
            ExcludedDirectField = "Hi",
            ExcludedInheritedField = "Hii",
            ExcludedDeepField = "Hiii",
            NestedClassExclusion = new TypeWithExcludedMembersDirectParent
            {
                GrandparentNum = 99,
                ExcludedDeepField = "This one isn't excluded in this case",
                GrandparentBool = true
            },
            StructMember = new StructMemberWithExclusion
            {
                StructBool = true,
                StructInt = 99,
                StructString = "Only member not excluded"
            },
            GrandparentBool = true
        });
        var restored = XMLFormat.From<TypeWithExcludedMembers>(xml);
        Assert.NotNull(restored);

        // Non-excluded
        Assert.Equal(restored.Num, 1);
        Assert.Equal(restored.ParentNum, 2);
        Assert.Equal(restored.GrandparentNum, 3);
        Assert.Equal(restored.StructMember.StructString, "Only member not excluded");
        // Excluded by topmost class
        Assert.True(restored.ExcludedDirectField == null);
        Assert.True(restored.ExcludedInheritedField == null);
        Assert.True(restored.ExcludedDeepField == null);
        Assert.False(restored.StructMember.StructBool);
        // Excluded by StructMemberClass
        Assert.Equal(restored.StructMember.StructInt, 0);
        // Excluded by DirectParent
        Assert.False(restored.GrandparentBool);
        // Excluded by NestedClassExclusion field
        Assert.NotNull(restored.NestedClassExclusion);
        Assert.Equal(restored.NestedClassExclusion.GrandparentNum, 0);
        Assert.Equal(restored.NestedClassExclusion.ExcludedDeepField, "This one isn't excluded in this case");
        // Excluded by DirectParent class
        Assert.False(restored.NestedClassExclusion.GrandparentBool);
    }

    public class ClassWithExcludedComplexType
    {
        [DontSerialize]
        public ClassWithExcluded? A { get; set; }

        public ClassWithExcluded? B { get; set; }
    }

    [Test]
    public void DeserializeDontSerialize()
    {
        string document = "" +
                          "<TypeWithExcludedMembersDirectParent>\n" +
                          "<GrandparentBool>true</GrandparentBool>\n" +
                          "<ExcludedInheritedField>Hi</ExcludedInheritedField>\n" +
                          "</TypeWithExcludedMembersDirectParent>";
        var memberExcluded = XMLFormat.From<TypeWithExcludedMembersDirectParent>(document);
        Assert.NotNull(memberExcluded);
        Assert.False(memberExcluded.GrandparentBool); // Excluded member is not deserialized.
        Assert.Equal(memberExcluded.ExcludedInheritedField, "Hi"); // Following fields should be deserialized though.

        document = "<ClassWithExcluded>\n<NotMe>true</NotMe>\n<Me>true</Me>\n</ClassWithExcluded>";
        var memberDontSerialize = XMLFormat.From<ClassWithExcluded>(document);
        Assert.NotNull(memberDontSerialize);
        Assert.False(memberDontSerialize.NotMe); // DontSerialize is not deserialized.
        Assert.True(memberDontSerialize.Me);

        document = "<ClassWithExcludedComplexType>\n" +
                   "    <A>\n" +
                   "        <NotMe>true</NotMe>\n" +
                   "        <Me>true</Me>\n" +
                   "    </A>\n" +
                   "    <B>\n" +
                   "        <NotMe>true</NotMe>\n" +
                   "        <Me>true</Me>\n" +
                   "    </B>\n" +
                   "</ClassWithExcludedComplexType>";
        var complexExcludedDeserialize = XMLFormat.From<ClassWithExcludedComplexType>(document);
        Assert.NotNull(complexExcludedDeserialize);
        Assert.True(complexExcludedDeserialize.A == null); // Shouldn't have been deserialized.
        Assert.NotNull(complexExcludedDeserialize.B);
        Assert.True(complexExcludedDeserialize.B.Me);
        Assert.False(complexExcludedDeserialize.B.NotMe);

        document = "<ClassWithExcludedComplexType>\n" +
                   "    <A>\n" +
                   "        <NotMe>true</NotMe>\n" +
                   "        <Me>true</Me>\n" +
                   "    <B>\n" +
                   "        <NotMe>true</NotMe>\n" +
                   "        <Me>true</Me>\n" +
                   "    </B>\n" +
                   "</ClassWithExcludedComplexType>";
        var brokenDocument = XMLFormat.From<ClassWithExcludedComplexType>(document);
        Assert.NotNull(brokenDocument);
        Assert.True(brokenDocument.A == null);
        Assert.True(brokenDocument.B == null);
    }

    public class BaseClassWithVirtualProperty
    {
        public virtual string? Field
        {
            get { return null; }
            set { }
        }
    }

    public class OverrideClass : BaseClassWithVirtualProperty
    {
        public override string? Field { get; set; }
    }

    public class OverrideClassWithDontSerialize : BaseClassWithVirtualProperty
    {
        [DontSerialize]
        public override string? Field
        {
            get => base.Field;
            set => base.Field = value;
        }
    }

    [Test]
    public void InheritedFields()
    {
        var overridingClass = new OverrideClass
        {
            Field = "Hi"
        };
        string document = ToXMLForTest(overridingClass);
        var rgx = new Regex("Hi");
        Assert.Equal(rgx.Matches(document).Count, 1);

        var overridingClassWithExclusion = new OverrideClassWithDontSerialize
        {
            Field = "Hi"
        };
        document = ToXMLForTest(overridingClassWithExclusion);
        Assert.Equal(rgx.Matches(document).Count, 0);
    }

    [Test]
    public void ClassWithNonPublicGetSet()
    {
        var obj = new ClassWithNonPublicField();
        obj.SetFieldSecretFunction("Helloo");
        string document = ToXMLForTest(obj);
        var deserialized = XMLFormat.From<ClassWithNonPublicField>(document);
        Assert.NotNull(deserialized);
        Assert.Equal(deserialized.Field, "Helloo");
    }

    public class OverflowClass
    {
        public ClassContainingOverflow[]? FirstArr;
    }

    public class ClassContainingOverflow
    {
        public OverflowClass[]? SecondArr;
    }

    public class OverflowRootClass
    {
        public ClassContainingOverflow? A;
    }

    [Test]
    public void RecursiveArrayType() // This tests type handler creation recursion
    {
        ToXMLForTest(new OverflowRootClass());
    }

    [Test]
    public void DictionaryWithObjectKeysThatArePrimitiveTypes() // Editor object prefabs use this
    {
        var test = new List<Dictionary<string, object>>();
        var firstDict = new Dictionary<string, object>
        {
            {"enumType", TestEnum.Test},
            {"numberType", 1},
            {"booleanType", true},
            {
                "complexType", new StringContainer
                {
                    Test = "Member!"
                }
            },
            {"stringType", "this is text"}
        };
        test.Add(firstDict);

        string xml = ToXMLForTest(test);
        var deserialized = XMLFormat.From<List<Dictionary<string, object>>>(xml);
        Assert.NotNull(deserialized);

        Dictionary<string, object> deserializedFirstDist = deserialized[0];
        Assert.True((TestEnum) deserializedFirstDist["enumType"] == TestEnum.Test);
        Assert.True((int) deserializedFirstDist["numberType"] == 1);
        Assert.True((bool) deserializedFirstDist["booleanType"]);
        Assert.True(((StringContainer) deserializedFirstDist["complexType"]).Test == "Member!");
        Assert.True((string) deserializedFirstDist["stringType"] == "this is text");
    }

    private class TypeBase
    {
        public int Val;
    }

    private class TypeInherit : TypeBase
    {
        public int Val2;
    }

    [Test]
    public void LoadingDocumentSerializedAsOneTypeAsItsBaseType() // BaseEditor uses this to load maps
    {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var obj = new TypeInherit
        {
            Val = 100,
            Val2 = 120
        };
#pragma warning restore IDE0059 // Unnecessary assignment of a value

        // We don't serialize this obj here in order to prevent it from being cached by
        // the XML type handlers.
        const string xml = @"<?xml version=""1.0""?>
			<TypeInherit>
			  <Val2>120</Val2>
			  <Val>100</Val>
			</TypeInherit>
		";

        var restored = XMLFormat.From<TypeBase>(xml);
        Assert.NotNull(restored);
        Assert.True(restored.Val == 100);
        Assert.True(restored is TypeInherit);
        Assert.True(((TypeInherit) restored).Val2 == 120);
    }

    public struct StructTransformHolder
    {
        public Transform Child;
    }

    [Test]
    public void ComplexValueTypeWithInheritedMember()
    {
        string objectBase = ToXMLForTest(new StructTransformHolder
        {
            Child = new Transform()
            {
                Y = 52
            }
        });

        var restoredBase = XMLFormat.From<StructTransformHolder>(objectBase);
        Assert.NotNull(restoredBase.Child);
        Assert.Equal(restoredBase.Child.Y, 52);

        string arrayInherited = ToXMLForTest(new StructTransformHolder
        {
            Child = new TransformInherited()
            {
                Height = 15
            }
        });

        var restoredInherited = XMLFormat.From<StructTransformHolder>(arrayInherited);
        Assert.NotNull(restoredInherited.Child);
        Assert.Equal(restoredInherited.Child.Height, 15);
    }

    private HashSet<string> _usedNamed = new();

    private string ToXMLForTest<T>(T obj)
    {
        string? data = XMLFormat.To(obj);
        data ??= "<null/>";

        string fileName = TestingUtility.GetFunctionBackInStack(1) ?? new Guid().ToString();
        fileName = fileName.Replace("Tests.EngineTests.XMLTests.", "");
        fileName = MakeStringPathSafe(fileName);

        lock (_usedNamed)
        {
            var counter = 1;
            string originalName = fileName;
            while (_usedNamed.Contains(fileName)) fileName = originalName + "_" + counter++;
            _usedNamed.Add(fileName);
        }

        string xmlsFolder = Path.Join(TestExecutor.TestRunFolder, "XMLOutput");
        Directory.CreateDirectory(xmlsFolder);
        string thisFile = Path.Join(xmlsFolder, $"{fileName}.xml");
        File.WriteAllText(thisFile, data);

        var referenceAsset = Engine.AssetLoader.Get<TextAsset>($"ReferenceXMLOutput/{fileName}.xml");
        if (referenceAsset?.Content != null)
            Assert.True(referenceAsset.Content == data, $"Serialization {fileName} must produce same result");

        return data;
    }

    /// <summary>
    /// Converts the string to one which is safe for use in the file system.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A string safe to use in the file system.</returns>
    private static string MakeStringPathSafe(string str)
    {
        return Path.GetInvalidPathChars().Aggregate(str, (current, c) => current.Replace(c, ' '));
    }
}
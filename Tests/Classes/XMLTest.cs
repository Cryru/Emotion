#region Using

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Test;

#endregion

namespace Tests.Classes
{
    /// <summary>
    /// Test connected to xml serializing and deserializing.
    /// </summary>
    [Test("XML", true)]
    public class XMLTest
    {
        [Test]
        public void BasicValueType()
        {
            string v2 = XmlFormat.To(new Vector2(100, 100));
            var restored = XmlFormat.From<Vector2>(v2);
            Assert.Equal(restored.X, 100);
            Assert.Equal(restored.Y, 100);
        }

        public class StringContainer
        {
            public string Test;
        }

        [Test]
        public void BasicString()
        {
            string str = XmlFormat.To(new StringContainer {Test = "Hello"});
            var restored = XmlFormat.From<StringContainer>(str);
            Assert.Equal(restored.Test, "Hello");
        }

        [Test]
        public void ComplexValueType()
        {
            string r = XmlFormat.To(new Rectangle(100, 100, 200, 200));
            var restored = XmlFormat.From<Rectangle>(r);
            Assert.Equal(restored.X, 100);
            Assert.Equal(restored.Y, 100);
            Assert.Equal(restored.Width, 200);
            Assert.Equal(restored.Height, 200);
        }

        [Test]
        public void ComplexType()
        {
            string p = XmlFormat.To(new Positional(100, 200, 300));
            var restored = XmlFormat.From<Positional>(p);
            Assert.Equal(restored.X, 100);
            Assert.Equal(restored.Y, 200);
            Assert.Equal(restored.Z, 300);
        }

        [Test]
        public void ComplexInheritedType()
        {
            string t = XmlFormat.To(new Transform(100, 200, 300, 400, 500));
            var restored = XmlFormat.From<Transform>(t);
            Assert.Equal(restored.X, 100);
            Assert.Equal(restored.Y, 200);
            Assert.Equal(restored.Z, 300);
            Assert.Equal(restored.Width, 400);
            Assert.Equal(restored.Height, 500);
        }

        public class TransformLink : Transform
        {
            public Transform Left { get; set; }
            public Transform Right { get; set; }

            public TransformLink()
            {

            }

            public TransformLink(float x, float y, float z, float w, float h) : base(x, y, z, w, h)
            {
            }
        }

        [Test]
        public void ComplexTypeRecursive()
        {
            string tl = XmlFormat.To(new TransformLink(100, 200, 300, 400, 500)
            {
                Left = new Transform(600, 700, 800, 900, 1000)
            });
            var restored = XmlFormat.From<TransformLink>(tl);
            Assert.Equal(restored.X, 100);
            Assert.Equal(restored.Y, 200);
            Assert.Equal(restored.Z, 300);
            Assert.Equal(restored.Width, 400);
            Assert.Equal(restored.Height, 500);

            Assert.Equal(restored.Left.X, 600);
            Assert.Equal(restored.Left.Y, 700);
            Assert.Equal(restored.Left.Z, 800);
            Assert.Equal(restored.Left.Width, 900);
            Assert.Equal(restored.Left.Height, 1000);
        }

        public class TransformDerived : Transform
        {
            public bool CoolStuff { get; set; }
        }

        [Test]
        public void ComplexTypeRecursiveDerived()
        {
            string tld = XmlFormat.To(new TransformLink(100, 200, 300, 400, 500) {Left = new TransformDerived {CoolStuff = true, Height = 1100}});
            var restored = XmlFormat.From<TransformLink>(tld);
            Assert.Equal(restored.X, 100);
            Assert.Equal(restored.Y, 200);
            Assert.Equal(restored.Z, 300);
            Assert.Equal(restored.Width, 400);
            Assert.Equal(restored.Height, 500);

            Assert.Equal(restored.Left.Height, 1100);
            Assert.True(((TransformDerived) restored.Left).CoolStuff);
        }

        public class TransformArrayHolder : Transform
        {
            public Transform[] Children { get; set; }
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void ComplexTypeRecursiveArrayWithDerived()
        {
            string tlda = XmlFormat.To(new TransformArrayHolder
            {
                X = 100,
                Children = new[]
                {
                    new Transform(1, 2, 3, 4, 5),
                    new TransformDerived
                    {
                        Width = 6,
                        CoolStuff = true
                    }
                }
            });

            var restored = XmlFormat.From<TransformArrayHolder>(tlda);
            Assert.Equal(restored.X, 100);
            Assert.True(restored.Children != null);
            Assert.Equal(restored.Children.Length, 2);

            {
                Transform child = restored.Children[0];
                Assert.True(child != null);
                Assert.Equal(child.X, 1);
                Assert.Equal(child.Y, 2);
                Assert.Equal(child.Z, 3);
                Assert.Equal(child.Width, 4);
                Assert.Equal(child.Height, 5);
            }

            {
                Transform child = restored.Children[1];
                Assert.True(child != null);
                Assert.Equal(child.Width, 6);
                Assert.True(((TransformDerived) child).CoolStuff);
            }
        }

        public class TransformListHolder : Transform
        {
            public List<Transform> Children { get; set; }
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void ComplexTypeRecursiveListWithDerived()
        {
            string tldl = XmlFormat.To(new TransformListHolder
            {
                X = 100,
                Children = new List<Transform>
                {
                    new Transform(1, 2, 3, 4, 5),
                    new TransformDerived
                    {
                        Width = 6,
                        CoolStuff = true
                    }
                }
            });

            var restored = XmlFormat.From<TransformListHolder>(tldl);
            Assert.Equal(restored.X, 100);
            Assert.True(restored.Children != null);
            Assert.Equal(restored.Children.Count, 2);

            {
                Transform child = restored.Children[0];
                Assert.True(child != null);
                Assert.Equal(child.X, 1);
                Assert.Equal(child.Y, 2);
                Assert.Equal(child.Z, 3);
                Assert.Equal(child.Width, 4);
                Assert.Equal(child.Height, 5);
            }

            {
                Transform child = restored.Children[1];
                Assert.True(child != null);
                Assert.Equal(child.Width, 6);
                Assert.True(((TransformDerived) child).CoolStuff);
            }
        }

        public class TransformRecursiveRef : Transform
        {
            public TransformRecursiveRef Other { get; set; }
        }

        /// <summary>
        /// When an object references itself.
        /// </summary>
        [Test]
        public void ComplexTypeRecursiveReferenceError()
        {
            var transformLink = new TransformRecursiveRef {X = 100};
            transformLink.Other = transformLink;
            string re = XmlFormat.To(transformLink);

            var restored = XmlFormat.From<TransformRecursiveRef>(re);
            Assert.Equal(restored.X, 100);
            Assert.True(restored.Other == null);
        }

        public class TransformRecursiveRefArray : Transform
        {
            public TransformRecursiveRefArray[] Others { get; set; }
        }

        /// <summary>
        /// When an object references itself in an array.
        /// </summary>
        [Test]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void ComplexTypeRecursiveReferenceArrayError()
        {
            var transformLink = new TransformRecursiveRefArray {X = 100};
            transformLink.Others = new [] { transformLink };
            string re = XmlFormat.To(transformLink);

            var restored = XmlFormat.From<TransformRecursiveRefArray>(re);
            Assert.Equal(restored.X, 100);
            Assert.True(restored.Others != null);
            Assert.True(restored.Others.Length == 0);

            var transformLinkNull = new TransformRecursiveRefArray {X = 100};
            string reTwo = XmlFormat.To(transformLinkNull);
            restored = XmlFormat.From<TransformRecursiveRefArray>(reTwo);
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
            string ex = XmlFormat.To(classWithExcluded);

            var restored = XmlFormat.From<ClassWithExcluded>(ex);
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
            public ExcludedClass NotMe;
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

            string ex = XmlFormat.To(excludedContainer);
            var restored = XmlFormat.From<ContainingExcludedClass>(ex);
            Assert.True(restored.JustMe);
            Assert.True(restored.NotMe == null);

            string exTwo = XmlFormat.To(new ExcludedClass {Hello = true});
            var restoredTwo = XmlFormat.From<ExcludedClass>(exTwo);
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

            string enm = XmlFormat.To(enumContainer);
            var restored = XmlFormat.From<EnumContainer>(enm);
            Assert.Equal(restored.Hello, TestEnum.Test);
        }

        [Test]
        public void StringSanitizeSerialize()
        {
            string str = XmlFormat.To("Test test <<<:O<< Whaaa");
            var restored = XmlFormat.From<string>(str);
            Assert.Equal(restored, "Test test <<<:O<< Whaaa");
        }

        public class GenericTypeContainer<T>
        {
            public T Stuff;
        }

        [Test]
        public void GenericSerialize()
        {
            var genericContainer = new GenericTypeContainer<Transform>
            {
                Stuff = new Transform(100, 200, 300, 400, 500)
            };

            string gen = XmlFormat.To(genericContainer);
            var restored = XmlFormat.From<GenericTypeContainer<Transform>>(gen);
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

            string gen = XmlFormat.To(genericContainers);
            GenericTypeContainer<Transform>[] restored = XmlFormat.From<GenericTypeContainer<Transform>[]>(gen);
            Assert.Equal(restored.Length, 1);
            Assert.Equal(restored[0].Stuff.X, 100);
            Assert.Equal(restored[0].Stuff.Y, 200);
            Assert.Equal(restored[0].Stuff.Z, 300);
            Assert.Equal(restored[0].Stuff.Width, 400);
            Assert.Equal(restored[0].Stuff.Height, 500);
        }

        public class GenericTypesContainer<T, T2, T3>
        {
            public T Stuff;
            public T2 StuffTwo;
            public T3 StuffThree;
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

            string gen = XmlFormat.To(generics);
            var restored = XmlFormat.From<GenericTypesContainer<Transform, Rectangle, string>>(gen);
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

            string nul = XmlFormat.To(nullableComplex);
            var restored = XmlFormat.From<NullableComplexContainer>(nul);
            Assert.True(restored.Stuff != null);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.Equal(restored.Stuff.Value.Number, 1);

            nullableComplex = new NullableComplexContainer
            {
                Stuff = new ComplexNullableSubject()
                {
                    Number = 0
                }
            };

            nul = XmlFormat.To(nullableComplex);
            restored = XmlFormat.From<NullableComplexContainer>(nul);
            Assert.True(restored.Stuff != null);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.Equal(restored.Stuff.Value.Number, 0);

            nullableComplex = new NullableComplexContainer
            {
                Stuff = new ComplexNullableSubject()
            };

            nul = XmlFormat.To(nullableComplex);
            restored = XmlFormat.From<NullableComplexContainer>(nul);
            Assert.True(restored.Stuff != null);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.Equal(restored.Stuff.Value.Number, 0);

            nullableComplex = new NullableComplexContainer
            {
                Stuff = null
            };

            nul = XmlFormat.To(nullableComplex);
            restored = XmlFormat.From<NullableComplexContainer>(nul);
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

            string nul = XmlFormat.To(nullableTrivial);
            var restored = XmlFormat.From<NullableTrivialContainer>(nul);
            Assert.Equal(restored.Number, 0);

            nullableTrivial = new NullableTrivialContainer
            {
                Number = 11 // This is intentionally the default value of int.
            };

            nul = XmlFormat.To(nullableTrivial);
            restored = XmlFormat.From<NullableTrivialContainer>(nul);
            Assert.Equal(restored.Number, 11);
        }

        [Test]
        public void NullableTrivialNull()
        {
            var nullableTrivial = new NullableTrivialContainer
            {
                Number = null
            };

            string nul = XmlFormat.To(nullableTrivial);
            var restored = XmlFormat.From<NullableTrivialContainer>(nul);
            Assert.True(restored.Number == null);
        }

        [Test]
        public void PrimitiveDictionary()
        {
            var primitiveDict = new Dictionary<string, int> {{"testOne", 1}, {"testTwo", 2}, {"", 4}, {" ", 0}};

            string xml = XmlFormat.To(primitiveDict);
            var restored = XmlFormat.From<Dictionary<string, int>>(xml);
            Assert.True(restored != null);
            Assert.Equal(restored.Count, 4);
            Assert.Equal(restored["testOne"], 1);
            Assert.Equal(restored["testTwo"], 2);
            Assert.Equal(restored[""], 4);
            Assert.Equal(restored[" "], 0);
        }

        [Test]
        public void ComplexDictionary()
        {
            var complexDict = new Dictionary<TestEnum, Transform> {{TestEnum.Test, new Transform(1, 2, 3, 4)}, {TestEnum.This, null}};

            string xml = XmlFormat.To(complexDict);
            var restored = XmlFormat.From<Dictionary<TestEnum, Transform>>(xml);
            Assert.True(restored != null);
            Assert.Equal(restored.Count, 2);
            Assert.Equal(restored[TestEnum.Test].X, 1);
            Assert.Equal(restored[TestEnum.Test].Y, 2);
            Assert.Equal(restored[TestEnum.Test].Z, 3);
            Assert.Equal(restored[TestEnum.Test].Width, 4);
            Assert.True(restored[TestEnum.This] == null);
        }
    }
}
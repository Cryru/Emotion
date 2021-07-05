#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Test;
using Emotion.UI;
using Emotion.Utility;
using Tests.Results;
using UIDescriptionAsset = Emotion.IO.XMLAsset<(string, Emotion.Primitives.Rectangle)[]>;

#endregion

namespace Tests.Classes
{
    [Test("UITests", true)]
    public class UITests
    {
        [Test]
        public static void BasicMaxSize()
        {
            var ui = new UIController();
            CompareUI("BasicMaxSize.xml", ui);
        }

        [Test]
        public static void BasicAnchor()
        {
            var ui = new UIController();
            CompareUI("BasicAnchor.xml", ui);
        }

        [Test]
        public static void BasicText()
        {
            var ui = new UIController();
            CompareUI("BasicText.xml", ui);
        }

        [Test]
        public static void BasicTexture()
        {
            var ui = new UIController();
            CompareUI("BasicTexture.xml", ui);
        }

        [Test]
        public static void VerticalList()
        {
            var ui = new UIController();
            CompareUI("VerticalList.xml", ui);
        }

        [Test]
        public static void VerticalListStretch()
        {
            var ui = new UIController();
            CompareUI("VerticalListStretch.xml", ui);
        }

        [Test]
        public static void HorizontalList()
        {
            var ui = new UIController();
            CompareUI("HorizontalList.xml", ui);
        }

        [Test]
        public static void HorizontalListStretch()
        {
            var ui = new UIController();
            CompareUI("HorizontalListStretch.xml", ui);
        }

        [Test]
        public static void MarginAnchors()
        {
            var ui = new UIController();
            CompareUI("MarginAnchors.xml", ui);
        }

        [Test]
        public static void ListsAndOffsetsHorizontal()
        {
            var ui = new UIController();
            CompareUI("ListsAndOffsetsHorizontal.xml", ui);
        }

        [Test]
        public static void ListsAndOffsetsVertical()
        {
            var ui = new UIController();
            CompareUI("ListsAndOffsetsVertical.xml", ui);
        }

        public class DisplacementTestWindow : UISolidColor
        {
            public int Rotation { get; set; } = 45;
            public Vector3 Translation { get; set; }
            public Vector2 Scale { get; set; } = Vector2.One;

            protected override void AfterLayout()
            {
                float scale = GetScale();
                Vector2 center = Center / scale;

                TransformationStack.AddOrUpdate("displacement name here",
                    Matrix4x4.CreateScale(Scale.X , Scale.Y, 1, new Vector3(center, 0)) *
                    Matrix4x4.CreateRotationZ(Maths.DegreesToRadians(Rotation), new Vector3(center, 0)) *
                    Matrix4x4.CreateTranslation(Translation)
                );

                base.AfterLayout();
            }
        }

        [Test]
        public static void DisplacementTest()
        {
            var template = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>($"UITestTemplates/DisplacementTest.xml");
            Assert.True(template != null);
            var ui = new UIController();
            ui.AddChild(template.Content);
            UIController.PreloadUI().Wait();
            ui.Update();

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                ui.Render(composer);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.UIControllerDisplacementTest);
            }).WaitOne();

            Engine.Host.Size = new Vector2(Engine.Host.Size.X + 10, Engine.Host.Size.Y + 10);
            ui.InvalidateLayout();
            ui.Update();

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                ui.Render(composer);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.UIControllerDisplacementTestScaled);
            }).WaitOne();
        }

        private static void CompareUI(string file, UIController controller)
        {
            var template = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>($"UITestTemplates/{file}");
            Assert.True(template != null);

            controller.ClearChildren();
            controller.AddChild(template.Content);
            UIController.PreloadUI().Wait();
            controller.Update();

            (string, Rectangle)[] layoutDesc = controller.GetLayoutDescription();
            string xml = XMLFormat.To(layoutDesc);

            var expectedResultFile = Engine.AssetLoader.Get<UIDescriptionAsset>($"UITestMetrics/{file}");
            Assert.True(expectedResultFile != null);
            (string, Rectangle)[] expectedLayout = expectedResultFile!.Content;

            Assert.Equal(layoutDesc.Length, expectedLayout.Length);

            for (var i = 0; i < layoutDesc.Length; i++)
            {
                (string, Rectangle) wndDesc = layoutDesc[i];
                (string, Rectangle) expectedDesc = expectedLayout[i];
                bool idMatches = wndDesc.Item1 == expectedDesc.Item1;
                bool rectMatches = wndDesc.Item2 == expectedDesc.Item2;

                Assert.True(idMatches && rectMatches);
            }
        }
    }
}
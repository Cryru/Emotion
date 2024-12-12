#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
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
    public static class UITestsExtension
    {
        public static void TestUpdate(this UIController controller)
        {
            // Increments the current tick to force updating of mouse input.
            Engine.TickCount++;
            controller.Update();
        }
    }

    [Test("UITests", true)]
    public class UITests
    {
        [Test]
        public static void BasicMaxSize()
        {
            var ui = new UIController();
            CompareUI("BasicMaxSize.xml", ui);

            ui.ClearChildren();
            CompareUI("clamping.xml", ui);
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

            ui.ClearChildren();
            CompareUI("OutsideMargins.xml", ui);
        }

        [Test]
        public static void Lists()
        {
            var ui = new UIController();
            CompareUI("ListsAndOffsetsHorizontal.xml", ui);

            ui.ClearChildren();
            CompareUI("ListsAndOffsetsVertical.xml", ui);

            ui.ClearChildren();
            CompareUI("ListWithOutsideChildren.xml", ui);

            ui.ClearChildren();
            CompareUI("ListWithAnchoredChildren.xml", ui);

            ui.ClearChildren();
            CompareUI("WrappingList.xml", ui);

            ui.ClearChildren();
            CompareUI("RelativeChildrenInList.xml", ui);
        }

        public class DisplacementTestWindow : UISolidColor
        {
            public new int Rotation { get; set; } = 45; // This test is older than base.Rotation

            public Vector3 Translation { get; set; }

            public Vector2 Scale { get; set; } = Vector2.One;

            protected override void AfterLayout()
            {
                float scale = GetScale();
                Vector2 center = Center / scale;

                TransformationStack.AddOrUpdate("displacement name here",
                    Matrix4x4.CreateScale(Scale.X, Scale.Y, 1, new Vector3(center, 0)) *
                    Matrix4x4.CreateRotationZ(Maths.DegreesToRadians(Rotation), new Vector3(center, 0)) *
                    Matrix4x4.CreateTranslation(Translation)
                );

                base.AfterLayout();
            }
        }

        [Test]
        public static void DisplacementTest()
        {
            var template = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>("UITestTemplates/DisplacementTest.xml");
            Assert.True(template != null);
            var ui = new UIController();
            ui.AddChild(template.Content);
            ui.PreloadUI().Wait();
            ui.Update();

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                ui.Render(composer);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.UIControllerDisplacementTest);
            }).WaitOne();

            Engine.Host.Size = new Vector2(Engine.Host.Size.X + 10, Engine.Host.Size.Y + 10);
            DesktopTest.EventualConsistencyHostWait();
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

        [Test]
        public static void NineSliceTest()
        {
            var template = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>("UITestTemplates/NineSlice.xml");
            Assert.True(template != null);
            var ui = new UIController();
            ui.AddChild(template.Content);
            ui.PreloadUI().Wait();
            ui.Update();

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                ui.Render(composer);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.UIControllerNineSlice);
            }).WaitOne();
        }

        public class MouseTestWindow : UISolidColor
        {
            public int ClickedCount;
            private Vector2 _lastMousePos;

            public MouseTestWindow()
            {
                MaxSize = new Vector2(50, 50);
                HandleInput = true;
            }

            public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
            {
                if (key is > Key.MouseKeyStart and < Key.MouseKeyEnd && status == KeyState.Down) ClickedCount++;

                return base.OnKey(key, status, mousePos);
            }

            public override void OnMouseEnter(Vector2 mousePos)
            {
                WindowColor = Color.Red;
                base.OnMouseEnter(mousePos);
            }

            public override void OnMouseLeft(Vector2 mousePos)
            {
                WindowColor = Color.White;
                base.OnMouseLeft(mousePos);
            }

            public override void OnMouseMove(Vector2 mousePos)
            {
                _lastMousePos = mousePos;
                base.OnMouseMove(mousePos);
            }

            protected override bool RenderInternal(RenderComposer c)
            {
                base.RenderInternal(c);
                if (MouseInside) c.RenderSprite(new Rectangle(_lastMousePos.X, _lastMousePos.Y, 6, 6), Color.Pink);
                return true;
            }
        }

        [Test]
        public static void MouseTestTest()
        {
            var template = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>("UITestTemplates/MouseTest.xml");
            Assert.True(template != null);
            var ui = new UIController();
            ui.AddChild(template.Content);
            ui.PreloadUI().Wait();
            ui.TestUpdate();

            var winOne = (MouseTestWindow) ui.GetWindowById("WinOne");
            var winThree = (MouseTestWindow) ui.GetWindowById("WinThree");

            var updateMousePrivateMethod = Engine.Host.GetType().GetMethod("UpdateMousePosition",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Mouse outside.
            updateMousePrivateMethod.Invoke(Engine.Host, new object[] { new Vector2(0, 0) });
            ui.TestUpdate();
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Down);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Up);
            Assert.Equal(winOne.ClickedCount, 0);
            Assert.Equal(winOne.WindowColor, Color.White);

            // Mouse inside.
            updateMousePrivateMethod.Invoke(Engine.Host, new object[] { winOne.Position2 + new Vector2(10, 10) });
            ui.TestUpdate();
            Assert.Equal(winOne.WindowColor, Color.Red);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Down);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Up);
            Assert.Equal(winOne.ClickedCount, 1);

            updateMousePrivateMethod.Invoke(Engine.Host, new object[] { winThree.Position2 + new Vector2(10, 10) });
            ui.TestUpdate();
            Assert.Equal(winOne.WindowColor, Color.White);
            Assert.Equal(winThree.WindowColor, Color.Red);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Down);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Up);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Down);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Up);
            Assert.Equal(winOne.ClickedCount, 1);
            Assert.Equal(winThree.ClickedCount, 2);

            var templateOutOfParent = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>("UITestTemplates/OutOfParentMouseTest.xml");
            Assert.True(templateOutOfParent != null);
            ui.ClearChildren();
            ui.AddChild(templateOutOfParent.Content);
            ui.PreloadUI().Wait();
            ui.TestUpdate();

            var outOfParent = (MouseTestWindow) ui.GetWindowById("OutOfParent");
            var notChild = (MouseTestWindow) ui.GetWindowById("WithinParentButNotChild");

            // Try to click on the out of parent window.
            updateMousePrivateMethod.Invoke(Engine.Host, new object[] { outOfParent.Position2 + new Vector2(10, 10) });
            ui.TestUpdate();
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Down);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Up);
            Assert.Equal(outOfParent.ClickedCount, 1);
            Assert.Equal(outOfParent.WindowColor, Color.Red);

            // Try to click on the window that is within the parental bounds, but not a child.
            updateMousePrivateMethod.Invoke(Engine.Host, new object[] { notChild.Position2 + new Vector2(10, 10) });
            ui.TestUpdate();
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Down);
            Engine.Host.OnKey.Invoke(Key.MouseKeyLeft, KeyState.Up);
            Assert.Equal(notChild.ClickedCount, 1);
            Assert.Equal(notChild.WindowColor, Color.Red);
        }

        private static void CompareUI(string file, UIController controller)
        {
            var template = Engine.AssetLoader.Get<XMLAsset<UIBaseWindow>>($"UITestTemplates/{file}");
            Assert.True(template != null);

            controller.ClearChildren();
            controller.AddChild(template.Content);
            controller.PreloadUI().Wait();
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
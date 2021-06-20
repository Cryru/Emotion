#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Test;
using Emotion.UI;
using UIDescriptionAsset = Emotion.IO.XMLAsset<(string, Emotion.Primitives.Rectangle)[]>;

#endregion

namespace Tests.Classes
{
    [Test("UITests", true)]
    public class UITests
    {
        [Test]
        public static void RunUITests()
        {
            var ui = new UIController();

            BasicMaxSize(ui);
            ui.ClearChildren();

            BasicAnchorTests(ui);
            ui.ClearChildren();

            BasicLists(ui);
            ui.ClearChildren();

            BasicText(ui);
            ui.ClearChildren();

            BasicTexture(ui);
            ui.ClearChildren();
        }

        private static void CompareUI(string file, UIController controller)
        {
            var expectedResultFile = Engine.AssetLoader.Get<UIDescriptionAsset>($"UITestMetrics/{file}");
            Assert.True(expectedResultFile != null);
            (string, Rectangle)[] expectedResult = expectedResultFile!.Content;

            (string, Rectangle)[] desc = controller.GetLayoutDescription();
            if (desc.Length != expectedResult.Length) return;

            for (var i = 0; i < desc.Length; i++)
            {
                (string, Rectangle) wndDesc = desc[i];
                (string, Rectangle) expectedDesc = expectedResult[i];
                bool idMatches = wndDesc.Item1 == expectedDesc.Item1;
                bool rectMatches = wndDesc.Item2 == expectedDesc.Item2;

                Assert.True(idMatches && rectMatches);
            }
        }

        public static void BasicMaxSize(UIController controller)
        {
            controller.ClearChildren();
            controller.AddChild(new UIBaseWindow
            {
                Color = new Color(255, 0, 0),
                MaxSize = new Vector2(50, 50)
            });

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("BasicMaxSize.xml", controller);
        }

        public static void BasicAnchorTests(UIController controller)
        {
            controller.ClearChildren();
            Array uiAnchorTypes = Enum.GetValues(typeof(UIAnchor));
            var anchorIdx = 0;
            foreach (UIAnchor anchor in uiAnchorTypes)
            {
                var parentAnchorIdx = 0;
                foreach (UIAnchor parentAnchor in uiAnchorTypes)
                {
                    controller.AddChild(new UIBaseWindow
                    {
                        Id = $"{anchor} {parentAnchor}",
                        Anchor = anchor,
                        ParentAnchor = parentAnchor,
                        Color = new Color(50 + anchorIdx * 13 + parentAnchorIdx * 13, anchorIdx * 20, parentAnchorIdx * 20),
                        MaxSize = new Vector2(25, 25)
                    });
                    parentAnchorIdx++;
                }

                anchorIdx++;
            }

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("BasicAnchor.xml", controller);
        }

        public static void BasicLists(UIController controller)
        {
            controller.ClearChildren();
            controller.AddChild(new UIBaseWindow
            {
                Id = "List",
                Color = new Color(255, 0, 0),
                ListSpacing = new Vector2(5, 5),
                ParentAnchor = UIAnchor.CenterCenter,
                Anchor = UIAnchor.CenterCenter,
                LayoutMode = LayoutMode.HorizontalList
            });
            UIBaseWindow list = controller.GetWindowById("List");
            Debug.Assert(list != null);
            for (var i = 0; i < 10; i++)
            {
                list.AddChild(new UIBaseWindow
                {
                    MaxSize = new Vector2(20, 20),
                    Color = new Color(0, 100, 20 * i)
                });
            }

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("HorizontalList.xml", controller);

            list.StretchX = true;
            list.StretchY = true;
            list.InvalidateLayout();

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("HorizontalListStretch.xml", controller);

            list.LayoutMode = LayoutMode.VerticalList;
            list.InvalidateLayout();

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("VerticalListStretch.xml", controller);

            list.StretchX = false;
            list.StretchY = false;
            list.InvalidateLayout();

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("VerticalList.xml", controller);
        }

        public static void BasicText(UIController controller)
        {
            controller.ClearChildren();
            controller.AddChild(new UIText
            {
                Text = "Hello World!",
                FontFile = "Fonts/Junction-bold.otf",
                FontSize = 25,
                ParentAnchor = UIAnchor.CenterCenter,
                Anchor = UIAnchor.CenterCenter,
                TextShadow = Color.Red,
                Underline = true
            });

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("BasicText.xml", controller);
        }

        public static void BasicTexture(UIController controller)
        {
            controller.ClearChildren();
            controller.AddChild(new UITexture
            {
                TextureFile = "Images/logoAlpha.png",
                ParentAnchor = UIAnchor.CenterCenter,
                Anchor = UIAnchor.CenterCenter,
                RenderSize = new Vector2(50, 50),
            });

            controller.PreloadUI().Wait();
            controller.Update();
            CompareUI("BasicTexture.xml", controller);
        }
    }
}
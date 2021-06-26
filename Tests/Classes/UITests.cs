#region Using

using Emotion.Common;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.XML;
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
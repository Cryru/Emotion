#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Standard.XML;
using Emotion.UI;
using UIDescriptionAsset = Emotion.IO.XMLAsset<(string, Emotion.Primitives.Rectangle)[]>;

#endregion

namespace Emotion.ExecTest
{
    public class UITestScene : Scene
    {
        public override async Task LoadAsync()
        {
            UI.Background = new Color(32, 32, 32);
            UI.DrawDebugGrid = true;

            BasicMaxSize(UI);
            UI.ClearChildren();

            BasicAnchorTests(UI);
            UI.ClearChildren();

            UI.AddChild(new UIBaseWindow
            {
                Id = "List",
                Background = new Color(255, 0, 0),
               // MaxSize = new Vector2(50, 50),
               ListSpacing = new Vector2(5, 0),
                StretchX = true,
                StretchY = true,
                ParentAnchor = UIAnchor.CenterCenter,
                Anchor = UIAnchor.CenterCenter,
                LayoutMode = LayoutMode.HorizontalList
            });
            UIBaseWindow list = UI.GetWindowById("List");
            Debug.Assert(list != null);
            for (var i = 0; i < 10; i++)
            {
                list.AddChild(new UIBaseWindow
                {
                    MaxSize = new Vector2(20, 20),
                    Background = new Color(0, 100, 20 * i)
                });
            }

            await base.LoadAsync();

            (string, Rectangle)[] desc = UI.GetLayoutDescription();
            string txt = XMLFormat.To(desc);
            bool a = true;
        }

        private static void CompareUI(string file, UIController controller)
        {
            var expectedResultFile = Engine.AssetLoader.Get<UIDescriptionAsset>(file);
            Debug.Assert(expectedResultFile != null);
            (string, Rectangle)[] expectedResult = expectedResultFile.Content;

            (string, Rectangle)[] desc = controller.GetLayoutDescription();
            if (desc.Length != expectedResult.Length) return;

            for (var i = 0; i < desc.Length; i++)
            {
                (string, Rectangle) wndDesc = desc[i];
                (string, Rectangle) expectedDesc = expectedResult[i];
                bool idMatches = wndDesc.Item1 == expectedDesc.Item1;
                bool rectMatches = wndDesc.Item2 == expectedDesc.Item2;

                Debug.Assert(idMatches && rectMatches);
            }
        }

        public static void BasicMaxSize(UIController controller)
        {
            controller.ClearChildren();
            controller.AddChild(new UIBaseWindow
            {
                Background = new Color(255, 0, 0),
                MaxSize = new Vector2(50, 50)
            });

            controller.Update();
            controller.UILoadingThread.Wait();
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
                        Background = new Color(50 + anchorIdx * 13 + parentAnchorIdx * 13, anchorIdx * 20, parentAnchorIdx * 20),
                        MaxSize = new Vector2(25, 25)
                    });
                    parentAnchorIdx++;
                }

                anchorIdx++;
            }

            controller.Update();
            controller.UILoadingThread.Wait();
            CompareUI("BasicAnchor.xml", controller);
        }

        public static void BasicHorizontalList(UIController controller)
        {
            controller.ClearChildren();
            controller.AddChild(new UIBaseWindow
            {
                Id = "List",
                Background = new Color(255, 0, 0),
                ListSpacing = new Vector2(5, 0),
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
                    Background = new Color(0, 100, 20 * i)
                });
            }
            
            controller.Update();
            controller.UILoadingThread.Wait();
            CompareUI("HorizontalList.xml", controller);

            list.StretchX = true;
            list.StretchY = true;
            list.InvalidateLayout();

            controller.Update();
            controller.UILoadingThread.Wait();
            CompareUI("HorizontalListStretch.xml", controller);
        }
    }
}
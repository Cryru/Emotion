#region Using

using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
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
        public UIController UI = new UIController();

        public override async Task LoadAsync()
        {
            UI.Color = new Color(32, 32, 32);
            UI.DrawDebugGrid = true;

            UI.AddChild(new UITexture
            {
                TextureFile = "Images/logoAlpha.png",
                ParentAnchor = UIAnchor.CenterCenter,
                Anchor = UIAnchor.CenterCenter,
                RenderSize = new Vector2(50, 50),
            });

            (string, Rectangle)[] desc = UI.GetLayoutDescription();
            string txt = XMLFormat.To(desc);
            bool a = true;
        }

        public override void Update()
        {
            UI.Update();
        }

        public override void Draw(RenderComposer composer)
        {
            composer.SetUseViewMatrix(false);
            UI.Render(composer);
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
    }
}
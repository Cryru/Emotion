#region Using

using System.Numerics;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.IO;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows.Art
{
    public class FontPreview : ImGuiWindow
    {
        private FontAsset _font;
        private int _size = 10;
        private string _testText = "the quick brown fox jumped over the lazy dog! 1234567890.'/?\\\nThe quick brown fox jumped over the lazy dog! 1234567890.'/?\\\nTHE QUICK BROWN FOX JUMPED OVER THE LAZY DOG";
        private bool _emotionRenderer = false;

        public FontPreview() : base("Font Preview")
        {

        }

        public override void Update()
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            ImGui.InputInt("Font Size", ref _size);

            if (ImGui.Button("Select Font"))
            {
                var explorer = new FileExplorer<FontAsset>(f => { _font = f; });
                Parent.AddWindow(explorer);
            }

            if (_font == null) return;

            ImGui.Text($"Font: {_font.Font.FullName}");
            ImGui.Text($"Asset Name: {_font.Name}");
            if (ImGui.Checkbox("Emotion Renderer", ref _emotionRenderer))
            {
                DrawableFontAtlas.Rasterizer = _emotionRenderer ? GlyphRasterizer.Emotion : GlyphRasterizer.StbTrueType;
            }
            ImGui.InputTextMultiline("Test Text", ref _testText, 200, new Vector2(200, 100));

            composer.RenderString(new Vector3(0, 100, 0), Color.Black,
                _testText,
                _font.GetAtlas(_size));
        }
    }
}
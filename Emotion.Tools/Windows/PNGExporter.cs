#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Tiled;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet.Windowing;
using Emotion.Primitives;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.TMX.Layer;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class PngExporter : ImGuiWindow
    {
        private string _status = "";

        public PngExporter() : base("PNG Exporter")
        {
        }

        public override void Update()
        {

        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("Choose File"))
            {
                _status = "Waiting for file...";
                var explorer = new FileExplorer<OtherAsset>(LoadFile);
                Parent.AddWindow(explorer);
            }

            ImGui.Text(_status);
        }

        private void LoadFile(OtherAsset f)
        {
            _status = "Loading...";

            byte[] data = f.Content;
            if (!PngFormat.IsPng(data))
            {
                _status = $"The provided file {f.Name} is not a PNG file.";
                return;
            }

            byte[] pixels = PngFormat.Decode(data, out PngFileHeader header);
            byte[] output = PngFormat.Encode(pixels, header.Width, header.Height);

            bool saved = Engine.AssetLoader.Save(output, "Player" + "/" + f.Name, false);
            _status = saved ? "Done!" : "Error when saving the file. Check logs.";
        }
    }
}
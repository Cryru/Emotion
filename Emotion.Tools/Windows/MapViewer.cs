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
using Emotion.Standard.TMX.Layer;
using Emotion.Tools.Windows.HelpWindows;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class MapViewer : ImGuiWindow
    {
        private static TextAsset _file;
        private static TileMap<TransformRenderable> _map;
        private CameraBase _previousCamera;

        public MapViewer() : base("Map Viewer")
        {
            _map = new TileMap<TransformRenderable>("");
            _previousCamera = Engine.Renderer.Camera;
            Engine.Renderer.Camera = new PixelArtCamera(Vector3.Zero);
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("Choose File"))
            {
                var explorer = new FileExplorer<TextAsset>(LoadFile);
                Parent.AddWindow(explorer);
            }

            ImGui.Text($"Current File: {_file?.Name ?? "None"}");
            if (_file == null) return;
            if (ImGui.Button("Reload")) LoadFile(FileExplorer<TextAsset>.ExplorerLoadAsset(_file.Name));

            ImGui.Text("Tile Layers");
            for (var i = 0; i < _map.TiledMap.TileLayers.Count; i++)
            {
                TmxLayer curLayer = _map.TiledMap.TileLayers[i];
                ImGui.Text($"{curLayer.Name} {curLayer.Width}x{curLayer.Height}" + (curLayer.Visible ? "" : " Hidden"));
            }

            composer.SetUseViewMatrix(true);
            composer.Render(_map);
        }

        public override void Dispose()
        {
            Engine.Renderer.Camera = _previousCamera;
            base.Dispose();
        }

        private void LoadFile(TextAsset f)
        {
            _map.Reset(f);
            _map.Position = new Vector3(-_map.Size / 2, 0);
            _file = f;
        }

        public override void Update()
        {
            var speed = 0.5f;
            Vector2 dir = Vector2.Zero;
            if (Engine.Host.IsKeyHeld(Key.W)) dir.Y -= 1;
            if (Engine.Host.IsKeyHeld(Key.A)) dir.X -= 1;
            if (Engine.Host.IsKeyHeld(Key.S)) dir.Y += 1;
            if (Engine.Host.IsKeyHeld(Key.D)) dir.X += 1;
            if (Engine.Host.IsKeyHeld(Key.LeftControl)) speed *= 2;

            dir *= new Vector2(speed * Engine.DeltaTime, speed * Engine.DeltaTime);
            Engine.Renderer.Camera.Position += new Vector3(dir, 0);

            _map.Update(Engine.DeltaTime);
        }

        public void MoveCamera()
        {
        }
    }
}
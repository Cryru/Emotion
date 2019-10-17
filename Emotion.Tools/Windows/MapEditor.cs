#region Using

using System.IO;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Tiled;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class MapEditor : ImGuiWindow
    {
        private static OtherAsset _file;
        private static TileMap _map;

        public MapEditor() : base("Map Editor")
        {
            _map = new TileMap("", "");
        }

        protected override void RenderContent(RenderComposer composer)
        {
            if (ImGui.Button("Choose File"))
            {
                var explorer = new FileExplorer<OtherAsset>(LoadFile);
                Parent.AddWindow(explorer);
            }

            ImGui.Text($"Current File: {_file?.Name ?? "None"}");
            if (_file == null) return;
            if (ImGui.Button("Reload")) LoadFile(FileExplorer<OtherAsset>.ExplorerLoadAsset(_file.Name));
            ImGui.SameLine();

            composer.SetUseViewMatrix(true);
            composer.Render(_map);
        }

        private void LoadFile(OtherAsset f)
        {
            _map.Reset(f, Path.GetDirectoryName(f.Name), true);
            _map.Position = new Vector3(-_map.Size / 2, 0);
            _file = f;
        }

        public override void Update()
        {
            var speed = 0.5f;
            Vector2 dir = Vector2.Zero;
            if (Engine.InputManager.IsKeyHeld(Key.W)) dir.Y -= 1;
            if (Engine.InputManager.IsKeyHeld(Key.A)) dir.X -= 1;
            if (Engine.InputManager.IsKeyHeld(Key.S)) dir.Y += 1;
            if (Engine.InputManager.IsKeyHeld(Key.D)) dir.X += 1;
            if (Engine.InputManager.IsKeyHeld(Key.LeftControl)) speed *= 2;

            dir *= new Vector2(speed * Engine.DeltaTime, speed * Engine.DeltaTime);
            Engine.Renderer.Camera.Position += new Vector3(dir, 0);

            _map.Update(Engine.DeltaTime);
        }

        public void MoveCamera()
        {
        }
    }
}
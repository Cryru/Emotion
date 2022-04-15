#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Tiled;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Standard.TMX.Layer;
using Emotion.Tools.Windows.HelpWindows;
using Emotion.UI;
using Emotion.Utility;
using ImGuiNET;

#endregion

#nullable enable

namespace Emotion.Tools.Editors.MapEditor
{
    public class MapEditor : PresetGenericEditorWindow<TextAsset>
    {
        private const float TOP_BARS = 60;
        private TileMap? _map;

        private CameraBase _previousCamera = null!;

        public MapEditor() : base("Map Viewer")
        {

        }

        #region Editor Controls

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            _previousCamera = Engine.Renderer.Camera;
            Engine.Renderer.Camera = new FloatScaleCamera2d(Vector3.Zero);
        }

        public override void DetachedFromController(UIController controller)
        {
            base.DetachedFromController(controller);

            Engine.Renderer.Camera = _previousCamera;
        }

        protected override bool UpdateInternal()
        {
            var speed = 0.15f;
            Vector2 dir = Vector2.Zero;
            if (Engine.Host.IsKeyHeld(Key.W)) dir.Y -= 1;
            if (Engine.Host.IsKeyHeld(Key.A)) dir.X -= 1;
            if (Engine.Host.IsKeyHeld(Key.S)) dir.Y += 1;
            if (Engine.Host.IsKeyHeld(Key.D)) dir.X += 1;
            if (Engine.Host.IsKeyHeld(Key.LeftControl)) speed *= 2;

            float zoom = Engine.Renderer.Camera.Zoom;
            if (Engine.Host.GetMouseScrollRelative() < 0)
            {
                zoom += 0.3f;
            }
            else if(Engine.Host.GetMouseScrollRelative() > 0)
            {
                zoom -= 0.3f;
            }

            Engine.Renderer.Camera.Zoom = Maths.Clamp(zoom, 0.1f, 5f);

            dir *= new Vector2(speed * Engine.DeltaTime, speed * Engine.DeltaTime);
            Engine.Renderer.Camera.Position += new Vector3(dir, 0);

            _map?.Update(Engine.DeltaTime);

            return true;
        }

        #endregion

        #region IO

        protected override void MenuBarButtons()
        {
            if (ImGui.Button("Open .tmx"))
            {
                var explorer = new FileExplorer<TextAsset>((file) =>
                {
                    _map = new TileMap(file);
                    _selectedLayer = 0;
                });
                _toolsRoot.AddLegacyWindow(explorer);
            }
        }

        protected override bool OnFileLoaded(XMLAsset<TextAsset> file)
        {
           
            return true;
        }

        protected override bool OnFileSaving()
        {
            return true;
        }

        #endregion

        #region Layer UI

        private int _selectedLayer = 0;

        private void RenderLayerUI(RenderComposer c)
        {
            var layerBarSize = new Vector2(200, 300);
            ImGui.SetNextWindowPos(new Vector2(c.CurrentTarget.Size.X - layerBarSize.X, TOP_BARS), ImGuiCond.Always);
            ImGui.SetNextWindowSize(layerBarSize);
            ImGui.Begin("Layers", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);

            if (ImGui.BeginListBox("LayerList", new(layerBarSize.X, layerBarSize.Y - 55)))
            {
                if (_map != null)
                    for (var i = 0; i < _map.TiledMap!.TileLayers.Count; i++)
                    {
                        TmxLayer curLayer = _map.TiledMap.TileLayers[i];
                        ImGui.MenuItem($"{curLayer.Name} {curLayer.Width}x{curLayer.Height}" + (curLayer.Visible ? "" : " Hidden"), "", i == _selectedLayer);
                    }

                ImGui.EndListBox();
            }

            ImGui.SmallButton("New");
            ImGui.SameLine();
            ImGui.SmallButton("Delete");
            ImGui.End();
        }

        #endregion

        protected override bool RenderInternal(RenderComposer c)
        {
            var open = true;

            ImGui.SetNextWindowPos(new Vector2(0, 20), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(c.CurrentTarget.Size.X, 20));
            ImGui.Begin(Title, ref open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);

            RenderImGui();

            //ImGui.SetNextWindowPos(new Vector2(0, TOP_BARS), ImGuiCond.Always);
            //var sceneBarSize = new Vector2(100, c.CurrentTarget.Size.Y - TOP_BARS);
            //ImGui.SetNextWindowSizeConstraints(sceneBarSize, new Vector2(200, sceneBarSize.Y));
            //ImGui.Begin("Scene", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
            //ImGui.Text("Test");
            //ImGui.End();

            RenderLayerUI(c);

            ImGui.End();

            Position = Vector3.Zero;
            Size = c.CurrentTarget.Size;
            if (!open)
            {
                Parent?.RemoveChild(this);
                return false;
            }

            EditorHelpers.RenderToolGrid(c, Position, Size, new Color(32, 32, 32), 20);

            Vector2 posVec2 = Position.ToVec2();
            c.RenderLine(posVec2 + new Vector2(Size.X / 2, 0), posVec2 + new Vector2(Size.X / 2, Size.Y), Color.White * 0.7f);
            c.RenderLine(posVec2 + new Vector2(0, Size.Y / 2), posVec2 + new Vector2(Size.X, Size.Y / 2), Color.White * 0.7f);

            c.SetUseViewMatrix(true);
            _map?.Render(c);
            c.SetUseViewMatrix(false);

            return true;
        }
    }
}
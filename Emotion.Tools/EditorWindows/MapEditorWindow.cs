#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.World2D;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Tools.Editors;
using Emotion.Tools.EmUI;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Tools.EditorWindows
{
    public class MapEditorWindow : IMBasePresetEditor<Map2D>
    {
        private Type _customMapType;
        private CameraBase _previousCamera = null!;

        private bool _dragWorld;
        private Vector2 _dragOffset;

        // Tile data
        private Vector2 _selectedTile;

        public MapEditorWindow() : base("Map Editor")
        {
            List<Type> mapTypes = EditorHelpers.GetTypesWhichInherit<Map2D>();
            if (mapTypes.Count == 1) // Map2D itself.
            {
                _customMapType = mapTypes[0];
                return;
            }

            _customMapType = mapTypes[^1];
            Title = $"Map Editor - {_customMapType}";
            // todo: make a modal to pick
        }

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
            UIBaseWindow? focusedWindow = Controller?.InputFocus;
            if (focusedWindow == null || !focusedWindow.IsWithin(this)) return true;

            if (_dragWorld)
            {
                if (Engine.Host.IsKeyHeld(Key.MouseKeyMiddle))
                {
                    Vector2 pos = Engine.Host.MousePosition;
                    Vector3 move = (_dragOffset - pos).ToVec3() / Engine.Renderer.Camera.CalculatedScale;
                    Engine.Renderer.Camera.Position += move;
                    _dragOffset = pos;
                }
                else
                {
                    _dragWorld = false;
                }
            }
            else if (Engine.Host.IsKeyDown(Key.MouseKeyMiddle))
            {
                Vector2 pos = Engine.Host.MousePosition;
                _dragWorld = true;
                _dragOffset = pos;
            }

            // Selection
            if (Engine.Host.IsKeyDown(Key.MouseKeyLeft))
            {
                Vector2 worldPos = Engine.Host.MousePosition;
                worldPos = Engine.Renderer.Camera.ScreenToWorld(worldPos);

                Map2D? map = _currentAsset?.Content;
                if (map != null && map.TileData != null)
                {
                    Map2DTileMapData? tileData = map.TileData;
                    worldPos -= tileData.TileSize / 2;
                    _selectedTile = (worldPos / tileData.TileSize).RoundClosest();
                }
            }

            var speed = 0.15f;
            Vector2 dir = Vector2.Zero;
            if (Engine.Host.IsKeyHeld(Key.W)) dir.Y -= 1;
            if (Engine.Host.IsKeyHeld(Key.A)) dir.X -= 1;
            if (Engine.Host.IsKeyHeld(Key.S)) dir.Y += 1;
            if (Engine.Host.IsKeyHeld(Key.D)) dir.X += 1;
            if (Engine.Host.IsKeyHeld(Key.LeftControl)) speed *= 2;

            float zoom = Engine.Renderer.Camera.Zoom;
            if (Engine.Host.GetMouseScrollRelative() < 0)
                zoom += 0.2f;
            else if (Engine.Host.GetMouseScrollRelative() > 0) zoom -= 0.2f;

            Engine.Renderer.Camera.Zoom = Maths.Clamp(zoom, 0.1f, 5f);

            dir *= new Vector2(speed * Engine.DeltaTime, speed * Engine.DeltaTime);
            Engine.Renderer.Camera.Position += new Vector3(dir, 0);

            _currentAsset?.Content.Update(Engine.DeltaTime);

            return true;
        }

        protected override bool CanOpenFile(XMLAsset<Map2D> file)
        {
            return true;
        }

        protected override bool CanSaveFile(XMLAsset<Map2D> file)
        {
            return true;
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            //RenderLayerUI(c);

            Map2DTileMapData? tileData = _currentAsset?.Content?.TileData;
            //if (tileData != null && _selectedLayer != -1)
            //{
            //    Map2DTileMapLayer layer = tileData.Layers[_selectedLayer];
            //    var layerBarSize = new Vector2(200, 300);
            //    ImGui.SetNextWindowPos(new Vector2(c.CurrentTarget.Size.X - layerBarSize.X, TOP_BARS + 300), ImGuiCond.Always);
            //    ImGui.SetNextWindowSize(layerBarSize);
            //    ImGui.Begin("Selected Tile", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);

            //    int oneDCoord = tileData.GetTile1DFromTile2D(_selectedTile);
            //    ImGui.Text($"Selected: {_selectedTile} / {oneDCoord}");
            //    layer.GetTileData(oneDCoord, out uint tid, out bool _, out bool _, out bool _);
            //    ImGui.Text($"In Layer {layer.Name}:");
            //    ImGui.Text($"\tTid {tid}");

            //    VertexData[]? renderCache = tileData.GetMapLayerRenderCache(layer.Name);
            //    if (renderCache != null)
            //    {
            //        VertexData thisTile = renderCache[oneDCoord * 4];
            //        ImGui.Text($"\tZ {thisTile.Vertex.Z}");
            //    }

            //    ImGui.End();
            //}

            //ImGui.End();

            //Position = Vector3.Zero;
            //Size = c.CurrentTarget.Size;
            //if (!open)
            //{
            //    Parent?.RemoveChild(this);
            //    return false;
            //}

            c.SetUseViewMatrix(true);

            Vector2 mapSize = c.CurrentTarget.Size;
            EditorHelpers.RenderToolGrid(c, Position, mapSize, new Color(32, 32, 32), 20);

            Vector2 posVec2 = Position.ToVec2();
            c.RenderLine(posVec2 + new Vector2(mapSize.X / 2, 0), posVec2 + new Vector2(mapSize.X / 2, mapSize.Y), Color.White * 0.7f);
            c.RenderLine(posVec2 + new Vector2(0, mapSize.Y / 2), posVec2 + new Vector2(mapSize.X, mapSize.Y / 2), Color.White * 0.7f);

            _currentAsset?.Content?.Render(c);

            c.SetUseViewMatrix(true);

            if (tileData != null)
            {
                Vector2 tileSize = tileData.TileSize;
                c.RenderOutline((_selectedTile * tileSize).ToVec3(), tileSize, Color.White * 0.5f, 3);
            }

            c.SetUseViewMatrix(false);

            base.RenderInternal(c);

            return true;
        }
    }
}
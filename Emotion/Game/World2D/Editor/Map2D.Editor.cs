#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Emotion.Common;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public partial class Map2D
    {
        protected UIController? _editUI;
        protected CameraBase? _oldCamera;

        // Selection and MouseOver
        protected GameObject2D? _lastMouseOverObject;
        protected bool _objectSelect = true;
        protected List<GameObject2D>? _mouseOverOverlapping;
        protected int _mouseOverIndex = -1; // Index in ^

        // Tile selection
        protected bool _tileSelect = false;
        protected Vector2 _tileBrushLoc = Vector2.Zero;

        // UI stuff
        protected MapEditorTopBarButton? _dropDownOpen;

        private void SetupDebug()
        {
            if (!Engine.Configuration.DebugMode) return;
            Engine.Host.OnKey.AddListener(DebugInputHandler, KeyListenerType.Editor);
        }

        private bool DebugInputHandler(Key key, KeyStatus status)
        {
            if (key == Key.F3 && status == KeyStatus.Down)
            {
                if (EditorMode)
                    ExitEditor();
                else
                    EnterEditor();
            }

            if (_mouseOverOverlapping != null && key == Key.LeftAlt && status == KeyStatus.Down)
            {
                _mouseOverIndex++;
                if (_mouseOverIndex > _mouseOverOverlapping.Count - 1) _mouseOverIndex = 0;
            }

            if (EditorMode) return false;
            return true;
        }

        private void EnterEditor()
        {
            // Reset certain settings
            _objectSelect = true;

            _editUI = new UIController();
            _editUI.KeyPriority = KeyListenerType.EditorUI;

            UIBaseWindow? topBar = GetEditorTopBar();
            _editUI.AddChild(topBar);

            UIBaseWindow? worldInspect = GetWorldAttachInspectWindow();
            _editUI.AddChild(worldInspect);

            _oldCamera = Engine.Renderer.Camera;
            Engine.Renderer.Camera = new FloatScaleCamera2d(Vector3.Zero);
            Engine.Renderer.Camera.Position = _oldCamera.Position;

            EditorMode = true;
        }

        private void ExitEditor()
        {
            EditorMode = false;

            _editUI!.Dispose();
            _editUI = null;

            _lastMouseOverObject = null;

            Engine.Renderer.Camera = _oldCamera;
        }

        protected void UpdateDebug()
        {
            if (!EditorMode) return;

            var mouseInUi = _editUI.MouseFocus != null;

            if (_objectSelect && !mouseInUi)
            {
                Vector2 mouseScreen = Engine.Host.MousePosition;
                Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen);
                var circle = new Circle(mouseWorld, 1);
                var results = new List<GameObject2D>();
                foreach (int treeLayerId in _worldTree!.ForEachLayer())
                {
                    GetObjects(results, treeLayerId, circle, QueryFlags.Unique);
                }

                for (var i = 0; i < ObjectsToSerialize.Count; i++)
                {
                    GameObject2D obj = ObjectsToSerialize[i];
                    if (obj.ObjectState != ObjectState.Alive)
                    {
                        Rectangle bounds = obj.Bounds;
                        if (bounds.Size == Vector2.Zero) bounds.Size = new Vector2(32);
                        if (bounds.Contains(mouseWorld)) results.Add(obj);
                    }
                }

                SelectObjectMulti(results);
            }
            else
            {
                SelectObject(null);
            }

            _editUI!.Update();
            Helpers.CameraWASDUpdate();
        }

        protected void RenderDebug(RenderComposer c)
        {
            if (!EditorMode) return;
            RenderState? prevState = c.CurrentState.Clone();

            c.SetUseViewMatrix(true);

            if (TileData != null)
            {
                Rectangle clipRect = c.Camera.GetWorldBoundingRect();
                for (var i = 0; i < TileData.Layers.Count; i++)
                {
                    Map2DTileMapLayer layer = TileData.Layers[i];
                    if (!layer.Visible) TileData.RenderLayer(c, i, clipRect);
                }

                c.ClearDepth();
            }

            // Show selection of object, if any.
            if (_lastMouseOverObject != null)
            {
                Rectangle bound = GetObjectBoundForEditor(_lastMouseOverObject);
                c.RenderSprite(bound, Color.White * 0.3f);
            }

            // Draw some indication of unspawned serialized objects.
            for (var i = 0; i < ObjectsToSerialize.Count; i++)
            {
                GameObject2D obj = ObjectsToSerialize[i];
                if (obj.ObjectState != ObjectState.Alive)
                {
                    Rectangle bounds = GetObjectBoundForEditor(obj);
                    c.RenderSprite(bounds, Color.Magenta * 0.2f);
                    c.RenderOutline(bounds, Color.White * 0.4f);
                }
            }

            int count = GetObjectCount();
            for (var i = 0; i < count; i++)
            {
                GameObject2D obj = GetObjectByIndex(i);
                c.RenderOutline(obj.Bounds, Color.White * 0.4f);
            }

            c.SetUseViewMatrix(false);
            c.SetDepthTest(false);
            _editUI!.Render(c);

            c.SetState(prevState);
        }

        #region Helpers

        // this handles unspawned objects which might not have sizes.
        private Rectangle GetObjectBoundForEditor(GameObject2D obj)
        {
            Rectangle bounds = obj.Bounds;
            if (bounds.Size == Vector2.Zero) bounds.Size = new Vector2(32);
            return bounds;
        }

        private int ObjectSort(GameObject2D x, GameObject2D y)
        {
            return MathF.Sign(x.Position.Z - y.Position.Z);
        }

        protected void SelectObjectMulti(List<GameObject2D>? multiple)
        {
            if (multiple == null || multiple.Count == 0)
            {
                _mouseOverIndex = -1;
                _mouseOverOverlapping = null;
                SelectObject(null);
                return;
            }

            var forceUpdate = false;
            multiple.Sort(ObjectSort);
            if (_mouseOverOverlapping != null) // Update multi
            {
                GameObject2D current = _mouseOverOverlapping[_mouseOverIndex];
                int currentIdxInNew = multiple.IndexOf(current);
                bool sameCount = multiple.Count == _mouseOverOverlapping.Count;
                if (!sameCount)
                {
                    forceUpdate = true;
                }
                else if (currentIdxInNew == _mouseOverIndex)
                {
                    var allSame = true;
                    for (var i = 0; i < multiple.Count; i++)
                    {
                        GameObject2D obj = multiple[i];
                        GameObject2D other = _mouseOverOverlapping[i];
                        if (obj != other)
                        {
                            allSame = false;
                            break;
                        }
                    }

                    if (!allSame) forceUpdate = true;
                }

                _mouseOverIndex = currentIdxInNew != -1 ? currentIdxInNew : 0;
            }
            else
            {
                _mouseOverIndex = 0;
            }

            _mouseOverOverlapping = multiple;
            SelectObject(_mouseOverOverlapping[_mouseOverIndex], forceUpdate);
        }

        protected void SelectObject(GameObject2D? obj, bool forceUpdate = false)
        {
            if (_lastMouseOverObject == obj && !forceUpdate) return;

            _lastMouseOverObject = obj;
            var worldAttachUI = (UIWorldAttachedWindow?) _editUI?.GetWindowById("WorldAttach");

            // Deselecting
            if (obj == null)
            {
                if (worldAttachUI != null) worldAttachUI.Visible = false;
                return;
            }

            // Selecting
            if (worldAttachUI != null)
            {
                worldAttachUI.Visible = true;

                var text = (UIText?) worldAttachUI.GetWindowById("text");
                if (text != null)
                {
                    var txt = new StringBuilder();
                    txt.AppendLine($"Name: {obj.ObjectName ?? "null"}");
                    txt.AppendLine($"   Type: {obj.GetType().Name}");
                    txt.AppendLine($"   Pos: {obj.Position} Size: {obj.Size}");
                    txt.Append("   In Layers: ");

                    // Warning: These objects might not actually be in these layers.
                    // If they reported a different value to IsPartOfMapLayer when being added.
                    var idx = 0;
                    foreach (int treeLayerId in _worldTree!.ForEachLayer())
                    {
                        if (obj.IsPartOfMapLayer(treeLayerId))
                        {
                            if (idx != 0) txt.Append(", ");
                            txt.Append(treeLayerId);
                            idx++;
                        }
                    }

                    txt.Append("\n");

                    if (obj.ObjectState == ObjectState.Alive)
                        txt.AppendLine($"   Serialized: {obj.MapFlags.HasFlag(Map2DObjectFlags.Serializable)}");
                    else
                        txt.AppendLine($"   Spawn Condition: {obj.ShouldSpawnSerializedObject(this)}");

                    if (_mouseOverOverlapping != null && _mouseOverOverlapping.Count > 0) txt.AppendLine($"Objects Here: {_mouseOverIndex + 1}/{_mouseOverOverlapping.Count} [ALT] to switch");

                    text.Text = txt.ToString();
                }

                // Todo: auto UI keep on screen.
                Rectangle bounds = GetObjectBoundForEditor(obj);
                Vector2 attachPoint = bounds.TopRight;
                worldAttachUI.AttachToPosition(attachPoint.ToVec3());
            }
        }

        protected void EditorSaveMap()
        {
            string? fileName = FileName;
            if (fileName == null)
            {
                Engine.Log.Warning("Map is missing file name.", "Map2D");
                return;
            }

            for (var i = 0; i < ObjectsToSerialize.Count; i++)
            {
                GameObject2D obj = ObjectsToSerialize[i];
                obj.PreMapEditorSave();
            }

            // Unload the preset in the asset loader cache if loaded. This allows for changes to be observed on re-get.
            // This won't break anything as XMLAsset doesn't perform any cleanup.
            if (Engine.AssetLoader.Loaded(fileName)) Engine.AssetLoader.Destroy(fileName);

            XMLAsset<Map2D>? asset = XMLAsset<Map2D>.CreateFromContent(this, fileName);
            asset.Save();
            Reset();
        }

        #endregion
    }
}
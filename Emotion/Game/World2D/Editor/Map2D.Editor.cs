#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
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
        private UIController? _editUI;
        private CameraBase? _oldCamera;

        // Selection and MouseOver
        private GameObject2D? _lastMouseOverObject;
        private bool _objectSelect = true;
        private List<GameObject2D>? _mouseOverOverlapping;
        private int _mouseOverIndex = -1; // Index in ^
        private GameObject2D? _objectDragging;
        private Vector2 _objectDragOffset;
        private bool _ignoreNextObjectDragLetGo;
        private Vector2 _objectDragStartPos;

        // Tile selection
        private bool _tileSelect = false;
        private Vector2 _tileBrush = Vector2.Zero;

        // UI stuff
        private MapEditorTopBarButton? _dropDownOpen;
        private UIDropDown? _topBarDropDown;

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

            if (EditorMode)
            {
                if (_mouseOverOverlapping != null && key == Key.LeftAlt && status == KeyStatus.Down)
                {
                    _mouseOverIndex++;
                    if (_mouseOverIndex > _mouseOverOverlapping.Count - 1) _mouseOverIndex = 0;
                }

                if (key == Key.MouseKeyLeft && status == KeyStatus.Down && _lastMouseOverObject != null)
                {
                    _objectDragging = _lastMouseOverObject;
                    Vector2 mouseScreen = Engine.Host.MousePosition;
                    Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen);
                    _objectDragOffset = _objectDragging.Position2 - mouseWorld;
                    _objectDragStartPos = _objectDragging.Position2;
                }
                else if (key == Key.MouseKeyLeft && status == KeyStatus.Up && _objectDragging != null)
                {
                    if (Vector2.Distance(_objectDragging.Position2, _objectDragStartPos) < 1) OpenPropertiesPanelForObject(_objectDragging);

                    if (!_ignoreNextObjectDragLetGo) _objectDragging = null;
                    _ignoreNextObjectDragLetGo = false;
                }

                if (key == Key.Z && status == KeyStatus.Down && Engine.Host.IsCtrlModifierHeld()) EditorUndoLastAction();

                return false;
            }

            return true;
        }

        private void EnterEditor()
        {
            // Reset certain settings
            _objectSelect = true;

            _editUI = new UIController();
            _editUI.KeyPriority = KeyListenerType.EditorUI;

            UIBaseWindow topBar = GetEditorTopBar();
            _editUI.AddChild(topBar);

            UIBaseWindow worldInspect = GetWorldAttachInspectWindow();
            _editUI.AddChild(worldInspect);

            _oldCamera = Engine.Renderer.Camera;
            Engine.Renderer.Camera = new FloatScaleCamera2d(Vector3.Zero);
            Engine.Renderer.Camera.Position = _oldCamera.Position;

            foreach (GameObject2D obj in GetObjects())
            {
                EnsureObjectNameplate(obj);
            }

            foreach (GameObject2D obj in ObjectsToSerialize)
            {
                EnsureObjectNameplate(obj);
            }

            EditorMode = true;
        }

        private void ExitEditor()
        {
            EditorMode = false;

            _editUI!.Dispose();
            _editUI = null;

            _namePlates?.Clear();

            _lastMouseOverObject = null;

            Engine.Renderer.Camera = _oldCamera;
        }

        protected void UpdateDebug()
        {
            if (!EditorMode) return;

            bool mouseInUI = _editUI?.MouseFocus != null;
            var mouseFocusNameplate = _editUI?.MouseFocus as MapEditorObjectNameplate;
            bool mouseNotInUIOrInNameplate = !mouseInUI || mouseFocusNameplate != null;

            if (_objectSelect && mouseNotInUIOrInNameplate && _objectDragging == null)
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
                        if (bounds.Contains(mouseWorld)) results.Add(obj);
                    }
                }

                if (mouseFocusNameplate != null) results.Add(mouseFocusNameplate.Object);

                SelectObjectMulti(results);
            }
            else if (_objectDragging != null)
            {
                SelectObject(_objectDragging, true);
            }
            else
            {
                SelectObject(null);
            }

            if (_objectDragging != null && mouseNotInUIOrInNameplate)
            {
                Vector2 mouseScreen = Engine.Host.MousePosition;
                Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen);
                _objectDragging.Position = (mouseWorld + _objectDragOffset).ToVec3(_objectDragging.Z);
                EditorRegisterMoveAction(_objectDragging, _objectDragStartPos, _objectDragging.Position2);
            }

            _editUI!.Update();

            if (!mouseNotInUIOrInNameplate) return;
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
                Rectangle bound = _lastMouseOverObject.Bounds;
                c.RenderSprite(bound, Color.White * 0.3f);
            }

            // Draw some indication of unspawned serialized objects.
            for (var i = 0; i < ObjectsToSerialize.Count; i++)
            {
                GameObject2D obj = ObjectsToSerialize[i];
                if (obj.ObjectState != ObjectState.Alive)
                {
                    Rectangle bounds = obj.Bounds;
                    c.RenderSprite(bounds, Color.Magenta * 0.2f);
                    c.RenderOutline(bounds, Color.White * 0.4f);
                }
            }

            int count = GetObjectCount();
            for (var i = 0; i < count; i++)
            {
                GameObject2D obj = GetObjectByIndex(i);
                Rectangle bounds = obj.Bounds;

                if (!obj.MapFlags.HasFlag(Map2DObjectFlags.Serializable))
                {
                    c.RenderSprite(bounds, Color.Black * 0.5f);
                    c.RenderLine(bounds.TopLeft, bounds.BottomRight, Color.Black);
                    c.RenderLine(bounds.TopRight, bounds.BottomLeft, Color.Black);
                }

                c.RenderOutline(bounds, Color.White * 0.4f);
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
            return obj.Bounds;
        }

        private int ObjectSort(GameObject2D x, GameObject2D y)
        {
            return MathF.Sign(x.Position.Z - y.Position.Z);
        }

        private void SelectObjectMulti(List<GameObject2D>? multiple)
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

        private void SelectObject(GameObject2D? obj, bool forceUpdate = false)
        {
            if (_lastMouseOverObject == obj && !forceUpdate) return;

            MapEditorObjectNameplate? namePlate = null;
            if (obj != null) _namePlates?.TryGetValue(obj, out namePlate);

            MapEditorObjectNameplate? oldNamePlate = null;
            if (_lastMouseOverObject != null) _namePlates?.TryGetValue(_lastMouseOverObject, out oldNamePlate);
            if (namePlate != oldNamePlate)
            {
                oldNamePlate?.SetSelected(false);
                namePlate?.SetSelected(true);
            }

            var worldAttachUI = (UIWorldAttachedWindow?) _editUI?.GetWindowById("WorldAttach");

            _lastMouseOverObject = obj;

            // Deselecting
            if (obj == null || _objectDragging != null)
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

        private void EditorSaveMap()
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

        private Dictionary<GameObject2D, MapEditorObjectNameplate>? _namePlates;

        private void EnsureObjectNameplate(GameObject2D obj)
        {
            _namePlates ??= new Dictionary<GameObject2D, MapEditorObjectNameplate>();
            if (_namePlates.ContainsKey(obj)) return;

            var namePlate = new MapEditorObjectNameplate();
            namePlate.AttachToObject(obj);
            _namePlates.Add(obj, namePlate);
            _editUI!.AddChild(namePlate);
        }

        #region Object Control API

        private void EditorAddObject(Type type)
        {
            Vector2 pos = Engine.Host.MousePosition;
            Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(pos);

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes)!;
            var newObj = (GameObject2D) constructor.Invoke(null);
            newObj.MapFlags |= Map2DObjectFlags.Serializable;
            newObj.Position = worldPos.ToVec3();
            AddObject(newObj);
            EnsureObjectNameplate(newObj);

            _objectDragging = newObj;
            _objectDragOffset = newObj.Size / 2f;
            _ignoreNextObjectDragLetGo = true;
        }

        #endregion
    }
}
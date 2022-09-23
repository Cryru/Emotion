#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Emotion.Common;
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

        // Selection
        private GameObject2D? _lastSelectedObject;
        private bool _objectSelect = true;
        private List<GameObject2D>? _selectionOthers;
        private int _selectionIndex = -1;

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

            if (_selectionOthers != null && key == Key.LeftAlt && status == KeyStatus.Down)
            {
                _selectionIndex++;
                if (_selectionIndex > _selectionOthers.Count - 1) _selectionIndex = 0;
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

            _lastSelectedObject = null;

            Engine.Renderer.Camera = _oldCamera;
        }

        protected void UpdateDebug()
        {
            if (!EditorMode) return;

            if (_objectSelect)
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

            _editUI!.Update();
            Helpers.CameraWASDUpdate();
        }

        protected void RenderDebug(RenderComposer c)
        {
            if (!EditorMode) return;
            RenderState? prevState = c.CurrentState.Clone();

            c.SetUseViewMatrix(true);

            // Show selection of object, if any.
            if (_lastSelectedObject != null)
            {
                Rectangle bound = GetObjectBoundForEditor(_lastSelectedObject);
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
                _selectionIndex = -1;
                _selectionOthers = null;
                SelectObject(null);
                return;
            }

            var forceUpdate = false;
            multiple.Sort(ObjectSort);
            if (_selectionOthers != null) // Update multi
            {
                GameObject2D current = _selectionOthers[_selectionIndex];
                int currentIdxInNew = multiple.IndexOf(current);
                bool sameCount = multiple.Count == _selectionOthers.Count;
                if (!sameCount)
                {
                    forceUpdate = true;
                }
                else if (currentIdxInNew == _selectionIndex)
                {
                    var allSame = true;
                    for (var i = 0; i <  multiple.Count; i++)
                    {
                        GameObject2D obj = multiple[i];
                        GameObject2D other = _selectionOthers[i];
                        if (obj != other)
                        {
                            allSame = false;
                            break;
                        }
                    }
                    if(!allSame) forceUpdate = true;
                }

                _selectionIndex = currentIdxInNew != -1 ? currentIdxInNew : 0;
            }
            else
            {
                _selectionIndex = 0;
            }

            _selectionOthers = multiple;
            SelectObject(_selectionOthers[_selectionIndex], forceUpdate);
        }

        protected void SelectObject(GameObject2D? obj, bool forceUpdate = false)
        {
            if (_lastSelectedObject == obj && !forceUpdate) return;

            _lastSelectedObject = obj;
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
                    txt.AppendLine($"   Pos: {obj.Position}");
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

                    if (_selectionOthers != null && _selectionOthers.Count > 0) txt.AppendLine($"Objects Here: {_selectionIndex + 1}/{_selectionOthers.Count} [ALT] to switch");

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
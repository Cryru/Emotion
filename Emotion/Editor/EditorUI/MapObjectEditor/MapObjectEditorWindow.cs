#nullable enable

using Emotion.Editor.Editor2D;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.Systems.UI;
using Emotion.Game.World;
using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TwoDee;
using Emotion.Graphics.Camera;

namespace Emotion.Editor.EditorUI.MapObjectEditor;

public class MapObjectEditorWindow : UIBaseWindow
{
    public MapObject? SelectedObject;
    public MapObject? MouseoverObject;

    private EditorLabel _bottomText = null!;
    private TranslationGizmo? _moveGizmo;

    public MapObjectEditorWindow()
    {
        HandleInput = true;
        OrderInParent = -1;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        _moveGizmo = new TranslationGizmo();

        GameMap? map = EngineEditor.GetCurrentMap();
        map?.AddObject(_moveGizmo);
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);

        if (_moveGizmo != null)
        {
            _moveGizmo.RemoveFromMap();
            _moveGizmo = null;
        }
    }

    public void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        // Bottom text
        {
            var textList = new UIBaseWindow()
            {
                LayoutMode = LayoutMode.HorizontalList,
                ListSpacing = new Vector2(10, 0),
                AnchorAndParentAnchor = UIAnchor.CenterLeft,
                GrowY = false,
            };
            barContent.AddChild(textList);

            var label = new EditorLabel
            ("Object Editor")
            {
                WindowColor = Color.White * 0.5f
            };
            textList.AddChild(label);

            var labelDynamic = new EditorLabel
            ("")
            {
                AllowRenderBatch = false
            };
            textList.AddChild(labelDynamic);
            _bottomText = labelDynamic;
        }
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft && status == KeyState.Down)
        {
            if (MouseoverObject != null)
            {
                if (MouseoverObject == SelectedObject)
                {
                    var editor = new ObjectPropertyEditorWindow(SelectedObject);
                    EngineEditor.EditorRoot.AddChild(editor);
                }
                else
                {
                    SelectedObject = MouseoverObject;
                }
            }
        }

        return base.OnKey(key, status, mousePos);
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        UpdateSelection();
    }

    protected override bool RenderInternal(Renderer c)
    {
        if (MouseoverObject != null)
        {
            c.SetUseViewMatrix(true);

            Cube bound = MouseoverObject.BoundingCube;
            bound.RenderOutline(c, Color.PrettyOrange, 0.05f);

            c.SetUseViewMatrix(false);
        }

        if (SelectedObject != null)
        {
            c.SetUseViewMatrix(true);

            Cube bound = SelectedObject.BoundingCube;
            bound.RenderOutline(c, Color.PrettyYellow, 0.05f);

            c.SetUseViewMatrix(false);
        }

        return base.RenderInternal(c);
    }

    private void UpdateSelection()
    {
        GameMap? map = EngineEditor.GetCurrentMap();
        if (map == null) return;

        CameraBase cam = Engine.Renderer.Camera;
        Ray3D ray = cam.GetCameraMouseRay();
        map.CollideWithRayFirst(ray, out MapObject? hit);
        MouseoverObject = hit;

        if (hit == null)
            _bottomText.Text = $"No object under mouse";
        else if (hit is MapObjectMesh meshObj)
            _bottomText.Text = $"Mouseover: {nameof(MapObjectMesh)} - {meshObj.MeshEntity.Name}";
        else if (hit is MapObjectSprite spriteObj)
            _bottomText.Text = $"Mouseover: {nameof(MapObjectSprite)} - {spriteObj.Entity.Name}";
        else
            _bottomText.Text = $"Mouseover: {hit}";
    }

    private void SelectObject(MapObject obj)
    {
        SelectedObject = obj;


    }
}

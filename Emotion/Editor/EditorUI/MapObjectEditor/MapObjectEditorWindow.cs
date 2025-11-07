#nullable enable

using Emotion.Editor.Editor2D;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.Systems.UI;
using Emotion.Game.World;
using Emotion.Game.World.Components;
using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TwoDee;
using Emotion.Graphics.Camera;
using System.Xml.Linq;

namespace Emotion.Editor.EditorUI.MapObjectEditor;

public class MapObjectEditorWindow : UIBaseWindow
{
    public GameObject? SelectedObject;
    public GameObject? MouseoverObject;

    private EditorLabel _bottomText = null!;
    private TranslationGizmo? _moveGizmo;

    public MapObjectEditorWindow()
    {
        HandleInput = true;
        OrderInParent = -1;
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        _moveGizmo = new TranslationGizmo();

        GameMap? map = EngineEditor.GetCurrentMap();
        map?.AddObject(_moveGizmo);
    }

    protected override void OnClose()
    {
        base.OnClose();

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
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(10),
                    AnchorAndParentAnchor = UIAnchor.CenterLeft,
                    SizingY = UISizing.Fit()
                }
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
                    EngineEditor.EditorUI.AddChild(editor);
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

            Cube bound = MouseoverObject.GetBoundingCube();
            bound.RenderOutline(c, Color.PrettyOrange, 0.05f);

            c.SetUseViewMatrix(false);
        }

        if (SelectedObject != null)
        {
            c.SetUseViewMatrix(true);

            Cube bound = SelectedObject.GetBoundingCube();
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
        map.CollideWithRayFirst(ray, out GameObject? hit);
        MouseoverObject = hit;

        if (hit == null)
            _bottomText.Text = $"No object under mouse";
        else if (hit.GetComponent<MeshComponent>(out MeshComponent? component))
            _bottomText.Text = $"Mouseover: {hit.Name} - {component.Entity.Name}";
        else if (hit.GetComponent<SpriteComponent>(out SpriteComponent? spriteComponent))
            _bottomText.Text = $"Mouseover: {hit.Name} - {spriteComponent.Entity.Name}";
        else
            _bottomText.Text = $"Mouseover: {hit.Name}";
    }

    private void SelectObject(GameObject obj)
    {
        SelectedObject = obj;


    }
}

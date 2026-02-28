#nullable enable

namespace Emotion.Editor.Workflow.LevelDesign;

public class ObjectSelectionWindow : UIBaseWindow
{
    private GameObject? _objectUnderMouse;

    public ObjectSelectionWindow()
    {
        HandleInput = true;
    }

    public override void OnMouseEnter(Vector2 mousePos)
    {
        base.OnMouseEnter(mousePos);
        UpdateSelection();
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        base.OnMouseMove(mousePos);
        UpdateSelection();
    }

    public override void OnMouseLeft(Vector2 mousePos)
    {
        base.OnMouseLeft(mousePos);
        UpdateSelection(true);
    }

    protected override bool RenderInternal(Renderer r)
    {
        if (_objectUnderMouse != null)
        {
            var bound = _objectUnderMouse.GetBoundingCube();
            r.SetUseViewMatrix(true);
            bound.RenderOutline(r, Color.PrettyYellow, 0.03f);
            r.SetUseViewMatrix(false);
        }

        return base.RenderInternal(r);
    }

    private void UpdateSelection(bool clear = false)
    {
        var mouseRay = Engine.Renderer.Camera.GetCameraMouseRay();
        var map = EngineEditor.GetCurrentMap();
        if (map == null) return; // huh?

        GameObject? newSel = null;

        if (!clear)
        {
            if (map.CollideWithRayFirst(mouseRay, out newSel))
            {
                bool a = true;
            }
        }

        _objectUnderMouse = newSel;
    }
}
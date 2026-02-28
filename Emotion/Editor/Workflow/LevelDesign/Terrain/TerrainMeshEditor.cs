using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.World.Terrain;

namespace Emotion.Editor.Workflow.LevelDesign.Terrain;

public class TerrainMeshEditor : UIBaseWindow
{
    private static TerrainMeshGridNew.TerrainBrush _brush = new TerrainMeshGridNew.TerrainBrush()
    {
        Operation = TerrainMeshGridNew.BrushOperation.ChangeHeight,
        Size = 2,
        Strength = 0.75f,
        Color = Color.Red,
    };

    public TerrainMeshEditor()
    {
        HandleInput = true;

        var controls = new UIBaseWindow()
        {
            Layout =
                {
                    Margins = new UISpacing(0, 50, 0, 0),
                    LayoutMethod = UILayoutMethod.VerticalList(1)
                }
        };
        AddChild(controls);

        UIBaseWindow brushSize = TypeEditor.CreateCustomWithLabel("Brush Size", _brush.Size, static (v) => _brush.Size = v, LabelStyle.MapEditor);
        brushSize.Layout.SizingX = UISizing.Fit();
        controls.AddChild(brushSize);

        UIBaseWindow brushStr = TypeEditor.CreateCustomWithLabel("Brush Strength", _brush.Strength, static (v) => _brush.Strength = v, LabelStyle.MapEditor);
        brushStr.Layout.SizingX = UISizing.Fit();
        controls.AddChild(brushStr);

        var brushes = new UIBaseWindow()
        {
            Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(1),
                    SizingY = UISizing.Fit()
                }
        };
        controls.AddChild(brushes);

        var colors = new Color[5] { Color.Red, Color.Green, Color.Blue, Color.PrettyOrange, Color.Black };
        for (int i = 0; i < 5; i++)
        {
            var color = colors[i];
            var colButton = new OneButton(color.ToString(), (_) => { _brush.Color = color; });
            brushes.AddChild(colButton);
        }

        var brushTools = new UIBaseWindow()
        {
            Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(1),
                    SizingY = UISizing.Fit()
                }
        };
        controls.AddChild(brushTools);

        foreach (var item in EnumExtensions.ReflectorGetValues<TerrainMeshGridNew.BrushOperation>())
        {
            brushTools.AddChild(new OneButton(item.ToString(), (_) => { _brush.Operation = item; }));
        }
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

    private void UpdateSelection(bool clear = false)
    {
        GameMap? map = EngineEditor.GetCurrentMap();
        TerrainMeshGridNew? terrainGrid = map?.GetFirstGridOfType<TerrainMeshGridNew>();
        if (terrainGrid == null) return;

        //terrainGrid.SetEditorBrush(!clear, _brushSize, _brushStr);
        if (clear) return;
    }

    protected override bool UpdateInternal()
    {
        if (_mouseDown)
        {
            GameMap? map = EngineEditor.GetCurrentMap();
            TerrainMeshGridNew? terrainGrid = map?.GetFirstGridOfType<TerrainMeshGridNew>();
            if (terrainGrid == null) return true;

            terrainGrid.ApplyBrushHeight(_brush);
        }

        return base.UpdateInternal();
    }

    private bool _mouseDown;

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft)
        {
            _mouseDown = status == KeyState.Down;
            return true;
        }

        return base.OnKey(key, status, mousePos);
    }
}
#nullable enable

using Emotion.Game.World.Editor;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.UI;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.MapObjectEditor;

namespace Emotion.WIPUpdates.One.EditorUI;

public class MapEditorViewMode : UIBaseWindow
{
    public MapEditorViewMode()
    {
        GrowY = false;
        GrowX = false;
        MinSizeY = 20;
        Margins = new Rectangle(0, 5, 0, 0);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow overallContainer = new()
        {
            LayoutMode = LayoutMode.VerticalList
        };
        AddChild(overallContainer);

        UISolidColor buttonContainer = new()
        {
            Id = "ButtonList",
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            AnchorAndParentAnchor = UIAnchor.CenterLeft,
            Paddings = new Rectangle(5, 5, 5, 5),
            IgnoreParentColor = true,
            WindowColor = EditorColorPalette.BarColor * 0.5f,
            GrowX = false,

            SetChildren = new List<UIBaseWindow>()
            {
                new MapEditorViewModeButton("Game")
                {
                    UserData = MapEditorMode.Off,
                    OnClickedProxy = PickerButtonPressed
                },

                new MapEditorViewModeButton("2D")
                {
                    UserData = MapEditorMode.TwoDee,
                    OnClickedProxy = PickerButtonPressed
                },

                new MapEditorViewModeButton("3D")
                {
                    UserData = MapEditorMode.ThreeDee,
                    OnClickedProxy = PickerButtonPressed
                }
            }
        };
        overallContainer.AddChild(buttonContainer);

        UIBaseWindow locationContainer = new()
        {
            LayoutMode = LayoutMode.VerticalList,
            Paddings = new Rectangle(10, 10, 5, 5),
            SetChildren = new List<UIBaseWindow>()
            {
                new EditorLabel()
                {
                    Id = "CameraPosition",
                    TextShadow = Color.Black,
                    ShadowOffset = new Vector2(1f),
                    OutlineColor = Color.Black,
                    OutlineSize = 2,
                    AllowRenderBatch = false,
                    FontSize = 25,
                    Text = "Position:\nLook At:"
                },
                new MapEditorViewModeOrientationGizmo()
                {
                    GrowX = false,
                    GrowY = false,
                    Margins = new Rectangle(0, 10, 0, 0),
                    MinSize = new Vector2(128),
                }
            }
        };
        overallContainer.AddChild(locationContainer);

        EngineEditor.OnMapEditorModeChanged += EngineEditor_OnMapEditorModeChanged;
        UpdateVisuals();
    }

    protected override bool UpdateInternal()
    {
        var camLabel = GetWindowById<EditorLabel>("CameraPosition");
        if (camLabel != null)
        {
            CameraBase camera = Engine.Renderer.Camera;
            if (camera is Camera2D cam2D)
                camLabel.Text = $"Position: {camera.Position:0.00}\nLook At: {camera.LookAt:0.00}\nZoom: {cam2D.Zoom}";
            else
                camLabel.Text = $"Position: {camera.Position:0.00}\nLook At: {camera.LookAt:0.00}";
        }

        return base.UpdateInternal();
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);

        EngineEditor.OnMapEditorModeChanged -= EngineEditor_OnMapEditorModeChanged;
    }

    private void EngineEditor_OnMapEditorModeChanged(MapEditorMode obj)
    {
        UpdateVisuals();
    }

    private void PickerButtonPressed(UICallbackButton button)
    {
        if (button is MapEditorViewModeButton pickerButton && pickerButton.UserData is MapEditorMode setToMode)
            EngineEditor.SetMapEditorMode(setToMode);
    }

    private void UpdateVisuals()
    {
        MapEditorMode mode = EngineEditor.MapEditorMode;

        UIBaseWindow buttonList = GetWindowByIdSafe("ButtonList");
        foreach (UIBaseWindow button in buttonList.WindowChildren())
        {
            if (button is MapEditorViewModeButton pickerButton)
                pickerButton.SetActiveMode(mode.Equals(pickerButton.UserData));
        }
    }

    private class MapEditorViewModeButton : EditorButton
    {
        public MapEditorViewModeButton(string label) : base(label)
        {
            NormalColor = new Color("#2e2a36");
            RolloverColor = new Color("#d6d6d6");
            ActiveColor = new Color("#d9d1eb");
        }

        protected override void RecalculateButtonColor()
        {
            if (_label != null)
                _label.WindowColor = _activeMode ? Color.Black : Color.White;

            base.RecalculateButtonColor();
        }
    }

    private class MapEditorViewModeOrientationGizmo : UIBaseWindow
    {
        private static MeshEntity _gizmoEntity = null!;

        public MapEditorViewModeOrientationGizmo()
        {
            _gizmoEntity ??= TranslationGizmo.GetTranslationGizmoEntity(15, 15, false)!;
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            //c.SetUseViewMatrix(true);
            //c.RenderLine(new Vector3(0, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
            //c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
            //c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);
            //c.SetUseViewMatrix(false);

            c.RenderCircle(Position, Size.X / 2f, Color.White * 0.5f);
            c.SetDepthTest(true);

            // todo: weirdness
            // why is the scale inverted on the z axis
            // why is the scale after the rotate, it should be SRT (scale, rotate, translate)
            // wtf is going on here?
            // if the scale is before the rotate it doesnt need to be inverted if the default projection has the near and far swapped.
            // the mesh seems to be correct?
            c.PushModelMatrix(c.Camera.GetRotationMatrix());
            c.PushModelMatrix(Matrix4x4.CreateScale(2f * GetScale(), 2f * GetScale(), -2f * GetScale()));
            c.PushModelMatrix(Matrix4x4.CreateTranslation((Center + new Vector2(0, 0)).ToVec3(100)));

            // todo: render 3d in UI
            if (_gizmoEntity.Meshes != null)
            {
                for (int i = 0; i < _gizmoEntity.Meshes.Length; i++)
                {
                    Mesh mesh = _gizmoEntity.Meshes[i];
                    mesh.Render(c);
                }
            }

            c.PopModelMatrix();
            c.PopModelMatrix();
            c.PopModelMatrix();

            c.SetDepthTest(false);

            return base.RenderInternal(c);
        }

        protected override bool UpdateInternal()
        {
            return base.UpdateInternal();
        }
    }
}

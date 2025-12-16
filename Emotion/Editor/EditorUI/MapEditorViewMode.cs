#nullable enable

using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.MapObjectEditor;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Text;

namespace Emotion.Editor.EditorUI;

public class MapEditorViewMode : UIBaseWindow
{
    public MapEditorViewMode()
    {
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
        Layout.MinSizeY = 20;
        Layout.Margins = new UISpacing(0, 5, 0, 0);

        UIBaseWindow overallContainer = new()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(0)
            }
        };
        AddChild(overallContainer);

        UIBaseWindow buttonContainer = new()
        {
            Name = "ButtonList",
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(5),
                Padding = new UISpacing(5, 5, 5, 5),
                SizingX = UISizing.Fit()
            },
            Visuals =
            {
                BackgroundColor = EditorColorPalette.BarColor * 0.5f
            },

            IgnoreParentColor = true,

            Children = new List<UIBaseWindow>()
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
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(0),
                Padding = new UISpacing(10, 10, 5, 5),
            },

            Children = new List<UIBaseWindow>()
            {
                new EditorLabel()
                {
                    Name = "CameraPosition",
                    Effect = TextEffect.Outline(Color.Black * 0.5f, 2),
                    FontSize = 23
                },
                new MapEditorViewModeOrientationGizmo()
                {
                    Layout =
                    {
                        Margins = new UISpacing(0, 10, 0, 0),
                        SizingX = UISizing.Fixed(100),
                        SizingY = UISizing.Fixed(100),
                    }
                }
            }
        };
        overallContainer.AddChild(locationContainer);
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        EngineEditor.OnMapEditorModeChanged += EngineEditor_OnMapEditorModeChanged;
        UpdateActiveMapEditorMode();
    }

    protected override void OnClose()
    {
        base.OnClose();

        EngineEditor.OnMapEditorModeChanged -= EngineEditor_OnMapEditorModeChanged;
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

    private void EngineEditor_OnMapEditorModeChanged(MapEditorMode obj)
    {
        UpdateActiveMapEditorMode();
    }

    private void PickerButtonPressed(UICallbackButton button)
    {
        if (button is MapEditorViewModeButton pickerButton && pickerButton.UserData is MapEditorMode setToMode)
            EngineEditor.SetMapEditorMode(setToMode);
    }

    private void UpdateActiveMapEditorMode()
    {
        MapEditorMode mode = EngineEditor.MapEditorMode;

        UIBaseWindow buttonList = GetWindowByIdSafe("ButtonList");
        foreach (UIBaseWindow button in buttonList.Children)
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
                _label.TextColor = _activeMode ? Color.Black : Color.White;

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

        protected override void InternalRender(Renderer r)
        {
            base.InternalRender(r);

            Vector2 pos = CalculatedMetrics.Position.ToVec2();
            Vector2 size = CalculatedMetrics.Size.ToVec2();
            Vector2 center = pos + size / 2;

            //c.SetUseViewMatrix(true);
            //c.RenderLine(new Vector3(0, 0, 0), new Vector3(short.MaxValue, 0, 0), Color.Red, snapToPixel: false);
            //c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, short.MaxValue, 0), Color.Green, snapToPixel: false);
            //c.RenderLine(new Vector3(0, 0, 0), new Vector3(0, 0, short.MaxValue), Color.Blue, snapToPixel: false);
            //c.SetUseViewMatrix(false);

            r.RenderCircle(pos.ToVec3(), size.X / 2f, Color.White * 0.5f);
            r.SetDepthTest(true);

            // todo: weirdness
            // why is the scale inverted on the z axis
            // why is the scale after the rotate, it should be SRT (scale, rotate, translate)
            // wtf is going on here?
            // if the scale is before the rotate it doesnt need to be inverted if the default projection has the near and far swapped.
            // the mesh seems to be correct?
            r.PushModelMatrix(r.Camera.GetRotationMatrix());
            r.PushModelMatrix(Matrix4x4.CreateScale(1.5f * GetScale(), 1.5f * GetScale(), -1.5f * GetScale()));
            r.PushModelMatrix(Matrix4x4.CreateTranslation((center + new Vector2(0, 0)).ToVec3(100)));

            // todo: render 3d in UI
            if (_gizmoEntity.Meshes != null)
            {
                for (int i = 0; i < _gizmoEntity.Meshes.Length; i++)
                {
                    Mesh mesh = _gizmoEntity.Meshes[i];
                    mesh.Render(r);
                }
            }

            r.PopModelMatrix();
            r.PopModelMatrix();
            r.PopModelMatrix();

            r.SetDepthTest(false);
        }
    }
}

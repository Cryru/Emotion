using Emotion.Core.Systems.Input;
using Emotion.Editor.EditorUI.Components;
using Emotion.Graphics.Camera;

#nullable enable

namespace Emotion.Editor.Tools.InterfaceTool;

public class UIViewport : EditorProxyRender
{
    public class ViewportCamera2D : Camera2D
    {
        private UIViewport _ui;

        public ViewportCamera2D(UIViewport ui) : base(Vector3.Zero, 1f, KeyListenerType.None)
        {
            _ui = ui;

            ZoomAllowed = true;
            MaxZoom = 10f;
            MovementSpeed = 15;
        }

        public bool OnKey(Key key, KeyState status)
        {
            return CameraKeyHandler(key, status);
        }

        protected override Vector2 GetZoomMousePos()
        {
            return Engine.Host.MousePosition - _ui.Position2;
        }

        /// <inheritdoc />
        public override void RecreateViewMatrix()
        {
            float scale = Engine.Renderer.Scale * Zoom;
            CalculatedScale = scale;

            Vector3 pos = new Vector3(X, Y, 0);
            Vector3 lookAt = _lookAt;
            Vector3 worldUp = GetCameraWorldUp();
            Matrix4x4 unscaled = Matrix4x4.CreateLookAtLeftHanded(pos, pos + lookAt, worldUp);
            ViewMatrix = Matrix4x4.CreateScale(new Vector3(scale, -scale, 1), pos) * unscaled;
        }
    }

    private Vector2? _startDrag = null;

    private ViewportCamera2D _camera;

    public UIViewport()
    {
        HandleInput = true;
        _camera = new ViewportCamera2D(this);
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseWheel)
        {
            var val = _camera.OnKey(key, status);
            if (!val)
                return false;
        }

        if (key == Key.MouseKeyLeft)
        {
            if (status == KeyState.Down)
                _startDrag = mousePos;
            else
                _startDrag = null;
        }

        return base.OnKey(key, status, mousePos);
    }

    protected override bool UpdateInternal()
    {
        _camera.Update();

        if (_startDrag != null)
        {
            var mouse = Engine.Input.MousePosition;
            var diff = _camera.ScreenToWorld(_startDrag.Value) - _camera.ScreenToWorld(mouse);
            _camera.Position2 += diff.ToVec2();
            _startDrag = mouse;
        }

        return base.UpdateInternal();
    }

    protected override bool RenderInternal(Renderer c)
    {
        c.RenderSprite(Position, Size, _calculatedColor);
        var oldClip = c.CurrentState.ClipRect;
        c.SetClipRect(Bounds);

        c.PushModelMatrix(
            _camera.ViewMatrix *
            Matrix4x4.CreateTranslation(X, Y, 0)
        );
        base.RenderInternal(c);
        
        if (_camera.CalculatedScale > 8f)
        {
            float pixelAlpha = Maths.Map(_camera.CalculatedScale, 8, 15, 0, 1);
            pixelAlpha = Maths.Clamp(pixelAlpha, 0f, 1f);

            Color gridColor = Color.White * pixelAlpha;
            c.RenderGrid(Vector3.Zero, new Vector2(1000), new Vector2(1f), gridColor, new Vector2(0.45f));
        }

        c.PopModelMatrix();
        c.SetClipRect(oldClip);
        return true;
    }
}

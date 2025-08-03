#nullable enable

using Emotion.Graphics.Camera;

namespace Emotion.Game.Systems.UI
{
    public class UIWorldAttachedWindow : UIBaseWindow
    {
        protected Vector3 _worldPos;
        protected bool _awaitingLayout = true;

        public UIWorldAttachedWindow()
        {
            StretchX = true;
            StretchY = true;
        }

        public void AttachToPosition(Vector3 pos)
        {
            _worldPos = pos;
        }

        public override void InvalidateLayout()
        {
            _awaitingLayout = true;
            base.InvalidateLayout();
        }

        protected override void AfterLayout()
        {
            _awaitingLayout = false;
            base.AfterLayout();
        }

        protected virtual Vector3 VerifyWorldPos(Vector3 pos)
        {
            return pos;
        }

        protected override bool RenderInternal(Renderer c)
        {
            if (Children == null || _awaitingLayout) return false;

            // We update this in the renderer rather than through the transformation matrix to ensure it is always up to date.
            Vector3 pos = c.Camera.WorldToScreen(_worldPos).ToVec3(
                Maths.Clamp(_worldPos.Z, CameraBase.NearZDefault2DProjection, CameraBase.FarZDefault2DProjection)
            );
            pos = VerifyWorldPos(pos);

            _renderBounds = new Rectangle(pos + Position, Size);
            _renderBoundsWithChildren = new Rectangle(pos + Position, Size);

            c.PushModelMatrix(Matrix4x4.CreateTranslation(pos));
            return base.RenderInternal(c);
        }

        protected override void AfterRenderChildren(Renderer c)
        {
            base.AfterRenderChildren(c);
            c.PopModelMatrix();
        }
    }
}
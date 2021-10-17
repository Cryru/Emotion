#region Using

using System.Numerics;
using Emotion.Graphics;

#endregion

namespace Emotion.UI
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

        protected override bool RenderInternal(RenderComposer c)
        {
            if (Children == null || _awaitingLayout) return false;

            Vector3 pos = c.Camera.WorldToScreen(_worldPos.ToVec2()).ToVec3(_worldPos.Z);
            c.PushModelMatrix(Matrix4x4.CreateTranslation(pos));
            return base.RenderInternal(c);
        }

        protected override void AfterRenderChildren(RenderComposer c)
        {
            base.AfterRenderChildren(c);
            c.PopModelMatrix();
        }
    }
}
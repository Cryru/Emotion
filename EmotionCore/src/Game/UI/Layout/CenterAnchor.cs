// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.System;

#endregion

namespace Emotion.Game.UI.Layout
{
    public class CenterAnchor : ParentControl
    {
        private List<LayoutControl> _controls;
        private EventHandler<EventArgs> _updateEvent;

        public CenterAnchor() : base(Vector3.Zero, Vector2.Zero)
        {
            _controls = new List<LayoutControl>();
            _updateEvent = (a, b) => ApplyLogic();
        }

        #region Parenting

        /// <summary>
        /// Add a control to the center anchor.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="margin">The margin of the control.</param>
        public void AddControl(Control control, Rectangle margin)
        {
            AddControl((Transform) control, margin);
            AddChild(control);
        }

        /// <summary>
        /// Add a transform to the center anchor.
        /// </summary>
        /// <param name="transform">The transform to add.</param>
        /// <param name="margin">The margin of the control.</param>
        public void AddControl(Transform transform, Rectangle margin)
        {
            LayoutControl anchorControl = new LayoutControl
            {
                Control = transform,
                Margin = margin
            };

            lock (_controls)
            {
                _controls.Add(anchorControl);
            }

            // Hook up to event.
            transform.OnResize += _updateEvent;

            ApplyLogic();
        }

        /// <summary>
        /// Remove a transform from the center anchor. If not found nothing will happen.
        /// </summary>
        /// <param name="transform">The control to remove.</param>
        public void RemoveControl(Transform transform)
        {
            lock (_controls)
            {
                LayoutControl match = _controls.FirstOrDefault(x => x.Control == transform);
                if (match != null) _controls.Remove(match);
            }

            // Unhook from event.
            transform.OnResize -= _updateEvent;

            ApplyLogic();
        }

        /// <summary>
        /// Remove a control from the center anchor. If not found nothing will happen.
        /// </summary>
        /// <param name="control">The control to remove.</param>
        public void RemoveControl(Control control)
        {
            RemoveControl((Transform) control);
            RemoveChild(control);
        }

        #endregion

        /// <summary>
        /// Performs checks on whether the anchoring logic is applied.
        /// </summary>
        /// <param name="renderer">The renderer to use for debugging.</param>
        public override void Render(Renderer renderer)
        {
            // Render children;
            base.Render(renderer);

            // Check if performing debug drawing.
            if (!Controller.DebugDraw) return;

#if DEBUG
            renderer.RenderOutlineFlush();
            renderer.RenderFlush();

            float screenWidth = Context.Settings.RenderWidth;
            float screenHeight = Context.Settings.RenderHeight;

            renderer.RenderQueueOutline(new Vector3(screenWidth / 2, 0, 0), new Vector2(0, screenHeight), Color.Pink);
            renderer.RenderQueueOutline(new Vector3(0, screenHeight / 2, 0), new Vector2(screenWidth, 0), Color.Pink);

            lock (_controls)
            {
                foreach (LayoutControl control in _controls)
                {
                    control.Draw(renderer);
                }
            }

            renderer.RenderOutlineFlush();
            renderer.RenderFlush();
#endif
        }

        private void ApplyLogic()
        {
            float screenWidth = Context.Settings.RenderWidth;
            float screenHeight = Context.Settings.RenderHeight;

            lock (_controls)
            {
                foreach (LayoutControl control in _controls)
                {
                    Vector2 centralPoint = new Vector2(screenWidth / 2, screenHeight / 2);
                    centralPoint.X += control.Margin.X;
                    centralPoint.Y += control.Margin.Y;
                    centralPoint.X -= control.Margin.Width;
                    centralPoint.Y -= control.Margin.Height;
                    control.Control.Center = centralPoint;
                }
            }
        }
    }
}
// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Engine;
using Emotion.Primitives;

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

        public override void Init()
        {
            if (Parent == null) Size = new Vector2(Context.Settings.RenderWidth, Context.Settings.RenderHeight);
        }

        #region Parenting

        /// <summary>
        /// Add a child to the center anchor.
        /// </summary>
        /// <param name="transform">The child to add.</param>
        public override void AddChild(Transform transform)
        {
            AddChild(transform, Rectangle.Empty);
        }

        /// <summary>
        /// Add a child to the center anchor.
        /// </summary>
        /// <param name="transform">The child to add.</param>
        /// <param name="margin">The margin of the control.</param>
        public void AddChild(Transform transform, Rectangle margin)
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
            base.AddChild(transform);
        }

        /// <summary>
        /// Remove a child from the center anchor. If not found nothing will happen.
        /// </summary>
        /// <param name="transform">The child to remove.</param>
        public override void RemoveChild(Transform transform)
        {
            lock (_controls)
            {
                LayoutControl match = _controls.FirstOrDefault(x => x.Control == transform);
                if (match != null)
                    _controls.Remove(match);
                else
                    return;
            }

            // Unhook from event.
            transform.OnResize -= _updateEvent;

            ApplyLogic();
            base.RemoveChild(transform);
        }

        #endregion

        /// <summary>
        /// Performs checks on whether the anchoring logic is applied.
        /// </summary>
        public override void Render()
        {
            // Render children;
            base.Render();

            // Check if performing debug drawing.
            if (!Controller.DebugDraw) return;

#if DEBUG
            Context.Renderer.RenderFlush();
            Context.Renderer.RenderFlush();

            Context.Renderer.RenderQueueOutline(new Vector3(Width / 2, 0, 0), new Vector2(0, Height), Color.Pink);
            Context.Renderer.RenderQueueOutline(new Vector3(0, Height / 2, 0), new Vector2(Width, 0), Color.Pink);

            lock (_controls)
            {
                foreach (LayoutControl control in _controls)
                {
                    control.Render();
                }
            }

            Context.Renderer.RenderFlush();
            Context.Renderer.RenderFlush();
#endif
        }

        private void ApplyLogic()
        {
            lock (_controls)
            {
                foreach (LayoutControl control in _controls)
                {
                    Vector2 centralPoint = new Vector2(Width / 2, Height / 2);
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
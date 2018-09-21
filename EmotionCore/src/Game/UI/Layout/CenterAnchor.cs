// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.UI.Layout
{
    public class CenterAnchor : Control
    {
        private List<LayoutControl> _controls;
        private bool _logicApplied;

        public CenterAnchor() : base(Rectangle.Empty, 0)
        {
            _controls = new List<LayoutControl>();
        }

        /// <summary>
        /// Add a control to the center anchor.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="margin">The margin of the control.</param>
        public void AddControl(Transform control, Rectangle margin)
        {
            LayoutControl anchorControl = new LayoutControl
            {
                Control = control,
                Margin = margin
            };

            lock (_controls)
            {
                _controls.Add(anchorControl);
            }

            _logicApplied = false;
        }

        /// <summary>
        /// Remove a control from the center anchor. If not found nothing will happen.
        /// </summary>
        /// <param name="control">The control to remove.</param>
        public void RemoveControl(Transform control)
        {
            lock (_controls)
            {
                LayoutControl match = _controls.FirstOrDefault(x => x.Control == control);
                if (match != null) _controls.Remove(match);
            }
        }

        /// <summary>
        /// Performs checks on whether the anchoring logic is applied.
        /// </summary>
        /// <param name="renderer">The renderer to use for debugging.</param>
        public override void Draw(Renderer renderer)
        {
            if (!_logicApplied)
            {
                ApplyLogic();
                _logicApplied = true;
            }

            // Check if performing debug drawing.
            if (!Controller.DebugDraw) return;

#if DEBUG
            renderer.RenderOutlineFlush();
            renderer.RenderFlush();

            float screenWidth = Controller.Context.Settings.RenderWidth;
            float screenHeight = Controller.Context.Settings.RenderHeight;

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

        public void Update()
        {
            _logicApplied = false;
        }

        private void ApplyLogic()
        {
            float screenWidth = Controller.Context.Settings.RenderWidth;
            float screenHeight = Controller.Context.Settings.RenderHeight;

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
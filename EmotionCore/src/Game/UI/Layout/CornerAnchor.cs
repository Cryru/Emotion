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
    public class CornerAnchor : ParentControl
    {
        #region Properties

        /// <summary>
        /// The overall padding.
        /// </summary>
        public Rectangle Padding { get; set; }

        /// <summary>
        /// What percent of the width can one column take. Take in mind that there are two columns.
        /// </summary>
        public float ColumnLimit = 50;

        #endregion

        private List<AnchorLayoutControl> _controls;
        private EventHandler<EventArgs> _updateEvent;

        public CornerAnchor() : base(Vector3.Zero, Vector2.Zero)
        {
            _controls = new List<AnchorLayoutControl>();
            Padding = Rectangle.Empty;
            _updateEvent = (a, b) => ApplyLogic();
        }

        #region Parenting

        /// <summary>
        /// Add a control to the anchor.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="anchor">The corner of the screen to anchor the control to.</param>
        public void AddControl(Control control, AnchorLocation anchor)
        {
            AddControl((Transform) control, anchor);
            AddChild(control);
        }

        /// <summary>
        /// Add a control to the anchor.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="anchor">The corner of the screen to anchor the control to.</param>
        /// <param name="margin">The margin of the control.</param>
        public void AddControl(Control control, AnchorLocation anchor, Rectangle margin)
        {
            AddControl((Transform) control, anchor, margin);
            AddChild(control);
        }


        /// <summary>
        /// Add a control to the corner anchor.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="anchor">The corner of the screen to anchor the control to.</param>
        public void AddControl(Transform control, AnchorLocation anchor)
        {
            AddControl(control, anchor, Rectangle.Empty);
        }

        /// <summary>
        /// Add a control to the corner anchor.
        /// </summary>
        /// <param name="transform">The control to add.</param>
        /// <param name="anchor">The corner of the screen to anchor the control to.</param>
        /// <param name="margin">The margin of the control.</param>
        public void AddControl(Transform transform, AnchorLocation anchor, Rectangle margin)
        {
            AnchorLayoutControl anchorControl = new AnchorLayoutControl
            {
                Control = transform,
                Anchor = anchor,
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
        /// Remove a control from the corner anchor. If not found nothing will happen.
        /// </summary>
        /// <param name="transform">The control to remove.</param>
        public void RemoveControl(Transform transform)
        {
            lock (_controls)
            {
                AnchorLayoutControl match = _controls.FirstOrDefault(x => x.Control == transform);
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
            base.Render(renderer);

            // Check if performing debug drawing.
            if (!Controller.DebugDraw) return;

#if DEBUG
            renderer.RenderOutlineFlush();
            renderer.RenderFlush();

            DrawDebugBounds(renderer, Padding, Color.Red);

            lock (_controls)
            {
                foreach (AnchorLayoutControl control in _controls)
                {
                    control.Draw(renderer);
                }
            }

            renderer.RenderOutlineFlush();
            renderer.RenderFlush();
#endif
        }

        private void DrawDebugBounds(Renderer renderer, Rectangle padding, Color color)
        {
            float screenWidth = Context.Settings.RenderWidth;
            float screenHeight = Context.Settings.RenderHeight;

            // Top
            renderer.RenderQueueOutline(new Vector3(0, 0, Z), new Vector2(screenWidth, padding.Y), color);

            // Left
            renderer.RenderQueueOutline(new Vector3(0, 0, Z), new Vector2(padding.X, screenHeight), color);

            // Bottom
            renderer.RenderQueueOutline(new Vector3(0, screenHeight - padding.Y, Z), new Vector2(screenWidth, padding.Y), color);

            // Right
            renderer.RenderQueueOutline(new Vector3(screenWidth - padding.X, 0, Z), new Vector2(padding.X, screenHeight), color);
        }

        private void ApplyLogic()
        {
            float screenWidth = Context.Settings.RenderWidth;
            float screenHeight = Context.Settings.RenderHeight;

            float limitPercentage = ColumnLimit / 100;
            float screenWidthLimit = screenWidth * limitPercentage;

            AnchorPen topLeftPen = new AnchorPen
            {
                Top = Padding.X,
                Left = Padding.Y,
                WidthLimit = screenWidthLimit,
                NeededTop = 0
            };

            AnchorPen bottomLeftPen = new AnchorPen
            {
                Top = screenHeight - Padding.Height,
                Left = Padding.Y,
                WidthLimit = screenWidthLimit,
                NeededTop = screenHeight
            };

            AnchorPen topRightPen = new AnchorPen
            {
                Top = Padding.X,
                Left = screenWidth - Padding.Width,
                WidthLimit = screenWidthLimit,
                NeededTop = 0,
                Holder = screenWidth - Padding.Width
            };

            AnchorPen bottomRightPen = new AnchorPen
            {
                Top = screenHeight - Padding.Height,
                Left = screenWidth - Padding.Width,
                WidthLimit = screenWidthLimit,
                NeededTop = screenHeight,
                Holder = screenWidth - Padding.Width
            };

            lock (_controls)
            {
                foreach (AnchorLayoutControl control in _controls)
                {
                    switch (control.Anchor)
                    {
                        case AnchorLocation.TopLeft:
                            TopLeftAnchor(topLeftPen, control);
                            break;
                        case AnchorLocation.BottomLeft:
                            BottomLeftAnchor(bottomLeftPen, control);
                            break;
                        case AnchorLocation.TopRight:
                            TopRightAnchor(topRightPen, control);
                            break;
                        case AnchorLocation.BottomRight:
                            BottomRightAnchor(bottomRightPen, control);
                            break;
                    }
                }
            }
        }

        #region Anchoring Logic

        private void TopLeftAnchor(AnchorPen pen, AnchorLayoutControl control)
        {
            // Check if reached limit.
            if (pen.Left + control.Control.Width >= pen.WidthLimit)
            {
                pen.Left = Padding.X;
                pen.Top = pen.NeededTop;
            }

            // Apply left margin.
            pen.Left += control.Margin.X;
            // Apply top margin.
            float topPenCopy = pen.Top + control.Margin.Y;

            // Position at top left.
            control.Control.X = pen.Left;
            control.Control.Y = topPenCopy;

            // Apply right margin for the next control.
            pen.Left += control.Control.Width + control.Margin.Width;

            // Set the max position of the top pen.
            float neededPenTop = control.Control.Height + topPenCopy + control.Margin.Height;
            if (neededPenTop > pen.NeededTop) pen.NeededTop = neededPenTop;
        }

        private void BottomLeftAnchor(AnchorPen pen, AnchorLayoutControl control)
        {
            // Check if reached limit.
            if (pen.Left + control.Control.Width >= pen.WidthLimit)
            {
                pen.Left = Padding.X;
                pen.Top = pen.NeededTop;
            }

            // Apply left margin.
            pen.Left += control.Margin.X;

            // Apply bottom margin and height.
            float topPenCopy = pen.Top - control.Margin.Height - control.Control.Height;

            // Position at bottom left.
            control.Control.X = pen.Left;
            control.Control.Y = topPenCopy;

            // Apply right margin for the next control.
            pen.Left += control.Control.Width + control.Margin.Width;

            // Set the max position of the top pen.
            float neededPenTop = topPenCopy - control.Margin.Y;
            if (neededPenTop < pen.NeededTop) pen.NeededTop = neededPenTop;
        }

        private void TopRightAnchor(AnchorPen pen, AnchorLayoutControl control)
        {
            // Check if reached limit.
            if (pen.Left - control.Control.Width - control.Margin.Width < pen.WidthLimit)
            {
                pen.Left = pen.Holder;
                pen.Top = pen.NeededTop;
            }

            // Apply right margin and size.
            pen.Left -= control.Margin.Width + control.Control.Width;
            // Apply top margin.
            float topPenCopy = pen.Top + control.Margin.Y;

            // Position at top left.
            control.Control.X = pen.Left;
            control.Control.Y = topPenCopy;

            // Apply left margin for the next control.
            pen.Left -= control.Margin.X;

            // Set the max position of the top pen.
            float neededPenTop = control.Control.Height + topPenCopy + control.Margin.Height;
            if (neededPenTop > pen.NeededTop) pen.NeededTop = neededPenTop;
        }

        private void BottomRightAnchor(AnchorPen pen, AnchorLayoutControl control)
        {
            // Check if reached limit.
            if (pen.Left - control.Control.Width - control.Margin.Width < pen.WidthLimit)
            {
                pen.Left = pen.Holder;
                pen.Top = pen.NeededTop;
            }

            // Apply right margin and size.
            pen.Left -= control.Margin.Width + control.Control.Width;

            // Apply bottom margin and height.
            float topPenCopy = pen.Top - control.Margin.Height - control.Control.Height;

            // Position at top left.
            control.Control.X = pen.Left;
            control.Control.Y = topPenCopy;

            // Apply left margin for the next control.
            pen.Left -= control.Margin.X;

            // Set the max position of the top pen.
            float neededPenTop = topPenCopy - control.Margin.Y;
            if (neededPenTop < pen.NeededTop) pen.NeededTop = neededPenTop;
        }

        #endregion
    }

    public enum AnchorLocation
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class AnchorLayoutControl : LayoutControl
    {
        public AnchorLocation Anchor;
    }

    public class AnchorPen
    {
        public float Top;
        public float Left;

        public float NeededTop;

        public float WidthLimit;

        public float Holder;
    }
}
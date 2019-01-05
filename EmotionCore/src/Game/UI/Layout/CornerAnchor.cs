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

        public override void Init()
        {
            if (Parent == null) Size = new Vector2(Context.Settings.RenderSettings.Width, Context.Settings.RenderSettings.Height);
        }

        #region Parenting

        /// <summary>
        /// Add a control to the anchor.
        /// </summary>
        /// <param name="control">The control to add.</param>
        public override void AddChild(Transform control)
        {
            AddChild(control, AnchorLocation.TopLeft);
        }

        /// <summary>
        /// Add a control to the corner anchor.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="anchor">The corner of the screen to anchor the control to.</param>
        public void AddChild(Transform control, AnchorLocation anchor)
        {
            AddChild(control, anchor, Rectangle.Empty);
        }

        /// <summary>
        /// Add a control to the corner anchor.
        /// </summary>
        /// <param name="transform">The control to add.</param>
        /// <param name="anchor">The corner of the screen to anchor the control to.</param>
        /// <param name="margin">The margin of the control.</param>
        public void AddChild(Transform transform, AnchorLocation anchor, Rectangle margin)
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
            base.AddChild(transform);
        }


        /// <summary>
        /// Remove a control from the corner anchor. If not found nothing will happen.
        /// </summary>
        /// <param name="transform">The control to remove.</param>
        public override void RemoveChild(Transform transform)
        {
            lock (_controls)
            {
                AnchorLayoutControl match = _controls.FirstOrDefault(x => x.Control == transform);
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
            base.Render();

            // Check if performing debug drawing.
            if (!Controller.DebugDraw) return;

#if DEBUG


            DrawDebugBounds(Padding, Color.Red);

            lock (_controls)
            {
                foreach (AnchorLayoutControl control in _controls)
                {
                    control.Render();
                }
            }


#endif
        }

        private void DrawDebugBounds(Rectangle padding, Color color)
        {
            // Top
            Context.Renderer.RenderOutline(new Vector3(0, 0, Z), new Vector2(Width, padding.Y), color);

            // Left
            Context.Renderer.RenderOutline(new Vector3(0, 0, Z), new Vector2(padding.X, Height), color);

            // Bottom
            Context.Renderer.RenderOutline(new Vector3(0, Height - padding.Y, Z), new Vector2(Width, padding.Y), color);

            // Right
            Context.Renderer.RenderOutline(new Vector3(Width - padding.X, 0, Z), new Vector2(padding.X, Height), color);
        }

        private void ApplyLogic()
        {
            float limitPercentage = ColumnLimit / 100;
            float widthLimit = Width * limitPercentage;

            AnchorPen topLeftPen = new AnchorPen
            {
                Top = Padding.X,
                Left = Padding.Y,
                WidthLimit = widthLimit,
                NeededTop = 0
            };

            AnchorPen bottomLeftPen = new AnchorPen
            {
                Top = Height - Padding.Height,
                Left = Padding.Y,
                WidthLimit = widthLimit,
                NeededTop = Height
            };

            AnchorPen topRightPen = new AnchorPen
            {
                Top = Padding.X,
                Left = Width - Padding.Width,
                WidthLimit = widthLimit,
                NeededTop = 0,
                Holder = Width - Padding.Width
            };

            AnchorPen bottomRightPen = new AnchorPen
            {
                Top = Height - Padding.Height,
                Left = Width - Padding.Width,
                WidthLimit = widthLimit,
                NeededTop = Height,
                Holder = Width - Padding.Width
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
// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Emotion.Debug;
using Emotion.GLES;
using Emotion.Input;
using Emotion.Primitives;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Game.UI
{
    public sealed class Controller
    {
        private static int _nextControllerId;

        public Controller()
        {
            Id = _nextControllerId;
            _nextControllerId++;
        }

        internal int Id;
        internal List<Control> Controls = new List<Control>();
        private Vector2 _lastMousePosition;

        /// <summary>
        /// Add a control to the controller. Called by the control when created.
        /// </summary>
        /// <param name="control">The control to add.</param>
        internal void Add(Control control)
        {
            Debugger.Log(MessageType.Info, MessageSource.UIController, "[" + Id + "] adding control " + control);

            lock (Controls)
            {
                Controls.Add(control);
                Controls.OrderBy(x => x.Priority);
            }

            Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] added control " + control);
        }

        /// <summary>
        /// Remove and dispose of a control from the controller.
        /// </summary>
        /// <param name="control">A reference to the control to remove.</param>
        internal void Remove(Control control)
        {
            Debugger.Log(MessageType.Info, MessageSource.UIController, "[" + Id + "] removing control " + control);

            lock (Controls)
            {
                // Check if destroyed.
                if (!control.Destroyed) control.Destroy();

                Controls.Remove(control);
            }

            Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] removed control " + control);
        }

        /// <summary>
        /// Draw all controls in their priority order.
        /// </summary>
        public void Draw(Renderer renderer)
        {
            foreach (Control c in Controls)
            {
                // Check if active.
                if (!c.Active) continue;

                // Draw.
                c.Draw(renderer);
                DebugDraw(renderer);
            }
        }

        /// <summary>
        /// Update the controller and its controls.
        /// </summary>
        public void Update(Input.Input input)
        {
            Vector2 mousePosition = input.GetMousePosition();

            // Check mouse inside and out.
            foreach (Control c in Controls)
            {
                // Check if active.
                if (!c.Active) continue;

                // Check if the mouse position has changed.
                if (_lastMousePosition != Vector2.Zero && _lastMousePosition != mousePosition) c.MouseMoved(_lastMousePosition, mousePosition);

                // Check if the mouse is inside this control.
                if (CheckTop(mousePosition, c))
                {
                    // Check if the mouse was already triggered as being inside.
                    if (c.MouseInside) continue;
                    Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] Mouse entered control with id " + Controls.IndexOf(c) + " - " + c);
                    c.MouseInside = true;
                    c.MouseEnter(mousePosition);
                }
                else
                {
                    // Check if the mouse was inside before.
                    if (!c.MouseInside) continue;
                    Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] Mouse left control with id " + Controls.IndexOf(c) + " - " + c);
                    c.MouseInside = false;
                    c.MouseLeave(mousePosition);
                }
            }

            // Check for button presses.
            foreach (Control c in Controls)
            {
                // Check if active or the mouse isn't inside.
                if (!c.Active) continue;

                // Loop through all buttons.
                string[] mouseButtons = Enum.GetNames(typeof(MouseKeys));
                for (int i = 0; i < mouseButtons.Length; i++)
                {
                    MouseKeys currentKey = (MouseKeys) Enum.Parse(typeof(MouseKeys), mouseButtons[i]);
                    bool held = input.IsMouseKeyHeld(currentKey);

                    // Check if the mouse is held.
                    if (held && c.MouseInside)
                    {
                        // If the button wasn't held, but now is.
                        if (c.Held[i] || HeldSomewhere(i)) continue;
                        Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] Mouse clicked using [" + currentKey + "] control with id " + Controls.IndexOf(c) + " - " + c);
                        c.Held[i] = true;
                        c.MouseDown(currentKey);
                    }

                    // Check if the button is being held.
                    if (held) continue;
                    // If the button was held, but now isn't.
                    if (!c.Held[i]) continue;
                    Debugger.Log(MessageType.Trace, MessageSource.UIController,
                        "Mouse let go of key [" + currentKey + "] on control " + Controls.IndexOf(c) + " of type [" + c + "]" + " with priority " + c.Priority);
                    c.Held[i] = false;
                    c.MouseUp(currentKey);
                }
            }

            // Record the position.
            _lastMousePosition = mousePosition;

            // Clear destroyed.
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                if (!Controls[i].Destroyed) continue;
                Debugger.Log(MessageType.Warning, MessageSource.UIController,
                    "[DEPRECATED] User has destroyed a UI control, which the controller had to clean up. Don't do this, it won't work in the future.\nControl id " + i + " of type " + Controls[i]);
                Remove(Controls[i]);
            }
        }

        #region Helpers

        /// <summary>
        /// Whether the control is on top.
        /// </summary>
        /// <param name="position">The mouse position.</param>
        /// <param name="c">The control to check whether is on top.</param>
        /// <returns>Whether the control is on top.</returns>
        private bool CheckTop(Vector2 position, Control c)
        {
            bool insideC = c.Bounds.Contains(position.X, position.Y);

            if (!insideC) return false;

            // Loop through all controls.
            foreach (Control oc in Controls)
            {
                // Check if oc is c, or if the oc is inactive.
                if (oc == c || !oc.Active) continue;

                // Check if the mouse is inside the oc.
                if (!oc.Bounds.Contains(position.X, position.Y)) continue;
                // Check if the priority is higher than c.
                if (oc.Priority <= c.Priority) continue;
                insideC = false;
                break;
            }

            return insideC;
        }

        /// <summary>
        /// Check if the mouse is held on another control. This is to prevent a click event when a control is held and the mouse
        /// moves on top of another.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is held on another control, false otherwise.</returns>
        private bool HeldSomewhere(int key)
        {
            // Loop through all controls.
            foreach (Control oc in Controls)
            {
                if (oc.Held[key]) return true;
            }

            return false;
        }

        #endregion

        #region Debugging

        [Conditional("DEBUG")]
        private void DebugDraw(Renderer renderer)
        {
            foreach (Control control in Controls)
            {
                renderer.DrawRectangleOutline(control.Bounds, control.Active ? Color.Green : Color.Red, false);
            }
        }

        public override string ToString()
        {
            string result = "[UI Controller " + Id + "]\n";

            foreach (Control control in Controls)
            {
                result += " |- " + control + "]\n";
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Cleanup resources used by the controller and run remove on all children.
        /// </summary>
        public void Dispose()
        {
            Debugger.Log(MessageType.Info, MessageSource.UIController, "[" + Id + "] destroyed.");

            lock (Controls)
            {
                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    Controls[i].Destroy();
                }

                Controls.Clear();
                Controls = null;
            }
        }
    }
}
#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.Input;
using Adfectus.Logging;

#endregion

namespace Adfectus.Game.UI
{
    /// <summary>
    /// A UI controller which manages events for UI controls.
    /// </summary>
    public sealed class Controller
    {
        private static int _nextControllerId;
        internal int Id;

        /// <summary>
        /// The Z coordinate priority of all attached UI controls.
        /// </summary>
        public int UIPriority { get; set; } = 50;

        /// <summary>
        /// The number of controls attached.
        /// </summary>
        public int ControlCount
        {
            get => Controls.Count;
        }

        #region State

        internal List<Control> Controls = new List<Control>();
        private Vector2 _lastMousePosition;
        private List<Control> _controlsToBeRemoved = new List<Control>();
        private List<Control> _controlsToBeAdded = new List<Control>();

        #endregion

        /// <summary>
        /// Create a new UI controller which manages events for UI controls.
        /// </summary>
        public Controller()
        {
            Id = _nextControllerId;
            _nextControllerId++;
        }

        /// <summary>
        /// Add a control to the controller on the next update tick.
        /// </summary>
        /// <param name="control">A reference to the control to add.</param>
        public void Add(Control control)
        {
            lock (_controlsToBeAdded)
            {
                Engine.Log.Trace($"Controller [{Id}] adding control of type [{control.GetType()}] {control}", MessageSource.UIController);
                _controlsToBeAdded.Add(control);
                control.Build(this); // Maybe the init in the build should be on another thread?
            }
        }

        /// <summary>
        /// Returns the control at the requested index.
        /// </summary>
        /// <param name="index">The index of the control to return.</param>
        public Control Get(int index)
        {
            lock (Controls)
            {
                return Controls[index];
            }
        }

        /// <summary>
        /// Returns controls of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of controls to return.</typeparam>
        /// <returns>The found controls of the specified type.</returns>
        public Control[] GetByType<T>()
        {
            lock (Controls)
            {
                return Controls.Where(x => x is T).ToArray();
            }
        }

        /// <summary>
        /// Remove and dispose of a control from the controller on the next update tick.
        /// </summary>
        /// <param name="control">A reference to the control to remove.</param>
        public void Remove(Control control)
        {
            lock (_controlsToBeRemoved)
            {
                Engine.Log.Trace($"Controller [{Id}] removing control of type [{control.GetType()}] {control}", MessageSource.UIController);
                _controlsToBeRemoved.Add(control);
            }
        }

        /// <summary>
        /// Draw all controls in their priority order.
        /// </summary>
        public void Draw()
        {
            // Disable view matrix.
            bool viewMatrixEnabled = Engine.GraphicsManager.ViewMatrixEnabled;
            if (viewMatrixEnabled) Engine.GraphicsManager.ViewMatrixEnabled = false;

            Engine.Renderer.PushToModelMatrix(Matrix4x4.CreateTranslation(0, 0, UIPriority));

            foreach (Control c in Controls)
            {
                // Check if active or is a child. Children drawing is handled by the parent.
                if (!c.Active || c.Parent != null) continue;

                // Draw.
                Engine.Renderer.Render(c);
            }

            Engine.Renderer.PopModelMatrix();
            Engine.GraphicsManager.ViewMatrixEnabled = viewMatrixEnabled;
        }

        /// <summary>
        /// Update the controller and its controls.
        /// </summary>
        public void Update()
        {
            lock (Controls)
            {
                // Remove queued controls.
                RemoveQueued();

                // Add queued.
                AddQueued();

                // Process mouse events.
                Vector2 mousePosition = Engine.InputManager.GetMousePosition();
                MouseEvents(mousePosition);

                // Check for button presses.
                ButtonPresses(Engine.InputManager);

                // Record the position.
                _lastMousePosition = mousePosition;
            }
        }

        #region Update Parts

        /// <summary>
        /// Clears controls queued for removal.
        /// </summary>
        private void RemoveQueued()
        {
            // This is a for and not a foreach because further removing can be triggered by the destroy function.
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _controlsToBeRemoved.Count; i++)
            {
                Control c = _controlsToBeRemoved[i];
                c.Destroy();
                Controls.Remove(c);
                Engine.Log.Trace($"Controller [{Id}] removed control of type [{c.GetType()}] {c}", MessageSource.UIController);
            }

            _controlsToBeRemoved.Clear();
        }

        /// <summary>
        /// Adds controls queued for initialization.
        /// </summary>
        private void AddQueued()
        {
            // This is a for and not a foreach because further adding can be triggered by the init function.
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _controlsToBeAdded.Count; i++)
            {
                Control c = _controlsToBeAdded[i];
                Controls.Add(c);
                Engine.Log.Info($"Controller [{Id}] added control of type [{c.GetType()}] {c}", MessageSource.UIController);
            }

            _controlsToBeAdded.Clear();
            Controls = Controls.OrderBy(x => x.Z).ToList();
        }

        /// <summary>
        /// Processes mouse events.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse for this tick.</param>
        private void MouseEvents(Vector2 mousePosition)
        {
            Control topControl = GetTop(mousePosition);

            Parallel.ForEach(Controls, c =>
            {
                // Check if active.
                if (!c.Active)
                {
                    // Check if it was previously, which means it was deactivated.
                    if (!c.WasActive) return;
                    Engine.Log.Trace($"Controller [{Id}] deactivated control of type [{c.GetType()}] {c}", MessageSource.UIController);
                    c.WasActive = false;
                    c.OnDeactivate();

                    // Don't update.
                    return;
                }

                // Check if it was previously inactive, which means it was activated.
                if (!c.WasActive)
                {
                    Engine.Log.Trace($"Controller [{Id}] activated control of type [{c.GetType()}] {c}", MessageSource.UIController);
                    c.WasActive = true;
                    c.OnActivate();
                }

                // Check if the mouse position has changed.
                if (_lastMousePosition != Vector2.Zero && _lastMousePosition != mousePosition) c.MouseMoved(_lastMousePosition, mousePosition);

                // Check if the mouse is inside this control.
                if (topControl == c)
                {
                    // Check if the mouse was already triggered as being inside.
                    if (c.MouseInside) return;
                    Engine.Log.Trace($"Controller [{Id}] mouse entered control of type [{c.GetType()}] {c}", MessageSource.UIController);
                    c.MouseInside = true;
                    c.MouseEnter(mousePosition);
                }
                else
                {
                    // Check if the mouse was inside before.
                    if (!c.MouseInside) return;
                    Engine.Log.Trace($"Controller [{Id}] mouse left control of type [{c.GetType()}] {c}", MessageSource.UIController);
                    c.MouseInside = false;
                    c.MouseLeave(mousePosition);
                }
            });
        }

        /// <summary>
        /// Processes mouse button press events.
        /// </summary>
        /// <param name="inputManager">The input module.</param>
        private void ButtonPresses(IInputManager inputManager)
        {
            Parallel.ForEach(Controls, c =>
            {
                // Check if active or the mouse isn't inside.
                if (!c.Active) return;

                // Loop through all buttons.
                string[] mouseButtons = Enum.GetNames(typeof(MouseKey));
                for (int i = 0; i < mouseButtons.Length; i++)
                {
                    MouseKey currentKey = (MouseKey) Enum.Parse(typeof(MouseKey), mouseButtons[i]);
                    bool down = inputManager.IsMouseKeyDown(currentKey);
                    bool up = inputManager.IsMouseKeyUp(currentKey);

                    // Check if the mouse is held.
                    if (down && c.MouseInside)
                    {
                        // If the button wasn't held, but now is.
                        if (c.Held[i] || HeldSomewhere(i)) continue;
                        Engine.Log.Trace($"Controller [{Id}] mouse clicked using key {currentKey} control of type [{c.GetType()}] {c}", MessageSource.UIController);
                        c.Held[i] = true;
                        c.MouseDown(currentKey);
                    }

                    // If the button was held, but now isn't.
                    if (up && c.Held[i])
                    {
                        Engine.Log.Trace($"Controller [{Id}] mouse let go using key {currentKey} control of type [{c.GetType()}] {c}", MessageSource.UIController);
                        c.Held[i] = false;
                        c.MouseUp(currentKey);
                    }
                }
            });
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Whether the control is on top. Unused.
        /// </summary>
        /// <param name="position">The mouse position.</param>
        /// <param name="c">The control to check whether is on top.</param>
        /// <returns>Whether the control is on top.</returns>
        public bool IsTop(Vector2 position, Control c)
        {
            bool insideC = c.GetTruePosition().Contains(position.X, position.Y);

            if (!insideC) return false;

            Parallel.ForEach(Controls, oc =>
            {
                // Check if oc is c, or if the oc is inactive.
                if (oc == c || !oc.Active) return;

                // Check if the mouse is inside the oc.
                if (!oc.GetTruePosition().Contains(position.X, position.Y)) return;
                // Check if the priority is higher than c.
                if (oc.Z <= c.Z) return;
                insideC = false;
            });

            return insideC;
        }

        /// <summary>
        /// Returns the control on top.
        /// </summary>
        /// <param name="position">The mouse position.</param>
        /// <returns>Whichever control is on top.</returns>
        private Control GetTop(Vector2 position)
        {
            Control top = null;

            foreach (Control c in Controls)
            {
                // Check if active.
                if (!c.Active) continue;

                if (c.GetTruePosition().Contains(position)) top = c;
            }

            return top;
        }

        /// <summary>
        /// Check if the mouse is held on another control. This is to prevent a click event when a control is held and the mouse
        /// moves on top of another.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is held on another control, false otherwise.</returns>
        private bool HeldSomewhere(int key)
        {
            bool held = false;

            Parallel.ForEach(Controls, oc =>
            {
                if (!oc.Active) return;
                if (oc.Held[key]) held = true;
            });

            return held;
        }

        #endregion

        /// <summary>
        /// Cleanup resources used by the controller and run remove on all children.
        /// </summary>
        public void Dispose()
        {
            Engine.Log.Info($"Controller [{Id}] destroyed", MessageSource.UIController);

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
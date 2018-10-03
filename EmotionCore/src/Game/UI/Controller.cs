// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Graphics;
using Emotion.Graphics.Text;
using Emotion.Input;
using Emotion.Primitives;
using Emotion.System;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Game.UI
{
    public sealed class Controller
    {
        private static int _nextControllerId;
        internal int Id;

        /// <summary>
        /// The Z coordinate priority of all attached UI controls.
        /// </summary>
        public int UIPriority { get; set; } = 50;

        #region State

        internal List<Control> Controls = new List<Control>();
        private Vector2 _lastMousePosition;
        private List<Control> _controlsToBeRemoved = new List<Control>();
        private List<Control> _controlsToBeAdded = new List<Control>();

        #endregion

        public Controller()
        {
            SetupDebug();

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
                Debugger.Log(MessageType.Info, MessageSource.UIController, $"[{Id}] adding control of type [{control.GetType()}] {control}");
                _controlsToBeAdded.Add(control);
                control.Build(this); // Maybe the init in the build should be on another thread?
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
                Debugger.Log(MessageType.Info, MessageSource.UIController, $"[{Id}] removing control of type [{control.GetType()}] {control}");
                _controlsToBeRemoved.Add(control);
            }
        }

        /// <summary>
        /// Draw all controls in their priority order.
        /// </summary>
        public void Draw()
        {
            Context.Renderer.DisableViewMatrix();
            Context.Renderer.MatrixStack.Push(Matrix4.CreateTranslation(0, 0, UIPriority));

            foreach (Control c in Controls)
            {
                // Check if active or is a child. Children drawing is handled by the parent.
                if (!c.Active || c.Parent != null) continue;

                // Draw.
                Context.Renderer.Render(c);
            }

            DrawDebug(Context.Renderer);
            Context.Renderer.MatrixStack.Pop();
            Context.Renderer.EnableViewMatrix();
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
                Vector2 mousePosition = Context.InputManager.GetMousePosition();
                MouseEvents(mousePosition);

                // Check for button presses.
                ButtonPresses(Context.InputManager);

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
                Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] removed control " + c);
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
                Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] added control " + c);
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
                    Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] Control was deactivated - " + c);
                    c.WasActive = false;
                    c.OnDeactivate();

                    // Don't update.
                    return;
                }

                // Check if it was previously inactive, which means it was activated.
                if (!c.WasActive)
                {
                    Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] Control was activated - " + c);
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
                    Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] Mouse entered control with id " + Controls.IndexOf(c) + " - " + c);
                    c.MouseInside = true;
                    c.MouseEnter(mousePosition);
                }
                else
                {
                    // Check if the mouse was inside before.
                    if (!c.MouseInside) return;
                    Debugger.Log(MessageType.Trace, MessageSource.UIController, "[" + Id + "] Mouse left control with id " + Controls.IndexOf(c) + " - " + c);
                    c.MouseInside = false;
                    c.MouseLeave(mousePosition);
                }
            });
        }

        /// <summary>
        /// Processes mouse button press events.
        /// </summary>
        /// <param name="inputManager">The input module.</param>
        private void ButtonPresses(InputManager inputManager)
        {
            Parallel.ForEach(Controls, c =>
            {
                // Check if active or the mouse isn't inside.
                if (!c.Active) return;

                // Loop through all buttons.
                string[] mouseButtons = Enum.GetNames(typeof(MouseKeys));
                for (int i = 0; i < mouseButtons.Length; i++)
                {
                    MouseKeys currentKey = (MouseKeys) Enum.Parse(typeof(MouseKeys), mouseButtons[i]);
                    bool held = inputManager.IsMouseKeyHeld(currentKey);

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
                        "Mouse let go of key [" + currentKey + "] on control " + Controls.IndexOf(c) + " of type [" + c + "]" + " with priority " + c.Z);
                    c.Held[i] = false;
                    c.MouseUp(currentKey);
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
                if (oc.Held[key]) held = true;
            });

            return held;
        }

        #endregion

        #region Debugging API

        private bool _debugSetup;
        internal bool DebugDraw;

        [Conditional("DEBUG")]
        private void SetupDebug()
        {
            if (_debugSetup) return;
            _debugSetup = true;

            Context.ScriptingEngine.Expose("debugUI",
                (Func<string>) (() =>
                {
                    DebugDraw = !DebugDraw;

                    return "UI debugging " + (DebugDraw ? "enabled." : "disabled.");
                }),
                "Enables the UI debugging. Showing the bounds of all UI controls.");
        }

        [Conditional("DEBUG")]
        private void DrawDebug(Renderer renderer)
        {
            if (!DebugDraw) return;

            Control top = GetTop(Context.InputManager.GetMousePosition());

            foreach (Control control in Controls)
            {
                renderer.RenderQueueOutline(control.GetTruePosition().LocationZ(control.Z + 1), control.Size, control.Active ? Color.Green : Color.Red);

                if (control == top)
                    renderer.RenderString(Context.AssetLoader.Get<Font>("debugFont.otf"), 10, control.GetType().ToString(), control.GetTruePosition().LocationZ(control.Z + 2), Color.Yellow);
            }

            renderer.RenderOutlineFlush();
        }

        public override string ToString()
        {
            string result = "[UI Controller " + Id + "]\n";

            foreach (Control control in Controls)
            {
                result += " |- " + control + "\n";
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
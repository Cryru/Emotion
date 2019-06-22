namespace Adfectus.Input
{
    /// <summary>
    /// Represents a game controller/joystick - loaded by an input manager.
    /// </summary>
    public abstract class Joystick
    {
        /// <summary>
        /// The id of the joystick.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The display friendly name of the joystick.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Whether the joystick is still connected.
        /// If it isn't the input results are kind of undefined, but will mostly boil down to
        /// false for buttons and 0 for axes.
        /// When created the joystick is thought of as connected.
        /// If the controller is invalid, this will also be set to false regardless of whether it is actually connected.
        /// </summary>
        public bool Connected { get; protected set; } = true;

        /// <summary>
        /// </summary>
        /// <param name="id">The id of the joystick.</param>
        /// <param name="name">The name of the joystick.</param>
        protected Joystick(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Whether the specified joystick button was pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>Whether the button was pressed.</returns>
        public abstract bool IsKeyDown(JoystickButton button);

        /// <summary>
        /// Whether the specified joystick button was let go.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>Whether the button was let go.</returns>
        public abstract bool IsKeyUp(JoystickButton button);

        /// <summary>
        /// Whether the specified joystick button is held.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>Whether the button is being held.</returns>
        public abstract bool IsKeyHeld(JoystickButton button);

        /// <summary>
        /// Get the value of the specified joystick axis.
        /// </summary>
        /// <param name="axis">The axis to poll.</param>
        /// <returns>The -1 to 1 value of the axis.</returns>
        public abstract float GetAxis(JoystickAxis axis);

        /// <summary>
        /// Get the value of the specified joystick axis, relative to the value in the last tick.
        /// Essentially the difference between the two - how the axis changed.
        /// </summary>
        /// <param name="axis">The axis to poll.</param>
        /// <returns>The -1 to 1 value of the axis.</returns>
        public abstract float GetAxisRelative(JoystickAxis axis);
    }
}
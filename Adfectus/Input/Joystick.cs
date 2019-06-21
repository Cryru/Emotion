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
        /// </summary>
        /// <param name="id">The id of the joystick.</param>
        /// <param name="name">The name of the joystick.</param>
        protected Joystick(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public abstract bool GetKeyDown(JoystickButton button);
        public abstract float GetAxis(JoystickAxis axis);
    }
}
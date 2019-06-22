namespace Adfectus.Input
{
    /// <summary>
    /// An enum of joystick buttons based on XInput.
    /// </summary>
    public enum JoystickButton
    {
        A = 0,
        B = 1,
        X = 2,
        Y = 3,
        LeftBumper = 4,
        RightBumper = 5,
        Back = 6,
        Start = 7,
        // Guide = 8, // It seems the "guide" button is not really reported. It if was - it would be here.
        LeftThumb = 8,
        RightThumb = 9,
        Up = 10,
        Right = 11,
        Down = 12,
        Left = 13
    }
}
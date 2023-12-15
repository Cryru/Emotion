namespace Emotion.Platform.Input
{
    public enum KeyStatus
    {
        Down,
        Up,

        MouseWheelScrollDown, // Used with Key.MouseWheel
        MouseWheelScrollUp, // Used with Key.MouseWheel

        [Obsolete("The frequency and behavior of held events depend on support from the platform." +
                  "\nInstead of using this key status consider a key that you have received DOWN for, but not UP as HELD")]
        Held
    }
}
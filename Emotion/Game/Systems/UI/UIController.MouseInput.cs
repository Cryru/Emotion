#nullable enable

namespace Emotion.Game.Systems.UI;

/// <summary>
/// Mouse focus is shared between all active controllers.
/// Active controllers are those which have called their update last tick.
/// </summary>
public partial class UIController
{
    /// <summary>
    /// The first non-input transparent visible window in any active controller. Can vary depending on window logic etc.
    /// </summary>
    public static UIBaseWindow? MouseFocus { get; private set; }

    public static UIRollover? CurrentRollover { get; private set; }

    private UIBaseWindow? _myMouseFocus; // The mouse focus of this controller in particular.

    public static void RemoveCurrentRollover()
    {
        CurrentRollover?.Close();
        CurrentRollover = null;
    }
}
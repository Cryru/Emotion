#nullable enable

namespace Emotion.Game.Systems.UI.Text.TextUpdate.LayoutEngineTypes;

public enum SpecialLayoutFlag : byte
{
    None,
    CenterStart,
    CenterContinue,
    RightStart,
    RightContinue,
}

public enum TextTagType
{
    None,
    ClosingTag,

    Color,
    Outline,
    Center,
    Right
}

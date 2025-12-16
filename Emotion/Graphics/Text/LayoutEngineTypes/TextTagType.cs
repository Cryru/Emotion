#nullable enable

using Emotion;

namespace Emotion.Graphics.Text.LayoutEngineTypes;

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

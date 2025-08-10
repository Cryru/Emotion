#nullable enable

namespace Emotion.Core.Platform.Implementation.Win32.Wgl;

public enum WglAttributes
{
    NumberPixelFormatsArb = 0x2000,
    SupportOpenGLArb = 0x2010,
    DrawToWindowArb = 0x2001,
    PixelTypeArb = 0x2013,
    AccelerationArb = 0x2003,
    RedBitsArb = 0x2015,
    RedShiftArb = 0x2016,
    GreenBitsArb = 0x2017,
    GreenShiftArb = 0x2018,
    BlueBitsArb = 0x2019,
    BlueShiftArb = 0x201a,
    AlphaBitsArb = 0x201b,
    AlphaShiftArb = 0x201c,
    AccumBitsArb = 0x201d,
    AccumRedBitsArb = 0x201e,
    AccumGreenBitsArb = 0x201f,
    AccumBlueBitsArb = 0x2020,
    AccumAlphaBitsArb = 0x2021,
    DepthBitsArb = 0x2022,
    StencilBitsArb = 0x2023,
    AuxBuffersArb = 0x2024,
    StereoArb = 0x2012,
    DoubleBufferArb = 0x2011,
    SamplesArb = 0x2042,
    FramebufferSrgbCapableArb = 0x20a9
}

public enum WglAttributeValues
{
    TypeRgbaArb = 0x202b,
    NoAccelerationArb = 0x2025
}
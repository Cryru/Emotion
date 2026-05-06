#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Graphics.Data;

[StructLayout(LayoutKind.Explicit, Size = SIZE_IN_BYTES)]
public struct BaseUniformData
{
    // Must match Common.h
    public const int SIZE_IN_BYTES = 196;
    public const int BINDING_LOCATION = 0;
    public const int MODEL_MATRIX_OFFSET = 128;

    [FieldOffset(0)] public Matrix4x4 ProjectionMatrix;
    [FieldOffset(64)] public Matrix4x4 ViewMatrix;
    [FieldOffset(MODEL_MATRIX_OFFSET)] public Matrix4x4 ModelMatrix;
    [FieldOffset(192)] public Vector4 ScreenResolution;

    public float Time { readonly get => ScreenResolution.W; set => ScreenResolution.W = value; }
}

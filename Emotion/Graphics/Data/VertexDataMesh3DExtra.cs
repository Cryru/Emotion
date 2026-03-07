#region Using

using System.Runtime.InteropServices;

#endregion

namespace Emotion.Graphics.Data;

[StructLayout(LayoutKind.Sequential)]
public struct VertexDataMesh3DExtra
{
    public static readonly int SizeInBytes = Marshal.SizeOf(new VertexDataMesh3DExtra());

    public Vector3 Normal;

    public override string ToString()
    {
        return $"Normal: {Normal}";
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Mesh3DVertexDataBones
{
    public static readonly int SizeInBytes = Marshal.SizeOf(new Mesh3DVertexDataBones());

    public Vector4 BoneIds;
    public Vector4 BoneWeights;

    public override string ToString()
    {
        return $"Ids: {BoneIds} // Weights: {BoneWeights}";
    }
}
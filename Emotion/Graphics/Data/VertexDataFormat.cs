#nullable enable

using System.Text;

namespace Emotion.WIPUpdates.Rendering;

public sealed class VertexDataFormat
{
    public static VertexDataFormat Default2D = new VertexDataFormat()
        .AddVertexPosition()
        .AddUV(1)
        .AddVertexColor()
        .Build();

    public bool Built { get; private set; }

    public int ElementSize { get; private set; }

    public bool HasPosition { get; private set; }

    public int HasUVCount { get; private set; }

    public bool HasNormals { get; private set; }

    public bool HasVertexColors { get; private set; }

    public bool HasBones { get; private set; }

    private const int POSITION_SIZE = sizeof(float) * 3; // Vector3
    private const int UV_SIZE = sizeof(float) * 2; // Vector2 (per UV)
    private const int NORMAL_SIZE = sizeof(float) * 3; // Vector3
    private const int COLOR_SIZE = sizeof(uint);
    private const int BONE_DATA_SIZE = sizeof(float) * 8; // VertexBoneData (Vector4 * 2)

    #region Build

    public VertexDataFormat AddVertexPosition()
    {
        if (!Built)
            HasPosition = true;
        return this;
    }

    public VertexDataFormat AddUV(int count)
    {
        if (!Built)
            HasUVCount += count;
        return this;
    }

    public VertexDataFormat AddNormal()
    {
        if (!Built)
            HasNormals = true;
        return this;
    }

    public VertexDataFormat AddVertexColor()
    {
        if (!Built)
            HasVertexColors = true;
        return this;
    }

    public VertexDataFormat AddBoneData()
    {
        if (!Built)
            HasBones = true;
        return this;
    }

    public VertexDataFormat Build()
    {
        if (Built) return this;

        int elementSize = 0;
        if (HasPosition) elementSize += POSITION_SIZE;
        if (HasUVCount > 0) elementSize += UV_SIZE * HasUVCount;
        if (HasNormals) elementSize += NORMAL_SIZE;
        if (HasVertexColors) elementSize += COLOR_SIZE;
        if (HasBones) elementSize += BONE_DATA_SIZE;
        ElementSize = elementSize;

        Built = true;
        return this;
    }

    #endregion

    #region Enum Helpers

    public void GetVertexPositionOffsetAndStride(out int offset, out int stride)
    {
        Assert(HasPosition);

        offset = 0;
        stride = ElementSize;
    }

    public void GetUVOffsetAndStride(int uvIndex, out int offset, out int stride)
    {
        Assert(HasUVCount > uvIndex);

        int byteOffset = 0;
        if (HasPosition) byteOffset += POSITION_SIZE;
        for (int i = 0; i < uvIndex; i++)
        {
            byteOffset += UV_SIZE;
        }

        offset = byteOffset;
        stride = ElementSize;
    }

    public void GetNormalOffsetAndStride(out int offset, out int stride)
    {
        Assert(HasNormals);

        int byteOffset = 0;
        if (HasPosition) byteOffset += POSITION_SIZE;
        for (int i = 0; i < HasUVCount; i++)
        {
            byteOffset += UV_SIZE;
        }

        offset = byteOffset;
        stride = ElementSize;
    }

    public void GetVertexColorsOffsetAndStride(out int offset, out int stride)
    {
        Assert(HasVertexColors);

        int byteOffset = 0;
        if (HasPosition) byteOffset += POSITION_SIZE;
        for (int i = 0; i < HasUVCount; i++)
        {
            byteOffset += UV_SIZE;
        }
        if (HasNormals) byteOffset += NORMAL_SIZE;

        offset = byteOffset;
        stride = ElementSize;
    }

    public void GetBoneDataOffsetAndStride(out int offset, out int stride)
    {
        Assert(HasBones);

        int byteOffset = 0;
        if (HasPosition) byteOffset += POSITION_SIZE;
        for (int i = 0; i < HasUVCount; i++)
        {
            byteOffset += UV_SIZE;
        }
        if (HasNormals) byteOffset += NORMAL_SIZE;
        if (HasVertexColors) byteOffset += COLOR_SIZE;

        offset = byteOffset;
        stride = ElementSize;
    }

    #endregion

    #region Shader Interop

    public void GetDescriptionForShader(StringBuilder b)
    {
        // $"VERTEX_ATTRIBUTE({nextLocation}, Vector3, vertPos)"

        int nextLocation = 0;
        if (HasPosition)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec3 vertPos");
            nextLocation++;
        }
        if (HasUVCount > 0)
        {
            for (int i = 0; i < HasUVCount; i++)
            {
                if (i == 0)
                    b.AppendLine($"layout(location = {nextLocation})in vec2 uv");
                else
                    b.AppendLine($"layout(location = {nextLocation})in vec2 uv_{i + 1}");

                nextLocation++;
            }
        }
        if (HasNormals)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec3 normal");
            nextLocation++;
        }
        if (HasVertexColors)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec4 color");
            nextLocation++;
        }
        if (HasBones)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec4 boneIds");
            nextLocation++;
            b.AppendLine($"layout(location = {nextLocation})in vec4 boneWeights");
            nextLocation++;
        }
    }

    #endregion
}
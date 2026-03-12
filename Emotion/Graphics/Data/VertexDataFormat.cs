#nullable enable

using System.Text;

namespace Emotion.Graphics.Data;

[DontSerialize]
public interface IVertexDataFormatStruct
{
    public static abstract VertexDataFormat Format { get; }
}

public enum VertexDataFormatAttributeType : int
{
    None,
    Position,
    UV,
    Normal,
    VertexColor,
    BoneIds,
    BoneWeights,

    CustomStart,
    Custom0,
    Custom1,
    Custom2,
    Custom3,
    Custom4
}

public sealed class VertexDataFormat : IEquatable<VertexDataFormat>
{
    public bool Built { get; private set; }

    public int ElementSize { get; private set; }

    public bool HasPosition { get; private set; }

    public int HasUVCount { get; private set; }

    public bool HasNormals { get; private set; }

    public bool HasVertexColors { get; private set; }

    public bool HasBones { get; private set; }

    public int Hash { get; private set; }

    private const int POSITION_SIZE = sizeof(float) * 3; // Vector3
    private const int UV_SIZE = sizeof(float) * 2; // Vector2 (per UV)
    private const int NORMAL_SIZE = sizeof(float) * 3; // Vector3
    private const int COLOR_SIZE = sizeof(uint);
    private const int BONE_DATA_SIZE = sizeof(float) * 8; // VertexBoneData (Vector4 * 2)

    public List<int>? CustomData;
    public int CustomDataByteOffset = -1;

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

    public VertexDataFormat AddCustomVector4()
    {
        if (!Built)
        {
            CustomData ??= new List<int>();
            CustomData.Add(4);
        }

        return this;
    }

    public VertexDataFormat Build()
    {
        if (Built) return this;

        int hash = 0;
        int elementSize = 0;
        if (HasPosition)
        {
            elementSize += POSITION_SIZE;
            hash += "Position".GetStableHashCode();
        }

        if (HasUVCount > 0)
        {
            elementSize += UV_SIZE * HasUVCount;

            int uvHash = "UV".GetStableHashCode();
            hash += uvHash * HasUVCount;
        }

        if (HasNormals)
        {
            elementSize += NORMAL_SIZE;
            hash += "Normal".GetStableHashCode();
        }

        if (HasVertexColors)
        {
            elementSize += COLOR_SIZE;
            hash += "Color".GetStableHashCode();
        }

        if (HasBones)
        {
            elementSize += BONE_DATA_SIZE;
            hash += "Bones".GetStableHashCode();
        }

        if (CustomData != null)
        {
            int customIdx = 1;
            CustomDataByteOffset = elementSize;
            foreach (int size in CustomData)
            {
                elementSize += sizeof(float) * size;

                hash += size * customIdx;
                customIdx++;
            }
        }

        ElementSize = elementSize;
        Hash = hash;

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

    #region Generic Helpers

    public bool AddAttribute(VertexDataFormatAttributeType type)
    {
        if (Built)
            return false;

        switch (type)
        {
            case VertexDataFormatAttributeType.Position:
                AddVertexPosition();
                return true;
            case VertexDataFormatAttributeType.UV:
                AddUV(1);
                return true;
            case VertexDataFormatAttributeType.VertexColor:
                AddVertexColor();
                return true;
            case VertexDataFormatAttributeType.Normal:
                AddNormal();
                return true;
            case VertexDataFormatAttributeType.BoneIds:
            case VertexDataFormatAttributeType.BoneWeights:
                AddBoneData();
                return true;
            default:
                {
                    if (type > VertexDataFormatAttributeType.CustomStart)
                    {
                        AddCustomVector4();
                        return true;
                    }
                    break;
                }
        }

        return false;
    }

    public bool HasAttribute(VertexDataFormatAttributeType type)
    {
        switch (type)
        {
            case VertexDataFormatAttributeType.Position:
                return HasPosition;
            case VertexDataFormatAttributeType.UV:
                return HasUVCount > 0;
            case VertexDataFormatAttributeType.VertexColor:
                return HasVertexColors;
            case VertexDataFormatAttributeType.Normal:
                return HasNormals;
            case VertexDataFormatAttributeType.BoneIds:
            case VertexDataFormatAttributeType.BoneWeights:
                return HasBones;
            default:
                {
                    if (type > VertexDataFormatAttributeType.CustomStart)
                    {
                        int customIdx = type - VertexDataFormatAttributeType.CustomStart;
                        return CustomData != null && CustomData.Count >= customIdx;
                    }
                    break;
                }
        }

        return false;
    }

    #endregion

    public override string ToString()
    {
        return $"FMT {{ [{Hash}] " +
            $"{(HasPosition ? "P" : "")}" +
            $"{(HasUVCount > 0 ? $"U({HasUVCount})" : "")}" +
            $"{(HasNormals ? $"N" : "")}" +
            $"{(HasVertexColors ? $"C" : "")}" +
            $"{(HasBones ? $"B" : "")}" +
            $"{(CustomData != null ? $"X({CustomData.Count})" : "")} }}";
    }

    public override int GetHashCode()
    {
        return Hash;
    }

    public bool Equals(VertexDataFormat? other)
    {
        if (other == null) return false;
        return other.Hash == Hash;
    }
}
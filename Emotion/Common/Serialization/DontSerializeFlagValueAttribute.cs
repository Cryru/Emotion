namespace Emotion.Common.Serialization;

/// <summary>
/// Allows you to specify flag values in [Flags] enums that you dont want serialized.
/// </summary>
public class DontSerializeFlagValueAttribute : Attribute
{
	public uint FlagsSkip;

	public DontSerializeFlagValueAttribute(uint flags)
	{
		FlagsSkip = flags;
	}

	public uint ClearDontSerialize(uint val)
	{
		return val & ~FlagsSkip;
	}
}
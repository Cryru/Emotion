#nullable enable

namespace Emotion.Standard.Reflector;

/// <summary>
/// Marker attribute for reflector to generate functions for a struct
/// that perform a per-member operation, like Equals or Clone.
/// </summary>
public class ReflectorGenerateStructPerMemberHelpersAttribute : Attribute
{
}

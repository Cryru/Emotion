#region Using

using Emotion.Common.Serialization;

#endregion

namespace Emotion.Game.World2D
{
	[Flags]
	public enum Map2DObjectFlags : uint
	{
		None = 0,

		UpdateWorldTree = 2 << 0, // The object moved or resized.
	}

	[Flags]
	[DontSerializeFlagValue((uint) Persistent)]
	public enum ObjectFlags : uint
	{
		None = 0,

		Persistent = 2 << 0, // The object is part of the map file.
		Map3DDontThrowShadow = 2 << 1, // This object will not be rendered in the shadow map, and will not throw shadow.
		Map3DDontReceiveShadow = 2 << 2, // This object will not blended with the rendered shadow map, and will now be shadowed by it.
		Map3DDontReceiveAmbient = 2 << 3 // This object will not be tinted by the ambient light.
	}

	public enum ObjectState : byte
	{
		None = 0,
		Loading = 1,
		Alive = 2,
		Destroyed = 3,

		// ShouldSpawnSerializedObject returned false
		// Will be checked again on map reset.
		ConditionallyNonSpawned = 4
	}

	[Flags]
	public enum QueryFlags : byte
	{
		None = 0,

		/// <summary>
		/// This will ensure that no object is present twice in the result. A single query would never
		/// add an object twice, but when chaining queries it could be useful to filter objects from the
		/// preexisting result set.
		/// </summary>
		Unique = 2 << 0
	}
}
#nullable enable

#region Using

using Emotion.Common.Serialization;

#endregion

namespace Emotion.Game.Animation3D;

public class SkeletonAnimRigRoot : SkeletonAnimRigNode
{
	public static SkeletonAnimRigRoot PromoteNode(SkeletonAnimRigNode node)
	{
		return new SkeletonAnimRigRoot
		{
			Name = node.Name,
			LocalTransform = node.LocalTransform,
			Children = node.Children
		};
	}
}

public class SkeletonAnimRigNode
{
	public string? Name { get; set; }
	public Matrix4x4 LocalTransform;
	public SkeletonAnimRigNode[]? Children;

	// Don't evaluate this node's local transform when applying animation channels.
	public bool DontAnimate;

	// For debugging purposes.
	private SkeletonAnimRigNode? FindInRig(string name)
	{
		if (Name == name) return this;
		if (Children == null) return null;

		for (var i = 0; i < Children.Length; i++)
		{
			SkeletonAnimRigNode child = Children[i];
			SkeletonAnimRigNode? found = child.FindInRig(name);
			if (found != null) return found;
		}

		return null;
	}

	public override string ToString()
	{
		return $"Bone: {Name}, Children: {Children?.Length}";
	}
}
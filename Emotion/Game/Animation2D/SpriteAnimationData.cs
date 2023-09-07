#region Using

using Emotion.Game.Animation;

#endregion

#nullable enable

namespace Emotion.Game.Animation2D
{
	public class SpriteAnimationData
	{
		/// <summary>
		/// The unique name of the animation.
		/// This is how the Controller will refer to it when indexing.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Describes the way the animation will loop once it reaches the final frame.
		/// </summary>
		public AnimationLoopType LoopType { get; set; } = AnimationLoopType.Normal;

		/// <summary>
		/// Frame indices into the frame source that comprise this animation.
		/// </summary>
		public int[] FrameIndices;

		/// <summary>
		/// The delay between frames in milliseconds.
		/// </summary>
		public int TimeBetweenFrames = 150;

		public SpriteAnimationData(string name, int frames)
		{
			Name = name;
			FrameIndices = new int[frames];
		}

		// Serialization constructor.
		protected SpriteAnimationData()
		{
			Name = null!;
			FrameIndices = null!;
		}
	}
}
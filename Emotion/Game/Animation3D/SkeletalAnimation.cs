#region Using

#endregion

namespace Emotion.Game.Animation3D
{
	public class SkeletalAnimation
	{
		/// <summary>
		/// The animation's name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Duration in milliseconds
		/// </summary>
		public float Duration { get; set; }

		/// <summary>
		/// Animation bone transformation that make up the animation.
		/// </summary>
		public SkeletonAnimChannel[] AnimChannels { get; set; }

		// todo: cache this
		public SkeletonAnimChannel GetMeshAnimBone(string name)
		{
			for (var i = 0; i < AnimChannels.Length; i++)
			{
				SkeletonAnimChannel channels = AnimChannels[i];
				if (channels.Name == name) return channels;
			}

			return null;
		}

        public override string ToString()
        {
            return $"Animation {Name} [{Duration}ms]";
        }
    }
}
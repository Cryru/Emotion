#region Using

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// Used for switching between animations in one animated texture.
    /// </summary>
    public class AnimationController
    {
        /// <summary>
        /// The currently playing node.
        /// </summary>
        [DontSerialize]
        public AnimationNode CurrentAnimation;

        /// <summary>
        /// The animated texture in which all animations are stored.
        /// </summary>
        public AnimatedTexture AnimTex;

        /// <summary>
        /// Possible animations.
        /// </summary>
        public Dictionary<string, AnimationNode> Animations = new Dictionary<string, AnimationNode>();

        public AnimationController(AnimatedTexture animTex)
        {
            AnimTex = animTex;
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        protected AnimationController()
        {

        }

        /// <summary>
        /// Add a new animation.
        /// </summary>
        /// <param name="n">The animation node to add.</param>
        public void AddAnimation(AnimationNode n)
        {
            Animations.Add(n.Name, n);
        }

        /// <summary>
        /// Remove an animation.
        /// </summary>
        /// <param name="name">The name of the animation to remove.</param>
        public void RemoveAnimation(string name)
        {
            Animations.Remove(name);
        }

        /// <summary>
        /// Set the current animation.
        /// </summary>
        /// <param name="animName"></param>
        public void SetAnimation(string animName)
        {
            if (!HasAnimation(animName))
            {
                Engine.Log.Warning($"Animation {animName} not found.", MessageSource.Anim);
                return;
            }

            AnimationNode n = Animations[animName];
            AnimTex.StartingFrame = n.StartingFrame;
            AnimTex.EndingFrame = n.EndingFrame;
            AnimTex.LoopType = n.LoopType;
            AnimTex.TimeBetweenFrames = n.TimeBetweenFrames;

            CurrentAnimation = n;
        }

        /// <summary>
        /// Check if the controller contains the requested animation.
        /// </summary>
        /// <param name="animName"></param>
        /// <returns>Whether the controller has this animation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnimation(string animName)
        {
            return Animations.ContainsKey(animName);
        }

        /// <summary>
        /// Re-index animation nodes.
        /// </summary>
        public void Reindex()
        {
            AnimationNode[] nodeData = Animations.Values.ToArray();
            Animations.Clear();
            foreach (AnimationNode node in nodeData)
            {
                Animations.Add(node.Name, node);
            }
        }
    }
}
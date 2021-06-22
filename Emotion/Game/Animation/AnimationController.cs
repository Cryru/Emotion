#region Using

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.Logging;

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
        [DontSerialize] public AnimationNode CurrentAnimation;

        /// <summary>
        /// The animated texture in which all animations are stored.
        /// </summary>
        [DontSerializeMembers("LoopType", "StartingFrame", "EndingFrame", "TimeBetweenFrames", "CurrentFrameIndex")] // These properties are provided by the current animation node.
        public AnimatedTexture AnimTex;

        /// <summary>
        /// Anchors in case the texture needs to be mirrored along the X axis.
        /// Optionally specified.
        /// </summary>
        public Vector2[] MirrorXAnchors { get; set; }

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
        /// <param name="force">Set the animation even if it the same as the current animation. This will reset it.</param>
        public void SetAnimation(string animName, bool force = false)
        {
            if (!force && CurrentAnimation != null && CurrentAnimation.Name == animName) return;

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
            AnimTex.Reset();

            CurrentAnimation = n;
        }

        /// <summary>
        /// Get the data needed to draw the current frame.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="uv"></param>
        /// <param name="anchor"></param>
        /// <param name="xMirror"></param>
        public void GetCurrentFrameData(out Texture image, out Rectangle uv, out Vector2 anchor, bool xMirror = false)
        {
            image = null;
            uv = Rectangle.Empty;
            anchor = Vector2.Zero;
            if (CurrentAnimation == null) return;

            uv = AnimTex.CurrentFrame;
            int frameIdx = AnimTex.CurrentFrameIndex;
            if (xMirror && MirrorXAnchors != null && MirrorXAnchors.Length > frameIdx)
                anchor = MirrorXAnchors[frameIdx];
            else
                anchor = AnimTex.Anchors[frameIdx];
            image = AnimTex.Texture;
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
                if (Animations.ContainsKey(node.Name)) node.Name += "_";
                Animations.Add(node.Name, node);
            }
        }

        public AnimationController Copy()
        {
            return new AnimationController
            {
                Animations = Animations,
                AnimTex = AnimTex.Copy(),
                CurrentAnimation = CurrentAnimation,
                MirrorXAnchors = MirrorXAnchors
            };
        }
    }
}
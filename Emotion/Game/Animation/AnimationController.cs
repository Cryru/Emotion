#region Using

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Emotion.Game.Animation
{
    /// <summary>
    /// Used for switching between animations in one animated texture.
    /// </summary>
    public class AnimationController
    {
        /// <summary>
        /// A single animation node.
        /// </summary>
        public class Node
        {
            public string Name { get; set; }
            public int StartingFrame { get; set; } = 0;
            public int EndingFrame { get; set; } = -1;
            public int TimeBetweenFrames { get; set; } = 500;
            public AnimationLoopType LoopType { get; set; } = AnimationLoopType.Normal;

            public Node(string name)
            {
                Name = name;
            }
        }

        /// <summary>
        /// List of possible animations.
        /// </summary>
        public Node[] Animations
        {
            get => _nodes.Values.ToArray();
        }

        /// <summary>
        /// The currently playing node.
        /// </summary>
        public Node CurrentAnimation;

        /// <summary>
        /// The animated texture in which all animations are stored.
        /// </summary>
        public AnimatedTexture AnimTex;

        /// <summary>
        /// Possible animations.
        /// </summary>
        private Dictionary<string, Node> _nodes = new Dictionary<string, Node>();

        public AnimationController(AnimatedTexture animTex)
        {
            AnimTex = animTex;
        }

        /// <summary>
        /// Add a new animation.
        /// </summary>
        /// <param name="n">The animation node to add.</param>
        public void AddAnimation(Node n)
        {
            _nodes.Add(n.Name, n);
        }

        /// <summary>
        /// Remove an animation.
        /// </summary>
        /// <param name="name">The name of the animation to remove.</param>
        public void RemoveAnimation(string name)
        {
            _nodes.Remove(name);
        }

        /// <summary>
        /// Set the current animation.
        /// </summary>
        /// <param name="animName"></param>
        public void SetAnimation(string animName)
        {
            Node n = _nodes[animName];
            AnimTex.StartingFrame = n.StartingFrame;
            AnimTex.EndingFrame = n.EndingFrame;
            AnimTex.LoopType = n.LoopType;
            AnimTex.TimeBetweenFrames = n.TimeBetweenFrames;

            CurrentAnimation = n;
        }

        /// <summary>
        /// Returns a serializable animation controller description file.
        /// </summary>
        /// <param name="textureName">The spritesheet texture's name within the asset loader.</param>
        /// <returns>A serializable animation controller description file.</returns>
        public AnimationControllerDescription GetDescription(string textureName = null)
        {
            return new AnimationControllerDescription()
            {
                AnimTex = AnimTex.GetDescription(textureName),
                Nodes = _nodes.Values.ToList()
            };
        }
    }
}
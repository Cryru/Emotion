#region Using

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Emotion.Common;
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
        /// A single animation node.
        /// </summary>
        public class Node
        {
            public string Name { get; set; }
            public int StartingFrame { get; set; } = 0;
            public int EndingFrame { get; set; } = -1;
            public int TimeBetweenFrames { get; set; } = 500;
            public AnimationLoopType LoopType { get; set; } = AnimationLoopType.Normal;

            public Node()
            {
            }

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
            if (!HasAnimation(animName))
            {
                Engine.Log.Warning($"Animation {animName} not found.", MessageSource.Anim);
                return;
            }

            Node n = _nodes[animName];
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
            return _nodes.ContainsKey(animName);
        }

        /// <summary>
        /// Re-index animation nodes.
        /// </summary>
        public void Reindex()
        {
            Node[] nodeData = _nodes.Values.ToArray();
            _nodes.Clear();
            foreach (Node node in nodeData)
            {
                _nodes.Add(node.Name, node);
            }
        }

        /// <summary>
        /// Returns a serializable animation controller description file.
        /// </summary>
        /// <param name="textureName">The spritesheet texture's name within the asset loader.</param>
        /// <returns>A serializable animation controller description file.</returns>
        public AnimationControllerDescription GetDescription(string textureName = null)
        {
            return new AnimationControllerDescription
            {
                AnimTex = AnimTex.GetDescription(textureName),
                Nodes = _nodes.Values.ToList()
            };
        }
    }
}
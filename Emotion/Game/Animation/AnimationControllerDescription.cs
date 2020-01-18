#region Using

using System.Collections.Generic;

#endregion

namespace Emotion.Game.Animation
{
    public class AnimationControllerDescription
    {
        public List<AnimationController.Node> Nodes { get; set; } = new List<AnimationController.Node>();
        public AnimatedTextureDescription AnimTex { get; set; }

        /// <summary>
        /// Create an animation controller from its description.
        /// </summary>
        public AnimationController CreateFrom()
        {
            AnimatedTexture animTex = AnimTex.CreateFrom();

            var anim = new AnimationController(animTex);
            foreach (AnimationController.Node n in Nodes)
            {
                anim.AddAnimation(n);
            }
            return anim;
        }
    }
}
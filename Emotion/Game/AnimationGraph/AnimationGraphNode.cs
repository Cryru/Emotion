#region Using

using System.Collections.Generic;
using Emotion.Game.Animation2D;

#endregion

#nullable enable

namespace Emotion.Game.AnimationGraph
{
    public class AnimationGraphNode
    {
        public IAnimationGraphVariable[]? Requirements;
        public List<AnimationGraphNode>? Children;
        public string? Animation;

        public string? Prompt(AnimationGraphController controller)
        {
            if (Children != null)
                for (var i = 0; i < Children.Count; i++)
                {
                    AnimationGraphNode child = Children[i];
                    if (child.RequirementsMet(controller))
                    {
                        string? anim = child.Prompt(controller);
                        if (anim != null) return anim;
                    }
                }

            return Animation;
        }

        public bool RequirementsMet(AnimationGraphController controller)
        {
            if (Requirements == null) return true;

            Dictionary<string, IAnimationGraphVariable> state = controller.State;
            for (var i = 0; i < Requirements.Length; i++)
            {
                IAnimationGraphVariable req = Requirements[i];
                if (!state.TryGetValue(req.Name, out IAnimationGraphVariable? stateVar)) continue;
                if (stateVar != req) return false;
            }

            return true;
        }
    }
}
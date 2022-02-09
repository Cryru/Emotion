#region Using

using System.Collections.Generic;

#endregion

namespace Emotion.Game.AnimationGraph
{
    public class AnimationGraphData
    {
        public AnimationGraphNode TopLevel;
        public Dictionary<string, IAnimationGraphVariable> InitialState;
    }
}
#region Using

using System.Collections.Generic;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

#nullable enable

namespace Emotion.Game.AnimationGraph
{
    public class AnimationGraphController
    {
        public Dictionary<string, IAnimationGraphVariable> State;

        public string? CurrentAnimation
        {
            get
            {
                if (_animationNeedsUpdate) _currentAnimation = _data.TopLevel.Prompt(this);
                _animationNeedsUpdate = false;
                return _currentAnimation;
            }
        }

        private AnimationGraphData _data;
        private string? _currentAnimation;
        private bool _animationNeedsUpdate = true;

        public AnimationGraphController(AnimationGraphData data)
        {
            State = new Dictionary<string, IAnimationGraphVariable>();
            _data = data;

            foreach ((string? name, IAnimationGraphVariable? graphVar) in _data.InitialState)
            {
                State.Add(name, graphVar.Clone());
            }
        }

        public void UpdateState(string variable, object value)
        {
            if (!State.TryGetValue(variable, out IAnimationGraphVariable? graphVar))
            {
                Engine.Log.Warning($"Tried setting state variable {variable} to animation graph that doesn't contain it.", MessageSource.Game, true);
                return;
            }

            if (graphVar.SetValue(value)) _animationNeedsUpdate = true;
        }
    }
}
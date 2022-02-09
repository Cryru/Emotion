#nullable enable

namespace Emotion.Game.AnimationGraph
{
    public interface IAnimationGraphVariable
    {
        public string Name { get; set; }
        public IAnimationGraphVariable Clone();
        public bool SetValue(object value);
    }
}
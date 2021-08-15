#region Using

using Emotion.Primitives;

#endregion

namespace Emotion.Game.Pathing
{
    public enum PathingActorType
    {
        Static,
        Dynamic
    }

    public class PathingActor : Transform
    {
        public string Identifier;
        public PathingActorType Type = PathingActorType.Static;

        // serialization constructor
        protected PathingActor()
        {

        }

        public PathingActor(string id)
        {
            Identifier = id;
        }
    }
}
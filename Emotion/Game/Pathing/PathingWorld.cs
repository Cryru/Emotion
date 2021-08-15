#region Using

using System.Collections.Generic;
using System.Numerics;

#endregion

namespace Emotion.Game.Pathing
{
    public class PathingWorld
    {
        public Vector2 Size;
        public List<PathingActor> Actors;

        // serialization constructor
        protected PathingWorld()
        {

        }

        public PathingWorld(Vector2 size)
        {
            Size = size;
            Actors = new List<PathingActor>();
        }
    }
}
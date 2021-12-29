#region Using

using System.Collections.Generic;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.QuadTree
{
    public class QuadTreeQuery<T>
    {
        public IShape SearchArea;
        public List<T> Results;
    }
}
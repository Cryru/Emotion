#nullable enable

using Emotion.Game.Systems.CollisionSystem;

namespace Emotion.Primitives.DataStructures.QuadTree;

public class QuadTreeQuery<T>
{
    public IShape SearchArea;
    public List<T> Results;
}
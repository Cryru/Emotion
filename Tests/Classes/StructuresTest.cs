#region Using

using System.Collections.Generic;
using Emotion.Game.QuadTree;
using Emotion.Primitives;
using Emotion.Test;

#endregion

namespace Tests.Classes
{
    /// <summary>
    /// Test connected to data structures.
    /// </summary>
    [Test("Scripting", true)]
    public class StructuresTest
    {
        [Test]
        public void QuadTreeTest()
        {
            var tree = new QuadTree<Transform>(new Rectangle(0, 0, 100, 100));

            // There shouldn't be anything inside.
            Assert.True(tree.EmptyLeaf);
            Assert.Equal(0, tree.Count);

            // Add a hundred objects in the top left.
            // None of these go beyond coordinate 50.
            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < 10; y++)
                {
                    tree.Add(new Transform(x * 5, y * 5, 0, 5, 5));
                }
            }

            Assert.Equal(100, tree.Count);
            Assert.Equal(100, tree.TopLeftChild.Count);

            // Top right should exist since the root node divided - but should be empty.
            Assert.True(tree.TopRightChild.EmptyLeaf);
            Assert.True(tree.BottomLeftChild.EmptyLeaf);
            Assert.True(tree.BottomRightChild.EmptyLeaf);

            // Check if the transform moving is detected as expected.
            tree.GetAllObjects()[0].X += 50;

            Assert.False(tree.TopRightChild.EmptyLeaf);
            Assert.Equal(1, tree.TopRightChild.Count);

            // Check if querying works.
            List<Transform> query = tree.GetObjects(new Rectangle(0, 0, 50, 50));
            Assert.Equal(99, query.Count);
        }
    }
}
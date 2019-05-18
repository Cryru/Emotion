#region Using

using System.Collections.Generic;
using Adfectus.Game.QuadTree;
using Adfectus.Primitives;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected to the QuadTree implementation.
    /// </summary>
    [Collection("main")]
    public class QuadTreeTests
    {

        [Fact]
        public void TreeTest()
        {
            QuadTree<Transform> tree = new QuadTree<Transform>(0, 0, 100, 100);

            // There shouldn't be anything inside.
            Assert.True(tree.QuadTreeRoot.isEmptyLeaf);
            Assert.Equal(0, tree.QuadTreeRoot.Count);

            // Add a hundred objects in the top left.
            // None of these go beyond coordinate 50.
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    tree.Add(new Transform(x * 5, y * 5, 0, 5, 5));
                }
            }

            Assert.Equal(100, tree.Count);
            Assert.Equal(100, tree.QuadTreeRoot.TopLeftChild.Count);

            // Top right should exist since the root node divided - but should be empty.
            Assert.True(tree.QuadTreeRoot.TopRightChild.isEmptyLeaf);
            Assert.True(tree.QuadTreeRoot.BottomLeftChild.isEmptyLeaf);
            Assert.True(tree.QuadTreeRoot.BottomRightChild.isEmptyLeaf);

            // Check if the transform moving is detected as expected.
            tree[0].X += 50;

            Assert.False(tree.QuadTreeRoot.TopRightChild.isEmptyLeaf);
            Assert.Equal(1, tree.QuadTreeRoot.TopRightChild.Count);

            // Check if querying works.
            List<Transform> query = tree.GetObjects(new Rectangle(0, 0, 50, 50));
            Assert.Equal(99, query.Count);
        }
    }
}
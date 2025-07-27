using Emotion.Testing;
using System.Collections.Generic;
using Emotion.ThirdParty.TopologicalSort;

namespace Tests.EngineTests;

[Test]
public class MiscTest
{
    [Test]
    public void TopologicalSortTest()
    {
        var data = new List<int>()
        {
            7, 5, 3, 8, 11, 2, 9, 10
        };

        var dependencies = new List<(int, int)>()
        {
            (7, 11),
            (7, 8),
            (3, 10),
            (11, 2),
            (11, 9),
        };

        // We need to copy it as the sort modifies the list
        var dependenciesCopy = new List<(int, int)>();
        dependenciesCopy.AddRange(dependencies);

        List<int> ret = TopologicalSort.Sort(data, dependencies);

        for (int i = 0; i < dependenciesCopy.Count; i++)
        {
            (int dependingOn, int node) = dependenciesCopy[i];

            int indexOfNode = ret.IndexOf(node);
            int indexOfDependingOn = ret.IndexOf(dependingOn);

            Assert.True(indexOfNode != -1);
            Assert.True(indexOfDependingOn != -1);
            Assert.True(indexOfNode > indexOfDependingOn);
        }
    }
}

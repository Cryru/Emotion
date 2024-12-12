#region Using

using System.Numerics;
using BenchmarkDotNet.Attributes;
using Emotion.Common;
using Emotion.Game.World;
using Emotion.Game.World2D;
using Emotion.Primitives;

using Emotion.Testing;

#endregion

namespace Emotion.Benchmark;

[MemoryDiagnoser]
public class ObjectQueryBenchmark
{
    private Map2D _map;

    private List<TestInheritedObject> _list = new List<TestInheritedObject>();
    private List<BaseGameObject> _listBase = new List<BaseGameObject>();

    public class TestInheritedObject : GameObject2D
    {
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        Configurator config = new Configurator();
        config.HiddenWindow = true;
        Engine.StartHeadless(config);

        _map = new Map2D(new Vector2(1000, 1000));
        _map.InitAsync().Wait();

        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                if (x * y % 2 == 0)
                {
                    _map.AddObject(new GameObject2D("")
                    {
                        Position = new Vector3(x, y, 0),
                        Size = new Vector2(1, 1)
                    });
                    ;
                }
                else
                {
                    _map.AddObject(new TestInheritedObject
                    {
                        Position = new Vector3(x, y, 0),
                        Size = new Vector2(1, 1)
                    });
                }
            }
        }
    }
    // | Method                         | Mean     | Error    | StdDev   | Allocated |


    // | NewQueryObjectsAllLiving       | 78.51 us | 0.819 us | 0.766 us |         - |
    [Benchmark]
    public void NewQueryObjectsAllLiving()
    {
        _map.ObjectsGet(_listBase);
        Assert.Equal(_listBase.Count, 100 * 100);
    }

    // | NewQueryObjectsAllLivingAsEnum | 38.35 us | 0.339 us | 0.317 us |         - |
    [Benchmark]
    public void NewQueryObjectsAllLivingAsEnum()
    {
        int counter = 0;
        foreach (var obj in _map.ObjectsEnum())
        {
            counter++;
        }

        Assert.Equal(counter, 100 * 100);
    }

    // | OldQueryObjectsAllLivingAsEnum | 58.67 us | 0.458 us | 0.382 us |      48 B |
    [Benchmark]
    public void OldQueryObjectsAllLivingAsEnum()
    {
        int counter = 0;
        foreach (var obj in _map.GetObjects())
        {
            counter++;
        }

        Assert.Equal(counter, 100 * 100);
    }

    // | NewQueryObjectsAllOfType       | 73.43 us | 0.417 us | 0.348 us |         - |
    [Benchmark]
    public void NewQueryObjectsAllOfType()
    {
        _map.ObjectsGet(_list);
        Assert.Equal(_list.Count, 2500);
    }

    // | NewQueryObjectsAllOfTypeAsEnum | 61.54 us | 0.558 us | 0.522 us |         - |
    [Benchmark]
    public void NewQueryObjectsAllOfTypeAsEnum()
    {
        int counter = 0;
        foreach (var obj in _map.ObjectsEnum<TestInheritedObject>())
        {
            counter++;
        }

        Assert.Equal(counter, 2500);
    }

    [Benchmark]
    public void NewQueryObjectsAllOfTypeInShapeAsEnum()
    {
        int counter = 0;
        foreach (var obj in _map.ObjectsEnum<TestInheritedObject, Circle>(new Circle(new Vector2(50, 50), 10)))
        {
            counter++;
        }

        Assert.Equal(counter, 86);
    }

    // | OldQueryObjectsAllOfType       | 62.48 us | 0.530 us | 0.442 us |         - |
    [Benchmark]
    public void OldQueryObjectsAllOfType()
    {
        _list.Clear();
        _map.GetObjectsByType(_list);
        Assert.Equal(_list.Count, 2500);
    }

    // | NewQueryObjectsInShape         | 34.40 us | 0.309 us | 0.289 us |         - |
    [Benchmark]
    public void NewQueryObjectsInShape()
    {
        _map.ObjectsGet(_listBase, new Circle(new Vector2(50, 50), 10));
        Assert.Equal(_listBase.Count, 344);
    }

    // | OldQueryObjectsInShape         | 35.26 us | 0.202 us | 0.189 us |      96 B |
    [Benchmark]
    public void OldQueryObjectsInShape()
    {
        _listBase.Clear();
        _map.GetObjects(_listBase, 0, new Circle(new Vector2(50, 50), 10));
        Assert.Equal(_listBase.Count, 344);
    }
}
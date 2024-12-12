using BenchmarkDotNet.Attributes;
using Emotion.Audio;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Benchmark;

[MemoryDiagnoser]
public class MiscBenchmark
{
    private int[] _numberArrayVerySmall = new int[2];
    private int[] _numberArraySmall = new int[16];
    private int[] _numberArray = new int[1024];
    private int[]? _numberArrayNull = null;

    [GlobalSetup]
    public void GlobalSetup()
    {
        for (int i = 0; i < _numberArrayVerySmall.Length; i++)
        {
            _numberArrayVerySmall[i] = 1337;
        }

        for (int i = 0; i < _numberArraySmall.Length; i++)
        {
            _numberArraySmall[i] = 1337;
        }

        for (int i = 0; i < _numberArray.Length; i++)
        {
            _numberArray[i] = 1337;
        }
    }

    [Benchmark]
    public int IterateArrayVerySmallNormal()
    {
        int sum = 0;
        foreach (int num in _numberArrayVerySmall)
        {
            sum += num;
        }
        return sum;
    }

    [Benchmark]
    public int IterateArrayVerySmallHelper()
    {
        int sum = 0;
        foreach (int num in Helpers.SafeForEachArray(_numberArrayVerySmall))
        {
            sum += num;
        }
        return sum;
    }

    [Benchmark]
    public int IterateArraySmallNormal()
    {
        int sum = 0;
        foreach (int num in _numberArraySmall)
        {
            sum += num;
        }
        return sum;
    }

    [Benchmark]
    public int IterateArraySmallHelper()
    {
        int sum = 0;
        foreach (int num in Helpers.SafeForEachArray(_numberArraySmall))
        {
            sum += num;
        }
        return sum;
    }

    [Benchmark]
    public int IterateArrayNormal()
    {
        int sum = 0;
        foreach (int num in _numberArray)
        {
            sum += num;
        }
        return sum;
    }

    [Benchmark]
    public int IterateArrayHelper()
    {
        int sum = 0;
        foreach (int num in Helpers.SafeForEachArray(_numberArray))
        {
            sum += num;
        }
        return sum;
    }

    [Benchmark]
    public int IterateArrayNullNormal()
    {
        int sum = 0;
        if (_numberArrayNull != null)
            foreach (int num in _numberArrayNull)
            {
                sum += num;
            }

        return sum;
    }

    [Benchmark]
    public int IterateArrayNullHelper()
    {
        int sum = 0;
        foreach (int num in Helpers.SafeForEachArray(_numberArrayNull))
        {
            sum += num;
        }

        return sum;
    }
}

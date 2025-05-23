﻿#nullable enable

namespace System;

public static class RandomExtensions
{
    public static int NextInclusive(this Random rng, int min, int max)
    {
        return rng.Next(min, max + 1);
    }
}
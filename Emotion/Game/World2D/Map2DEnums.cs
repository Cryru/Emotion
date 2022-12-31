#region Using

using System;

#endregion

namespace Emotion.Game.World2D
{
    [Flags]
    public enum Map2DObjectFlags : uint
    {
        None = 0,

        UpdateWorldTree = 2 << 0, // The object moved or resized.
    }

    [Flags]
    public enum ObjectFlags : uint
    {
        None = 0,

        Serializable = 2 << 0 // The object is part of the map file.
    }

    public enum ObjectState : byte
    {
        None = 0,
        Loading = 1,
        Alive = 2,
        Destroyed = 3
    }

    [Flags]
    public enum QueryFlags : byte
    {
        None = 0,
        Unique = 2 << 0
    }
}
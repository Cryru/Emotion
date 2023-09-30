#nullable enable

#region Using

using Emotion.Game.World.SceneControl;

#endregion

namespace Emotion.Game.World2D.SceneControl;

/// <inheritdoc />
public interface IWorld2DAwareScene<T> : IWorldAwareScene where T : Map2D
{
}
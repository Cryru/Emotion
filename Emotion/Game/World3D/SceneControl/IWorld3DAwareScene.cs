#nullable enable

#region Using

using Emotion.Game.World.SceneControl;

#endregion

namespace Emotion.Game.World3D.SceneControl;

/// <inheritdoc />
public interface IWorld3DAwareScene<T> : IWorldAwareScene where T : Map3D
{
}
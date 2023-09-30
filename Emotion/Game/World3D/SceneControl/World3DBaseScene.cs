#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World.SceneControl;
using Emotion.Game.World3D.Editor;

#endregion

namespace Emotion.Game.World3D.SceneControl;

public abstract class World3DBaseScene<T> : WorldBaseScene<T>, IWorld3DAwareScene<T> where T : Map3D
{
	protected override WorldBaseEditor CreateEditor()
	{
		return new World3DEditor(this, typeof(T));
	}
}
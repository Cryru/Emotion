#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World.SceneControl;
using Emotion.Game.World2D.Editor;

#endregion

namespace Emotion.Game.World2D.SceneControl;

public abstract class World2DBaseScene<T> : WorldBaseScene<Map2D>, IWorld2DAwareScene where T : Map2D
{
	protected override WorldBaseEditor CreateEditor()
	{
		return new World2DEditor(this, typeof(T));
	}
}
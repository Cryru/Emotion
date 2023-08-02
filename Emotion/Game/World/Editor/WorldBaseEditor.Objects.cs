#nullable enable

#region Using

using Emotion.Game.World2D;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Game.World.Editor;

public abstract partial class WorldBaseEditor
{
	#region Internal API

	public abstract void ChangeObjectProperty(GameObject2D obj, XMLFieldHandler field, object? value, bool recordUndo = true);

	#endregion
}
#nullable enable

#region Using

using Emotion.Game.ThreeDee.Editor;
using Emotion.Game.World;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World3D;

public class Map3D : BaseMap
{
	public LightModel LightModel = new();

	public override List<Type> GetValidObjectTypes()
	{
		var types = EditorUtility.GetTypesWhichInherit<GameObject3D>();

		// Editor only
		types.Remove(typeof(TranslationGizmo));
		types.Remove(typeof(InfiniteGrid));

		return types;
	}

	public override void Render(RenderComposer c)
	{
		if (!Initialized) return;

		var renderObjectsList = new List<BaseGameObject>();
		GetObjects(renderObjectsList, 0);
		for (var i = 0; i < renderObjectsList.Count; i++)
		{
			BaseGameObject obj = renderObjectsList[i];
			obj.Render(c);
		}
	}
}
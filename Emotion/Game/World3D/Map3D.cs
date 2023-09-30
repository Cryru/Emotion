#nullable enable

#region Using

using Emotion.Game.World;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World3D;

public class Map3D : BaseMap
{
	public override List<Type> GetValidObjectTypes()
	{
		return EditorUtility.GetTypesWhichInherit<GameObject3D>();
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
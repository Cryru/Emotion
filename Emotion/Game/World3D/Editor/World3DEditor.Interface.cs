#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.UI;

#endregion

namespace Emotion.Game.World3D.Editor;

public partial class World3DEditor
{
	protected override void EditorAttachTopBarButtons(UIBaseWindow parentList)
	{
		base.EditorAttachTopBarButtons(parentList);

		Map3D? map = CurrentMap;
		EditorButton menu3D = EditorDropDownButton("3D", new[]
		{
			// todo: when the property panel can edit sub objects this will be accessible via map->properties
			new EditorDropDownButtonDescription
			{
				Name = "Light Model",
				Click = (_, __) =>
				{
					AssertNotNull(map);
					var panel = new GenericPropertiesEditorPanel(map.LightModel);
					_editUI!.AddChild(panel);
				},
				Enabled = () => map != null
			},
		});

		parentList.AddChild(menu3D);
	}
}
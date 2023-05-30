#region Using

using System.Linq;
using System.Reflection;
using System.Text;
using Emotion.Common.Serialization;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D.Editor;

public sealed class MapEditorObjectPropertiesPanel : GenericPropertiesEditorPanel
{
	public World2DEditor Editor;

	public GameObject2D Object;
	public int ObjectUId;
	public Map2D ObjectMap;

	public MapEditorObjectPropertiesPanel(World2DEditor editor, GameObject2D obj) : base(obj)
	{
		Editor = editor;
		Object = obj;
		ObjectUId = Object.UniqueId;
		ObjectMap = Object.Map;

		// Inject the "Center" property of Transform.
		// Since it isn't serialized it wont be returned, but its handy to have for objects.
		// todo: Make this injection function generic? In the base class or w/e.
		EditorUtility.TypeAndFieldHandlers? positionalType = _fields.FirstOrDefault(x => x.DeclaringType == typeof(Transform));
		if (positionalType != null)
		{
			XMLTypeHandler? v2Handler = XMLHelpers.GetTypeHandler(typeof(Vector2));
			PropertyInfo? centerProp = typeof(Transform).GetProperty("Center");
			if (centerProp != null) positionalType.Fields.Add(new XMLFieldHandler(new ReflectedMemberHandler(centerProp), v2Handler));
		}
	}

	protected override void AddHeaderUI(UIBaseWindow uiContainer)
	{
		var statusLabel = new UIText();
		statusLabel.ScaleMode = UIScaleMode.FloatScale;
		statusLabel.WindowColor = MapEditorColorPalette.TextColor;
		statusLabel.FontFile = "Editor/UbuntuMono-Regular.ttf";
		statusLabel.FontSize = MapEditorColorPalette.EditorButtonTextSize;

		var metaText = new StringBuilder();
		metaText.Append(Object.ObjectState.ToString());
		ObjectFlags[] objectFlags = Enum.GetValues<ObjectFlags>();
		for (var i = 0; i < objectFlags.Length; i++)
		{
			ObjectFlags flag = objectFlags[i];
			if (flag == ObjectFlags.None) continue; // All have this :P
			if (Object.ObjectFlags.HasFlag(flag)) metaText.Append($", {flag}");
		}

		// Warning: These objects might not actually be in these layers.
		// If they reported a different value to IsPartOfMapLayer when being added.
		metaText.Append("\nIn Layers: ");
		var idx = 0;
		foreach (int treeLayerId in ObjectMap.GetWorldTree()!.ForEachLayer())
		{
			if (Object.IsPartOfMapLayer(treeLayerId))
			{
				if (idx != 0) metaText.Append(", ");
				metaText.Append(treeLayerId);
				idx++;
			}
		}

		statusLabel.Text = metaText.ToString();

		uiContainer.AddChild(statusLabel);
	}

	protected override void ApplyObjectChange(XMLFieldHandler field, object value)
	{
		Editor.ChangeObjectProperty(Object, field, value);
	}
}
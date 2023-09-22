#nullable enable

#region Using

using System.Linq;
using System.Reflection;
using System.Text;
using Emotion.Common.Serialization;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.PropertyEditors;
using Emotion.Game.World2D;
using Emotion.Game.World2D.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World3D;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.UI;

#endregion

namespace Emotion.Game.World.Editor;

public sealed class MapEditorObjectPropertiesPanel : GenericPropertiesEditorPanel
{
	public WorldBaseEditor Editor;

	public BaseGameObject Object;
	public int ObjectUId;
	public BaseMap ObjectMap;

	public MapEditorObjectPropertiesPanel(WorldBaseEditor editor, BaseGameObject obj) : base(obj)
	{
		Editor = editor;
		Object = obj;
		ObjectUId = Object.UniqueId;
		ObjectMap = Object.Map;

		if (obj is GameObject3D)
		{
			InjectDontSerializeProperty<Transform>("RotationDeg");
			InjectDontSerializeProperty<GameObject3D>("CurrentAnimation");

			// Remove Size to show Size3D so axes are intuitive to the 3D world.
			InjectDontSerializeProperty<Transform>("Size3D");
			RemoveSerializedProperty<Transform>("Size");

			RemoveSerializedProperty<Transform>("Depth");
		}
		else
		{
			// Inject the "Center" property of Transform.
			// Since it isn't serialized it wont be returned, but its handy to have for objects.
			InjectDontSerializeProperty<Transform>("Center");
			RemoveSerializedProperty<Transform>("Depth");
		}
	}

	private void InjectDontSerializeProperty<T>(string name)
	{
		EditorUtility.TypeAndFieldHandlers? declaringType = _fields.FirstOrDefault(x => x.DeclaringType == typeof(T));
		if (declaringType != null)
		{
			PropertyInfo? injectedProp = typeof(T).GetProperty(name);
			if (injectedProp != null)
			{
				XMLTypeHandler? editor = XMLHelpers.GetTypeHandler(injectedProp.PropertyType);
				declaringType.Fields.Add(new XMLFieldHandler(new ReflectedMemberHandler(injectedProp), editor));
			}
		}
	}

	private void RemoveSerializedProperty<T>(string name)
	{
		EditorUtility.TypeAndFieldHandlers? declaringType = _fields.FirstOrDefault(x => x.DeclaringType == typeof(T));
		if (declaringType == null) return;

		for (var i = 0; i < declaringType.Fields.Count; i++)
		{
			XMLFieldHandler? field = declaringType.Fields[i];
			if (field.Name != name) continue;
			declaringType.Fields.Remove(field);
			return;
		}
	}

	protected override IPropEditorGeneric? AddEditorForField(XMLFieldHandler field)
	{
		// Special handling for the animation selection.
		if (field.ReflectionInfo.DeclaredIn == typeof(GameObject3D) && field.Name == "CurrentAnimation") return new PropEditorObject3DAnimationList(Object as GameObject3D);

		return base.AddEditorForField(field);
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		UIBaseWindow? uiContainer = GetWindowById("InnerContainer");
		if (uiContainer == null) return;

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

		var viewObjectButton = new EditorButton();
		viewObjectButton.Text = "Show Me";
		viewObjectButton.StretchY = true;
		viewObjectButton.OnClickedProxy = _ =>
		{
			if (Object is GameObject3D obj3D)
			{
				Sphere boundingSphere = obj3D.BoundingSphere;
				Engine.Renderer.Camera.Position = boundingSphere.Origin - new Vector3(boundingSphere.Radius, boundingSphere.Radius, -boundingSphere.Radius);
				Engine.Renderer.Camera.LookAtPoint(boundingSphere.Origin);
			}
			else
			{
				Engine.Renderer.Camera.Position = Object.Position;
			}
		};
		uiContainer.AddChild(viewObjectButton);
	}

	protected override void ApplyObjectChange(IPropEditorGeneric editor, XMLFieldHandler field, object value)
	{
		Editor.ChangeObjectProperty(Object, field, value);

		var editorWindow = editor as UIBaseWindow;
		OnFieldEditorUpdated(field, editor, (FieldEditorWithLabel) editorWindow?.Parent!);
	}

	protected override void OnFieldEditorCreated(XMLFieldHandler field, IPropEditorGeneric? editor, FieldEditorWithLabel editorWithLabel)
	{
		// bruh
		if (field.ReflectionInfo.Name == "Entity")
			for (var i = 0; i < _editorUIs.Count; i++)
			{
				IPropEditorGeneric editorUI = _editorUIs[i];
				if (editorUI is PropEditorObject3DAnimationList animList) animList.EntityChanged();
			}

		if (Object.PrefabOrigin == null) return;

		var valueDiffAlert = new MapEditorGameObjectPrefabValueDiff();
		valueDiffAlert.Anchor = UIAnchor.CenterLeft;
		valueDiffAlert.ParentAnchor = UIAnchor.CenterLeft;
		valueDiffAlert.Offset = new Vector2(-3, 0); // Combat list spacing
		valueDiffAlert.Visible = Editor.IsPropertyDifferentFromPrefab(Object, field);
		valueDiffAlert.Id = "DiffAlert";
		editorWithLabel.AddChild(valueDiffAlert);
	}

	protected override void OnFieldEditorUpdated(XMLFieldHandler field, IPropEditorGeneric? editor, FieldEditorWithLabel editorWithLabel)
	{
		UIBaseWindow? diffAlert = editorWithLabel.GetWindowById("DiffAlert");
		if (diffAlert == null) return;
		diffAlert.Visible = Editor.IsPropertyDifferentFromPrefab(Object, field);
	}
}
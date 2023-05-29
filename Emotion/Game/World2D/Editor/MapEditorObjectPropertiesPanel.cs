#region Using

using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Emotion.Common.Serialization;
using Emotion.Game.World2D.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D;

public sealed class MapEditorObjectPropertiesPanel : GenericPropertiesEditorPanel
{
	public GameObject2D Object;
	public int ObjectUId;
	public Map2D ObjectMap;

	private Action<GameObject2D> _objectChangedCallback;

	private bool _objectReferenceInvalidated;

	public MapEditorObjectPropertiesPanel(GameObject2D obj, Action<GameObject2D> objectChangeCallback) : base(obj)
	{
		Object = obj;
		ObjectUId = Object.UniqueId;
		ObjectMap = Object.Map;

		// Inject the "Center" property of Transform.
		// Since it isn't serialized it wont be returned, but its handy to have for objects.
		// todo: Make this injection function generic? In the base class or w/e.
		EditorUtility.TypeAndFieldHandlers? positionalType = _fields.FirstOrDefault(x => x.DeclaringType == typeof(Positional));
		if (positionalType != null)
		{
			XMLTypeHandler? v2Handler = XMLHelpers.GetTypeHandler(typeof(Vector2));
			PropertyInfo? centerProp = typeof(Transform).GetProperty("Center");
			if (centerProp != null) positionalType.Fields.Add(new XMLFieldHandler(new ReflectedMemberHandler(centerProp), v2Handler));
		}

		_objectChangedCallback = objectChangeCallback;
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
		GameObject2D oldObject = Object;
		Map2D objectMap = oldObject.Map;

		Debug.Assert(objectMap != null);

		// Remove the object UndoAction and remove the object from the map.
		_objectChangedCallback(Object);
		
		
		// Clean the project of unserialized properties.
		// This basically brings it in line to how it will look when loaded the first time..
		bool isPersist = Object.ObjectFlags.HasFlag(ObjectFlags.Persistent);
		EditorUtility.SetObjectToSerializationDefault<GameObject2D>(Object);
		if (isPersist) Object.ObjectFlags |= ObjectFlags.Persistent;

		// Set the new value.
		field.ReflectionInfo.SetValue(Object, value);

		objectMap.Editor_ReinitializeObject(Object);
	}

	protected override bool UpdateInternal()
	{
		if (_objectReferenceInvalidated)
		{
			ObjectReferenceUpdated();
			_objectReferenceInvalidated = false;
			return false;
		}

		return base.UpdateInternal();
	}

	public override void UpdatePropertyValues()
	{
		base.UpdatePropertyValues();
		Header = $"{Object.ObjectName ?? "Object"} [{ObjectUId}] Properties";
	}

	// todo: check wtf this is about
	public void InvalidateObjectReference()
	{
		_objectReferenceInvalidated = true;
	}

	private void ObjectReferenceUpdated()
	{
		// Try to find the object we were showing.
		GameObject2D? newObjectReference = null;
		foreach (GameObject2D obj in ObjectMap.GetObjects())
		{
			if (obj.UniqueId == ObjectUId)
			{
				newObjectReference = obj;
				break;
			}
		}

		if (newObjectReference == null)
		{
			Controller?.RemoveChild(this);
			return;
		}

		Debug.Assert(newObjectReference.GetType() == Object.GetType());
		Debug.Assert(newObjectReference.UniqueId == Object.UniqueId);
		Debug.Assert(newObjectReference.Map == ObjectMap);
		Object = newObjectReference;
		_obj = newObjectReference;

		UpdatePropertyValues();
	}
}
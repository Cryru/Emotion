#region Using

using Emotion.Editor.EditorWindows.DataEditorUtil;
using Emotion.Editor.PropertyEditors;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

public class GenericPropertiesEditorPanel : EditorPanel
{
    public Action<string, object?>? OnPropertyEdited;
    public Action<object?>? OnNonComplexTypeValueChanged;

    protected List<EditorUtility.TypeAndFieldHandlers>? _fields;
    protected List<IPropEditorGeneric> _editorUIs;

    protected Type _objType;
    protected object? _obj;
    protected bool _nonComplexType;

    protected bool _spawnFieldGroupHeaders = true;

    public GenericPropertiesEditorPanel(object obj) : base($"{obj} Properties")
    {
        _editorUIs = new();

        var objType = obj.GetType();
        _objType = objType;

        // Types without parameterless constructors will explode since they cannot be created.
        // todo: look into at least displaying their values.
        if (!objType.IsValueType && objType != typeof(string) && !EditorUtility.HasParameterlessConstructor(obj))
            return;

        // Non complex types (string, int, etc.) can also be editted via this
        // panel but since there is no reference back to the object field that contains
        // them they need to be updated via a OnNonComplexTypeValueChanged callback.
        var fieldHandler = XMLHelpers.GetTypeHandler(objType);
        bool nonComplexType = fieldHandler != null && fieldHandler is not XMLComplexBaseTypeHandler;
        if (nonComplexType)
        {
            _obj = obj;
            _fields = new List<EditorUtility.TypeAndFieldHandlers>
            {
                new EditorUtility.TypeAndFieldHandlers(objType)
                {
                    Fields = new List<XMLFieldHandler>
                    {
                        new XMLFieldHandler(null, fieldHandler)
                    }
                }
            };
            _nonComplexType = true;
            _spawnFieldGroupHeaders = false;
            return;
        }

        _obj = obj;
        _fields = EditorUtility.GetTypeFields(obj);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        if (_obj == null)
        {
            var editorLabel = new MapEditorLabel("Object has no parameterless constructor\nand cannot be serialized (or edited).");
            _contentParent.AddChild(editorLabel);
            return;
        }

        var innerContainer = new UIBaseWindow();
        innerContainer.StretchX = true;
        innerContainer.StretchY = true;
        innerContainer.LayoutMode = LayoutMode.VerticalList;
        innerContainer.ListSpacing = new Vector2(0, 3);
        innerContainer.ChildrenAllSameWidth = true;
        innerContainer.Id = "InnerContainer";
        _contentParent.AddChild(innerContainer);

        var listContainer = new UIBaseWindow();
        listContainer.StretchX = true;
        listContainer.StretchY = true;
        listContainer.LayoutMode = LayoutMode.HorizontalList;
        listContainer.ZOffset = 10;
        innerContainer.AddChild(listContainer);

        var listNav = new UICallbackListNavigator();
        listNav.LayoutMode = LayoutMode.VerticalList;
        listNav.StretchX = true;
        listNav.ListSpacing = new Vector2(0, 1);
        listNav.Margins = new Rectangle(0, 0, 5, 0);
        listNav.ChildrenAllSameWidth = true;
        listNav.MinSizeX = 100;
        listContainer.AddChild(listNav);

        var scrollBar = new EditorScrollBar();
        listNav.SetScrollbar(scrollBar);
        listContainer.AddChild(scrollBar);

        // For each group of fields (inherited from the same class)
        for (var i = 0; i < _fields.Count; i++)
        {
            EditorUtility.TypeAndFieldHandlers fieldGroup = _fields[i];

            var fieldGroupHeaderContainer = new UIBaseWindow();
            fieldGroupHeaderContainer.StretchX = true;
            fieldGroupHeaderContainer.StretchY = true;

            if (_spawnFieldGroupHeaders)
            {
                var fieldGroupHeader = new UIText();
                fieldGroupHeader.ScaleMode = UIScaleMode.FloatScale;
                fieldGroupHeader.WindowColor = MapEditorColorPalette.TextColor;
                fieldGroupHeader.FontFile = "Editor/UbuntuMono-Regular.ttf";
                fieldGroupHeader.FontSize = MapEditorColorPalette.EditorButtonTextSize + 1;
                fieldGroupHeader.Underline = true;
                fieldGroupHeader.IgnoreParentColor = true;
                fieldGroupHeader.Text = XMLHelpers.GetTypeName(fieldGroup.DeclaringType);
                fieldGroupHeader.MinSize = new Vector2(0, 11);
                fieldGroupHeaderContainer.AddChild(fieldGroupHeader);
            }

            listNav.AddChild(fieldGroupHeaderContainer);

            // For each field
            for (var j = 0; j < fieldGroup.Fields.Count; j++)
            {
                XMLFieldHandler field = fieldGroup.Fields[j];
                IPropEditorGeneric? editor = AddEditorForField(field);
                if (editor != null)
                {
                    editor.Field = field;

                    object? propertyValue = _nonComplexType ? _obj : field.ReflectionInfo.GetValue(_obj); // Non-complex types have no reflection info.
                    editor.SetValue(propertyValue); // Initialize value before attaching callback.
                    editor.SetCallbackValueChanged(newValue => { ApplyObjectChange(editor, field, newValue); });

                    var editorAsWnd = (UIBaseWindow) editor;
                    editorAsWnd.Anchor = UIAnchor.CenterRight;
                    editorAsWnd.ParentAnchor = UIAnchor.CenterRight;
                    editorAsWnd.ZOffset = 10;

                    _editorUIs.Add(editor);
                }

                var editorWithLabel = new FieldEditorWithLabel($"{field.Name}: ", editor);
                OnFieldEditorCreated(field, editor, editorWithLabel);
                listNav.AddChild(editorWithLabel);
            }
        }

        //UpdatePropertyValues();
    }

    protected virtual void OnFieldEditorCreated(XMLFieldHandler field, IPropEditorGeneric? editor, FieldEditorWithLabel editorWithLabel)
    {
    }

    protected virtual void OnFieldEditorUpdated(XMLFieldHandler field, IPropEditorGeneric? editor, FieldEditorWithLabel editorWithLabel)
    {
    }

    protected virtual IPropEditorGeneric? AddEditorForField(XMLFieldHandler field)
    {
        if (_objType.IsAssignableTo(typeof(IGameDataReferenceEditorMarker)) && field.Name == "Id")
        {
            var gameDataRefInherit = _objType;
            Type[] genericArgs = gameDataRefInherit.GetGenericArguments();
            while (genericArgs == null || genericArgs.Length == 0)
            {
                gameDataRefInherit = gameDataRefInherit.BaseType;
                if (gameDataRefInherit == null) return null;

                genericArgs = gameDataRefInherit.GetGenericArguments();
            }

            string[] ids = GameDataDatabase.GetObjectIdsOfType(genericArgs[0]) ?? Array.Empty<string>();
            return new MetaPropEditorCombo<string>(ids);
        }

        // Primitives
        if (field.TypeHandler.Type == typeof(Vector2)) return new PropEditorFloat2();
        if (field.TypeHandler.Type == typeof(float)) return new PropEditorNumber<float>();
        if (field.TypeHandler.Type == typeof(int)) return new PropEditorNumber<int>();
        if (field.TypeHandler.Type == typeof(byte)) return new PropEditorNumber<byte>();
        if (field.TypeHandler.Type == typeof(Vector3)) return new PropEditorFloat3();
        if (field.TypeHandler.Type == typeof(Rectangle)) return new PropEditorRect();
        if (field.TypeHandler.Type == typeof(Matrix4x4)) return new PropEditorMatrix();
        if (field.TypeHandler.Type == typeof(string))
        {
            var assetFileNameAttribute = field.ReflectionInfo?.GetAttribute<AssetFileNameAttribute>();
            if (assetFileNameAttribute != null)
                return new PropEditorStringPath(assetFileNameAttribute);

            return new PropEditorString();
        }

        if (field.TypeHandler.Type == typeof(bool)) return new PropEditorBool();
        if (field.TypeHandler.Type.IsEnum) return new PropEditorEnum(field.TypeHandler.Type, field.ReflectionInfo?.Nullable ?? false);

        // Complex
        if (field.TypeHandler is XMLComplexTypeHandler) return new PropEditorNestedObject();
        if (field.TypeHandler.Type == typeof(Color)) return new PropEditorNestedObject(); // temp
        if (field.TypeHandler is XMLArrayTypeHandler && field.TypeHandler is not XMLDictionaryTypeHandler)
            return new PropEditorArray();

        return new PropEditorNone();
    }

    protected virtual void ApplyObjectChange(IPropEditorGeneric editor, XMLFieldHandler field, object value)
    {
        var editorWindow = editor as UIBaseWindow;

        if (_nonComplexType)
        {
            if (!Helpers.AreObjectsEqual(_obj, value)) OnNonComplexTypeValueChanged?.Invoke(value);
            _obj = value;
            OnFieldEditorUpdated(field, editor, (FieldEditorWithLabel) editorWindow?.Parent!);
            return;
        }

        AssertNotNull(_obj);

        object? oldValue = field.ReflectionInfo.GetValue(_obj);
        field.ReflectionInfo.SetValue(_obj, value);

        if (field.TypeHandler is XMLArrayTypeHandler || !Helpers.AreObjectsEqual(oldValue, value))
            OnPropertyEdited?.Invoke(field.Name, oldValue);

        OnFieldEditorUpdated(field, editor, (FieldEditorWithLabel) editorWindow?.Parent!);
    }

    protected override bool UpdateInternal()
    {
        UpdatePropertyValues();
        return base.UpdateInternal();
    }

    protected virtual void UpdatePropertyValues()
    {
        for (var i = 0; i < _editorUIs.Count; i++)
        {
            IPropEditorGeneric editor = _editorUIs[i];
            var editorWindow = editor as UIBaseWindow;

            if (Controller?.InputFocus != null && editorWindow != null && Controller.InputFocus.IsWithin(editorWindow)) continue;

            XMLFieldHandler? field = editor.Field;
            object? propertyValue = _nonComplexType ? _obj : field.ReflectionInfo?.GetValue(_obj);
            if (!Helpers.AreObjectsEqual(editor.GetValue(), propertyValue))
            {
                editor.SetValue(propertyValue);
                OnFieldEditorUpdated(field, editor, (FieldEditorWithLabel) editorWindow?.Parent!);
            }
        }
    }
}
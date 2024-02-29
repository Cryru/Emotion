using Emotion.Editor.EditorHelpers;
using Emotion.Editor.PropertyEditors;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.Editor.EditorWindows;

public abstract class ObjectEditorPageDesc
{
    public object? Object;
    public string? OriginName;

    public abstract void SetupPage(ObjectEditor editor, UIBaseWindow pageContent);
}

public class ObjectEditorPageDescObjectMode : ObjectEditorPageDesc
{
    public List<EditorUtility.TypeAndFieldHandlers>? Fields;

    public bool IsReadOnly;
    public bool IsValueType;

    public ObjectEditorPageDescObjectMode(object obj)
    {
        Object = obj;
        Fields = EditorUtility.GetTypeFields(Object, out bool _);
    }

    public override void SetupPage(ObjectEditor editor, UIBaseWindow pageContent)
    {
        if (Fields == null || Object == null) return;

        var listNav = new UIBaseWindow();
        listNav.LayoutMode = LayoutMode.VerticalList;
        listNav.ListSpacing = new Vector2(0, 1);
        pageContent.AddChild(UIScrollArea.WrapInScrollArea(listNav));

        // For each group of fields (inherited from the same class)
        for (var i = 0; i < Fields.Count; i++)
        {
            EditorUtility.TypeAndFieldHandlers fieldGroup = Fields[i];

            var fieldGroupHeaderContainer = new UIBaseWindow();
            fieldGroupHeaderContainer.LayoutMode = LayoutMode.VerticalList;
            fieldGroupHeaderContainer.ListSpacing = new Vector2(0, 1);
            listNav.AddChild(fieldGroupHeaderContainer);

            var fieldGroupHeader = new MapEditorLabel(XMLHelpers.GetTypeName(fieldGroup.DeclaringType));
            fieldGroupHeader.FontSize = MapEditorColorPalette.EditorButtonTextSize + 1;
            fieldGroupHeader.Underline = true;
            fieldGroupHeader.Margins = new Primitives.Rectangle(0, 3, 0, 2);
            fieldGroupHeaderContainer.AddChild(fieldGroupHeader);

            // For each field
            for (var j = 0; j < fieldGroup.Fields.Count; j++)
            {
                XMLFieldHandler field = fieldGroup.Fields[j];

                PropEditorBase editorForType = GetEditorForField(field);
                object? initialValue = field.ReflectionInfo.GetValue(Object);
                editorForType.Init($"{field.Name}: ", field, initialValue, OnObjectChanged);
                //OnFieldEditorCreated(field, editor, editorWithLabel);
                listNav.AddChild(editorForType);
            }
        }
    }

    public PropEditorBase GetEditorForField(XMLFieldHandler field)
    {
        var fieldType = field.TypeHandler.Type;

        if (fieldType == typeof(float)) return new PropEditorNumberv2<float>();
        if (fieldType == typeof(int)) return new PropEditorNumberv2<int>();
        if (fieldType == typeof(byte)) return new PropEditorNumberv2<byte>();

        return new PropEditorNonev2();
    }

    protected virtual void OnObjectChanged(PropEditorBase editor, XMLFieldHandler field, object value)
    {
    }
}

public class PropEditorNonev2 : PropEditorBase
{
    protected override void AttachPropEditorControls(UIBaseWindow parent)
    {

    }

    protected override void ValueChanged(object? newValue)
    {
        throw new NotImplementedException();
    }
}

public class PropEditorNumberv2<T> : PropEditorBase where T : INumber<T>
{
    private T _value = T.Zero;
    private UITextInput _textInput = null!;

    protected override void AttachPropEditorControls(UIBaseWindow parent)
    {
        var inputBg = new UISolidColor();
        inputBg.WindowColor = MapEditorColorPalette.ButtonColor;
        inputBg.Paddings = new Rectangle(2, 1, 2, 1);
        inputBg.Anchor = UIAnchor.CenterLeft;
        inputBg.ParentAnchor = UIAnchor.CenterLeft;
        parent.AddChild(inputBg);

        var textEditor = new UITextInput();
        textEditor.FontFile = "Editor/UbuntuMono-Regular.ttf";
        textEditor.FontSize = MapEditorColorPalette.EditorButtonTextSize;
        textEditor.SizeOfText = true;
        textEditor.MinSize = new Vector2(25, 0);
        textEditor.IgnoreParentColor = true;
        textEditor.Id = "textEditor";
        textEditor.OnSubmit = val =>
        {
            if (!T.TryParse(val, null, out T? intVal)) return;
            ViaEditorSetValue(intVal);
        };
        textEditor.SubmitOnEnter = true;
        textEditor.SubmitOnFocusLoss = true;
        inputBg.AddChild(textEditor);
        _textInput = textEditor;
        ValueChanged(_currentValue);
    }

    protected override void ValueChanged(object? newValue)
    {
        if (newValue is not T numericalVal)
        {
            Assert(false, "Wrong type value set in number editor!");
            return;
        }

        _value = numericalVal;
        _textInput.Text = _value.ToString();
    }
}

public abstract class PropEditorBase : UIBaseWindow
{
    protected bool _initialized;

    protected string? _labelText;
    protected XMLFieldHandler _fieldHandler = null!;
    protected object? _currentValue;
    protected Action<PropEditorBase, XMLFieldHandler, object?>? _valueChangedCallback;

    public PropEditorBase()
    {
        UseNewLayoutSystem = true;
        FillY = false;
        FillX = false;
    }

    public object? GetValue()
    {
        return _currentValue;
    }

    public void SetValue(object? value)
    {
        if (Helpers.AreObjectsEqual(_currentValue, value)) return;

        _currentValue = value;
        ValueChanged(value);
    }

    protected void ViaEditorSetValue(object? newValue)
    {
        if (Helpers.AreObjectsEqual(_currentValue, newValue)) return;

        _currentValue = newValue;
        _valueChangedCallback?.Invoke(this, _fieldHandler, newValue);
        ValueChanged(newValue);
    }

    public void Init(string? label, XMLFieldHandler fieldHandler, object? startValue, Action<PropEditorBase, XMLFieldHandler, object?>? onValueChanged)
    {
        _labelText = label;
        _fieldHandler = fieldHandler;
        _currentValue = startValue;
        _valueChangedCallback = onValueChanged;

        _initialized = true;
    }

    public override void AttachedToController(UIController controller)
    {
        Assert(_initialized, "PropEditor must be initialized prior to attaching to a controller!");
        base.AttachedToController(controller);

        if (_labelText != null)
        {
            MapEditorLabel label = new MapEditorLabel(_labelText);
            AddChild(label);
            LayoutMode = LayoutMode.HorizontalList;
        }

        var editorControls = new UIBaseWindow();
        AddChild(editorControls);

        var editorControlsParent = new UIBaseWindow();
        editorControls.AddChild(editorControlsParent);

        AttachPropEditorControls(editorControlsParent);
    }

    protected abstract void AttachPropEditorControls(UIBaseWindow parent);
    protected abstract void ValueChanged(object? newValue);
}

public class ObjectEditorPageDescArrayMode : ObjectEditorPageDesc
{
    public IObjectEditorArrayModeAdapter ArrayAdapter;

    public override void SetupPage(ObjectEditor editor, UIBaseWindow pageContent)
    {

    }
}

public interface IObjectEditorArrayModeAdapter
{
    void AddToArray();

    void RemoveFromArray(int idx);

    void GetAtIndex(int idx);

    int GetLength();

    void MoveItemUp(int idx);

    void MoveItemDown(int idx);

    Type GetElementType();
}


public class ObjectEditor : EditorPanelv2
{
    private object _rootObject;

    protected List<ObjectEditorPageDesc> _pages;

    public ObjectEditor(object obj) : base("Properties")
    {
        _rootObject = obj;
        _pages = new List<ObjectEditorPageDesc>();

        UseNewLayoutSystem = true;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var innerContainer = new UIBaseWindow();
        innerContainer.LayoutMode = LayoutMode.VerticalList;
        innerContainer.ListSpacing = new Vector2(0, 3);
        innerContainer.Id = "InnerContainer";
        _contentParent.AddChild(innerContainer);

        //var editorLabel = new MapEditorLabel("Object Properties: ");
        //innerContainer.AddChild(editorLabel);

        var currentPage = new UIBaseWindow();
        currentPage.Id = "PageContent";
        innerContainer.AddChild(currentPage);

        UISetupPageForDesc(AutoGetPageDescForObject(_rootObject));
        UIUpdatePagingDisplay();
    }

    private ObjectEditorPageDesc AutoGetPageDescForObject(object obj)
    {
        var objType = obj.GetType();
        var typeHandlerBase = XMLHelpers.GetTypeHandler(objType);

        return new ObjectEditorPageDescObjectMode(obj);
    }

    private void UISetupPageForDesc(ObjectEditorPageDesc desc)
    {
        _pages.Add(desc);

        var pageContent = GetWindowById("PageContent");
        AssertNotNull(pageContent);
        pageContent.ClearChildren();

        desc.SetupPage(this, pageContent);
    }

    private void UIUpdatePagingDisplay()
    {

    }
}

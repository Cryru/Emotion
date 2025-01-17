#nullable enable

#region Using

using System.Collections;
using Emotion.Editor.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.Utility;
using Emotion.WIPUpdates.One.EditorUI.Components;

#endregion

namespace Emotion.Editor.PropertyEditors;

public class PropEditorArray : EditorButton, IPropEditorGeneric
{
    public XMLFieldHandler? Field
    {
        get => _field;
        set
        {
            _field = value;
            UpdateText();
        }
    }

    private XMLFieldHandler? _field;

    public object? Value;
    private Action<object>? _changeCallback;

    public PropEditorArray()
    {
        OnClickedProxy = _ => { OpenPanel(); };
        StretchY = true;
    }

    protected void OpenPanel()
    {
        var panel = new PropEditorArrayPanel(this);
        Controller?.AddChild(panel);
    }

    public int GetLength()
    {
        return GetLength(Value);
    }

    public static int GetLength(object? value)
    {
        if (value is Array valAsArray)
            return valAsArray.Length;
        if (value is IList valAsList) return valAsList.Count;

        return 0;
    }

    public Type GetElementType()
    {
        var declType = Field!.TypeHandler.Type;
        if (declType.IsAssignableTo(typeof(Array))) return declType.GetElementType() ?? typeof(object);

        if (declType.IsAssignableTo(typeof(IList)))
        {
            var genericArg = declType.GetGenericArguments();
            return genericArg.Length > 0 ? genericArg[0] : typeof(object);
        }

        return typeof(object);
    }

    public object? GetItemAtIndex(int index)
    {
        var declType = Field!.TypeHandler.Type;
        if (declType.IsAssignableTo(typeof(Array)) && Value is Array valAsArray)
            return valAsArray.GetValue(index);
        if (declType.IsAssignableTo(typeof(IList)) && Value is IList valAsList) return valAsList[index];

        return null;
    }

    public void SetItemAtIndex(int index, object value)
    {
        var declType = Field!.TypeHandler.Type;
        if (declType.IsAssignableTo(typeof(Array)) && Value is Array valAsArray)
            valAsArray.SetValue(value, index);
        else if (declType.IsAssignableTo(typeof(IList)) && Value is IList valAsList) valAsList[index] = value;
        ValueModified();
    }

    public void CreateItem()
    {
        var elementType = GetElementType();
        var newItem = EditorUtility.CreateNewObjectOfType(elementType);

        var declType = Field!.TypeHandler.Type;
        if (declType.IsAssignableTo(typeof(Array)))
        {
            Array? arrayVal = Value as Array;
            if (arrayVal == null)
            {
                arrayVal = Array.CreateInstance(elementType, 1);
            }
            else
            {
                var biggerArray = Array.CreateInstance(elementType, arrayVal.Length + 1);
                arrayVal.CopyTo(biggerArray, 0);
                arrayVal = biggerArray;
            }

            arrayVal.SetValue(newItem, arrayVal.Length - 1);
            SetValue(arrayVal);
        }
        else if (declType.IsAssignableTo(typeof(IList)))
        {
            IList? listVal = Value as IList;
            if (listVal == null)
            {
                listVal = Activator.CreateInstance(declType, true) as IList;
                AssertNotNull(listVal);
                SetValue(listVal);
            }

            listVal.Add(newItem);
        }

        ValueModified();
    }

    public void RemoveItemAtIndex(int index)
    {
        var declType = Field!.TypeHandler.Type;
        if (declType.IsAssignableTo(typeof(Array)) && Value is Array valAsArray)
        {
            valAsArray = valAsArray.RemoveFromArray(index);
            SetValue(valAsArray);
        }
        else if (declType.IsAssignableTo(typeof(IList)) && Value is IList valAsList)
        {
            valAsList.RemoveAt(index);
        }

        ValueModified();
    }

    public void SetValue(object value)
    {
        if (Helpers.AreObjectsEqual(Value, value)) return;

        Value = value;
        ValueModified();
    }

    private void UpdateText()
    {
        var value = Value;
        var length = GetLength(value);
        var itemType = GetElementType();
        Text = $"{XMLHelpers.GetTypeName(itemType)}[{length}]";
        if (value == null) Text += " (null)";
    }

    public void ValueModified()
    {
        UpdateText();
        _changeCallback?.Invoke(Value);
    }

    public object GetValue()
    {
        return Value;
    }

    public void SetCallbackValueChanged(Action<object> callback)
    {
        _changeCallback = callback;
    }
}
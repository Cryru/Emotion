#region Using

using System.Reflection;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;

#endregion

namespace Emotion.Editor.EditorHelpers;

public static class EditorUtility
{
    /// <summary>
    /// Set all fields in the object to the values they would have
    /// if the object is newly created by deserializing it.
    /// </summary>
    public static void SetObjectToSerializationDefault<T>(object obj)
    {
        // First serialization copy.
        string xml = XMLFormat.To(obj);
        object recreated = XMLFormat.From<T>(xml);

        // Get all field (incl backing fields), inherited too
        Type type = obj.GetType();
        var fields = new List<FieldInfo>();
        var fieldsAdded = new HashSet<string>();
        while (type != null && type != typeof(object))
        {
            FieldInfo[] fieldsInType = type.GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

            for (var i = 0; i < fieldsInType.Length; i++)
            {
                FieldInfo field = fieldsInType[i];
                string name = field.Name;
                if (fieldsAdded.Contains(name)) continue;

                fields.Add(field);
                fieldsAdded.Add(name);
            }

            type = type.BaseType;
        }

        // Copy properties from the serialization copy to the obj.
        for (var i = 0; i < fields.Count; i++)
        {
            FieldInfo field = fields[i];
            object value = field.GetValue(recreated);
            field.SetValue(obj, value);
        }
    }

    public class TypeAndFieldHandlers
    {
        public Type DeclaringType;
        public List<XMLFieldHandler> Fields = new();

        public TypeAndFieldHandlers(Type t)
        {
            DeclaringType = t;
        }
    }

    /// <summary>
    /// Get list of types with a parameterless constructor that inherit a specific type.
    /// </summary>
    public static List<Type> GetTypesWhichInherit<T>()
    {
        List<Type> inheritors = new();
        Type type = typeof(T);
        foreach (Assembly assembly in Helpers.AssociatedAssemblies)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type assemblyType in types)
            {
                if (!type.IsAssignableFrom(assemblyType)) continue;

                bool invalid = assemblyType.GetConstructor(Type.EmptyTypes) == null;
                if (invalid) continue;

                inheritors.Add(assemblyType);
            }
        }

        return inheritors;
    }

    public static bool HasParameterlessConstructor(object obj)
    {
        Type t = obj.GetType();
        return t.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes) != null ||
               t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes) != null;
    }

    /// <summary>
    /// Get all serializable fields of a type, ordered by declaring type.
    /// </summary>
    public static List<TypeAndFieldHandlers> GetTypeFields<T>(T obj)
    {
        var typeHandler = (XMLComplexBaseTypeHandler)XMLHelpers.GetTypeHandler(obj.GetType());
        List<TypeAndFieldHandlers> currentWindowHandlers = new();
        currentWindowHandlers.Clear();

        if (typeHandler == null) return currentWindowHandlers;

        // Collect type handlers sorted by declared type.
        IEnumerator<XMLFieldHandler> fields = typeHandler.EnumFields();
        while (fields.MoveNext())
        {
            XMLFieldHandler field = fields.Current;
            if (field == null) continue;
            if (field.ReflectionInfo.GetAttribute<DontShowInEditorAttribute>() != null) continue;

            TypeAndFieldHandlers handlerMatch = null;
            for (var i = 0; i < currentWindowHandlers.Count; i++)
            {
                TypeAndFieldHandlers handler = currentWindowHandlers[i];
                if (handler.DeclaringType == field.ReflectionInfo.DeclaredIn)
                {
                    handlerMatch = handler;
                    break;
                }
            }

            if (handlerMatch == null)
            {
                handlerMatch = new TypeAndFieldHandlers(field.ReflectionInfo.DeclaredIn);
                currentWindowHandlers.Add(handlerMatch);
            }

            handlerMatch.Fields.Add(field);
        }

        // Sort by inheritance.
        var indices = new int[currentWindowHandlers.Count];
        var idx = 0;
        Type t = typeHandler.Type;
        while (t != typeof(object))
        {
            for (var i = 0; i < currentWindowHandlers.Count; i++)
            {
                TypeAndFieldHandlers handler = currentWindowHandlers[i];
                if (handler.DeclaringType != t) continue;
                indices[i] = idx;
                idx++;
                break;
            }

            t = t!.BaseType;
        }

        List<TypeAndFieldHandlers> originalIndices = new();
        originalIndices.AddRange(currentWindowHandlers);

        currentWindowHandlers.Sort((x, y) =>
        {
            int idxX = originalIndices.IndexOf(x);
            int idxY = originalIndices.IndexOf(y);
            return indices[idxY] - indices[idxX];
        });

        return currentWindowHandlers;
    }

    public static Enum EnumSetFlag(Enum value, Enum flag, bool set)
    {
        var valueType = value.GetType();
        Type underlyingType = Enum.GetUnderlyingType(valueType);

        // note: AsInt mean: math integer vs enum (not the c# int type)
        dynamic valueAsInt = Convert.ChangeType(value, underlyingType);
        dynamic flagAsInt = Convert.ChangeType(flag, underlyingType);
        if (set)
            valueAsInt |= flagAsInt;
        else
            valueAsInt &= ~flagAsInt;

        return (Enum)Enum.ToObject(valueType, valueAsInt);
    }

    public static string GetEnumFlagsAsBinaryString(Enum value)
    {
        // Get the underlying type of the enum
        Type enumType = value.GetType();

        var values = Enum.GetValues(enumType);
        int numFlags = values.Length;

        // Find out if there is a zero flag value.
        Enum zeroFlagValue = null;
        Type underlyingType = Enum.GetUnderlyingType(enumType);
        for (int i = 0; i < numFlags; i++)
        {
            var thisVal = values.GetValue(i) as Enum;
            dynamic underlyingValue = Convert.ChangeType(thisVal, underlyingType);
            if (underlyingValue == 0)
            {
                zeroFlagValue = thisVal;
                break;
            }
        }

        // Dont write the zero flag as a flag.
        int flagsToWrite = numFlags;
        if (zeroFlagValue != null) flagsToWrite--;

        int writeI = 0;
        char[] binaryChars = new char[flagsToWrite];
        for (int i = 0; i < numFlags; i++)
        {
            var thisVal = values.GetValue(i) as Enum;

            if (zeroFlagValue != null && Helpers.AreObjectsEqual(thisVal, zeroFlagValue)) continue;

            if (value.HasFlag(thisVal))
                binaryChars[writeI] = '1';
            else
                binaryChars[writeI] = '0';
            writeI++;
        }

        return new string(binaryChars);
    }

    public static object? CreateNewObjectOfType(Type t)
    {
        if (t == typeof(string)) return new string("New String");
        return Activator.CreateInstance(t, true);
    }
}
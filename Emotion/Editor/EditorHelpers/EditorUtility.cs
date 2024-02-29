#region Using

using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using Emotion.IO;
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
    public static List<TypeAndFieldHandlers> GetTypeFields(object obj, out bool nonComplexType)
    {
        nonComplexType = false;
        var objType = obj.GetType();
        var typeHandlerBase = XMLHelpers.GetTypeHandler(objType);
        if (typeHandlerBase == null) return new List<TypeAndFieldHandlers>();

        XMLComplexBaseTypeHandler typeHandler = typeHandlerBase as XMLComplexBaseTypeHandler;
        nonComplexType = typeHandler == null;
        if (nonComplexType)
        {
            return new List<TypeAndFieldHandlers>
            {
                new TypeAndFieldHandlers(objType)
                {
                    Fields = new List<XMLFieldHandler>
                    {
                        new XMLFieldHandler(null, typeHandlerBase)
                    }
                }
            };
        }
        
        List<TypeAndFieldHandlers> currentTypeHandlers = new();

        // Collect type handlers sorted by declared type.
        IEnumerator<XMLFieldHandler> fields = typeHandler.EnumFields();
        while (fields.MoveNext())
        {
            XMLFieldHandler field = fields.Current;
            if (field == null) continue;
            if (field.ReflectionInfo.GetAttribute<DontShowInEditorAttribute>() != null) continue;

            // Try to find a declarting type match for this field.
            TypeAndFieldHandlers handlerMatch = null;
            for (var i = 0; i < currentTypeHandlers.Count; i++)
            {
                TypeAndFieldHandlers handler = currentTypeHandlers[i];
                if (handler.DeclaringType == field.ReflectionInfo.DeclaredIn)
                {
                    handlerMatch = handler;
                    break;
                }
            }

            if (handlerMatch == null)
            {
                handlerMatch = new TypeAndFieldHandlers(field.ReflectionInfo.DeclaredIn);
                currentTypeHandlers.Add(handlerMatch);
            }

            handlerMatch.Fields.Add(field);
        }

        // Sort the type groups by inheritance.
        var indices = new int[currentTypeHandlers.Count];
        var idx = 0;
        Type t = typeHandler.Type;
        while (t != typeof(object))
        {
            for (var i = 0; i < currentTypeHandlers.Count; i++)
            {
                TypeAndFieldHandlers handler = currentTypeHandlers[i];
                if (handler.DeclaringType != t) continue;
                indices[i] = idx;
                idx++;
                break;
            }

            t = t!.BaseType;
        }

        List<TypeAndFieldHandlers> originalIndices = new();
        originalIndices.AddRange(currentTypeHandlers);

        currentTypeHandlers.Sort((x, y) =>
        {
            int idxX = originalIndices.IndexOf(x);
            int idxY = originalIndices.IndexOf(y);
            return indices[idxY] - indices[idxX];
        });

        // Now sort all fields in all type lists by metadata token.
        // This should present them in declaration order.
        for (int i = 0; i < currentTypeHandlers.Count; i++)
        {
            var byTypeHandlers = currentTypeHandlers[i];
            byTypeHandlers.Fields.Sort((x, y) =>
            {
                return x.ReflectionInfo.GetMetadataToken() - y.ReflectionInfo.GetMetadataToken();
            });
        }

        return currentTypeHandlers;
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

    public static string? GetCsProjFilePath()
    {
        string fileFolder = DebugAssetStore.ProjectDevPath;
        string[] allFilesHere = Directory.GetFiles(fileFolder);
        string? csProjFile = null;
        for (int i = 0; i < allFilesHere.Length; i++)
        {
            var file = allFilesHere[i];
            if (file.EndsWith(".csproj"))
            {
                csProjFile = file;
                break;
            }
        }

        return csProjFile;
    }

    public static void RegisterAssetAsCopyNewerInProjectFile(string path)
    {
        // Don't code gen in release mode lol
        if (Engine.Configuration == null || !Engine.Configuration.DebugMode) return;

        var csProjFile = GetCsProjFilePath();
        if (csProjFile == null) return; // No file

        string csProjFileContents = File.ReadAllText(csProjFile);
        ReadOnlySpan<char> contentAsSpan = csProjFileContents.AsSpan();
        StringBuilder builder = new StringBuilder();

        int lastCopyNewestUse = contentAsSpan.LastIndexOf("PreserveNewest");
        bool createNewGroup = lastCopyNewestUse == -1;
        int flushedUpTo = -1;
        if (createNewGroup)
        {
            // Create new item group
            int finalTag = contentAsSpan.IndexOf("</Project>");
            builder.Append(contentAsSpan.Slice(0, finalTag).ToString());
            builder.AppendLine("\n  <ItemGroup>");
        }
        else
        {
            int lastItemInThatGroup = csProjFileContents.IndexOf("</None>", lastCopyNewestUse);
            flushedUpTo = lastItemInThatGroup + "</None>".Length;
            builder.AppendLine(csProjFileContents.Substring(0, flushedUpTo).ToString());

            if (flushedUpTo == -1) return;
        }

        builder.AppendLine($"    <None Update=\"{path}\">");
        builder.AppendLine("      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>");
        builder.Append("    </None>");

        if (createNewGroup)
        {
            builder.Append("\n  </ItemGroup>");
            builder.Append("\n</Project>");
        }
        else
        {
            builder.Append(contentAsSpan.Slice(flushedUpTo).ToString());
        }

        string contentModified = builder.ToString().Replace("\r\n", "\n");
        File.WriteAllText(csProjFile, contentModified);
    }
}
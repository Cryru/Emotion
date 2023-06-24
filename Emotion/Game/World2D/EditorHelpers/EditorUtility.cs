#region Using

using System.Reflection;
using Emotion.Editor;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
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

		/// <summary>
		/// Get all serializable fields of a type, ordered by declaring type.
		/// </summary>
		public static List<TypeAndFieldHandlers> GetTypeFields<T>(T obj)
		{
			var typeHandler = (XMLComplexBaseTypeHandler) XMLHelpers.GetTypeHandler(obj.GetType());
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
	}
}
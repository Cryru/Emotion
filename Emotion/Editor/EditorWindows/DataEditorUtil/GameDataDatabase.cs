#nullable enable

#region Using

using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Editor.EditorWindows.DataEditorUtil.GameDataObject>;

#endregion

namespace Emotion.Editor.EditorWindows.DataEditorUtil;

public static class GameDataDatabase
{
	public static bool Initialized { get; private set; }

	private static Dictionary<Type, Dictionary<string, GameDataObject>>? _database;

	private const string DATA_OBJECTS_PATH = "Data";

	public static async Task Load()
	{
		if (Initialized) return;

		_database = new Dictionary<Type, Dictionary<string, GameDataObject>>();
		List<Type>? types = EditorUtility.GetTypesWhichInherit<GameDataObject>();
		for (var i = 0; i < types.Count; i++)
		{
			Type type = types[i];
			string typeName = type.Name;
			string[] files = Engine.AssetLoader.GetAssetsInFolder($"{DATA_OBJECTS_PATH}/{typeName}");
			var tasks = new Task<GameDataObjectAsset?>[files.Length];
			for (var j = 0; j < files.Length; j++)
			{
				string file = files[j];
				tasks[j] = Engine.AssetLoader.GetAsync<GameDataObjectAsset>(file);
			}

			await Task.WhenAll(tasks);

			var objectsOfThisType = new Dictionary<string, GameDataObject>(StringComparer.OrdinalIgnoreCase);
			for (var j = 0; j < tasks.Length; j++)
			{
				Task<GameDataObjectAsset?> task = tasks[j];
				if (!task.IsCompleted || task.Result?.Content == null) continue;
				GameDataObjectAsset? asset = task.Result;
				GameDataObject? content = asset.Content;

				string assetName = EnsureNonDuplicatedId(content.Id, objectsOfThisType);

				// Just in case set identifying parameters again.
				// This should result in nothing having changed :)
				content.Id = assetName;
				content.AssetPath = files[j];

				objectsOfThisType.Add(content.Id, content);
			}

			_database.Add(type, objectsOfThisType);
		}

		Initialized = true;
	}

	private static string EnsureNonDuplicatedId(string name, IDictionary dict)
	{
		var counter = 1;
		string originalName = name;
		while (dict.Contains(name)) name = originalName + "_" + counter++;
		return name;
	}

	public static string EnsureNonDuplicatedId(string name, Type type)
	{
		Dictionary<string, GameDataObject>? dict = GetObjectsOfType(type);
		return dict != null ? EnsureNonDuplicatedId(name, dict) : name;
	}

	public static T? GetDataObject<T>(string name) where T : GameDataObject
	{
        Dictionary<string, GameDataObject>? dict = GetObjectsOfType(typeof(T));
		if (dict == null) return null;
		dict.TryGetValue(name, out GameDataObject? obj);
		return (T?)obj;
    }

	public static Type[]? GetGameDataTypes()
	{
		if (!Initialized) return null;
		AssertNotNull(_database);
		return _database.Keys.ToArray();
	}

	public static Dictionary<string, GameDataObject>? GetObjectsOfType(Type t)
	{
		if (!Initialized) return null;
		AssertNotNull(_database);
		return !_database.ContainsKey(t) ? null : _database[t];
	}

    public static T[]? GetObjectsOfType<T>() where T : GameDataObject
    {
        if (!Initialized) return null;
        AssertNotNull(_database);

		var t = typeof(T);
		if (!_database.ContainsKey(t)) return null;

		var dict = _database[t];
		T[] arr = new T[dict.Count];
		int idx = 0;
		foreach (var item in dict)
		{
			arr[idx] = (T) item.Value;
			idx++;
		}

        return arr;
    }

    public static bool EditorAddObject(Type t, GameDataObject obj)
	{
		if (!Initialized) return false;
		AssertNotNull(_database);

		Dictionary<string, GameDataObject> objectsOfThisType;
		if (_database.TryGetValue(t, out Dictionary<string, GameDataObject>? value))
		{
			objectsOfThisType = value;
		}
		else
		{
			objectsOfThisType = new();
			_database[t] = objectsOfThisType;
		}

		string safeId = EnsureNonDuplicatedId(obj.Id, objectsOfThisType);
		obj.Id = safeId;
		objectsOfThisType.Add(safeId, obj);
		GenerateCode();

        string path = GetAssetPath(obj);
		obj.AssetPath = path;
		GameDataObjectAsset asAsset = GameDataObjectAsset.CreateFromContent(obj, path);
		return asAsset.Save();
	}

	public static void EditorDeleteObject(Type t, GameDataObject obj)
	{
		string? assetPath = obj.AssetPath;
		if (assetPath == null) return;

		obj.AssetPath = null;
		Engine.AssetLoader.Destroy(assetPath);
        DebugAssetStore.DeleteFile(assetPath);
		EditorReIndex(t);
    }

    public static void EditorReIndex(Type type)
	{
		Dictionary<string, GameDataObject>? data = GetObjectsOfType(type);
		if (data == null) return;

		// Readd objects that no longer match their key.
		// Objects with null AssetPaths are deleted.
		List<string>? toReAdd = null;
		List<string>? toRemove = null;
		foreach ((string? oldObjId, GameDataObject? obj) in data)
		{
			string? objId = obj.Id;
			if(obj.AssetPath == null)
			{
				toRemove ??= new List<string>();
                toRemove.Add(objId);
			}
			else if (oldObjId != objId)
			{
				toReAdd ??= new List<string>();
				toReAdd.Add(oldObjId);
			}
		}

        // Add them back as their new Id.
        if (toRemove != null)
            for (var i = 0; i < toRemove.Count; i++)
            {
                string objName = toRemove[i];
                data.Remove(objName, out GameDataObject? val);
                AssertNotNull(val);
            }

        // Add them back as their new Id.
        if (toReAdd != null)
			for (var i = 0; i < toReAdd.Count; i++)
			{
				string objName = toReAdd[i];
				data.Remove(objName, out GameDataObject? val);
				AssertNotNull(val);
				data.Add(val.Id, val);
			}

		GenerateCode();
    }

	public static string GetAssetPath(GameDataObject obj)
	{
		Type type = obj.GetType();

		return $"{DATA_OBJECTS_PATH}/{type.Name}/{obj.Id}.xml";
	}

    private static void GenerateCode()
    {
        // Don't code gen in release mode lol
        if (Engine.Configuration == null || !Engine.Configuration.DebugMode) return;

        var builder = new StringBuilder();
        builder.AppendLine("// THIS IS AN AUTO-GENERATED FILE (GameDataDatabase.cs) TO HELP WITH INTELLISENSE");
        builder.AppendLine("");
        builder.AppendLine("namespace GameData");
        builder.AppendLine("{");
		var types = GetGameDataTypes();
		if (types != null)
		{
			for (var i = 0; i < types.Length; i++)
			{
				var type = types[i];

				if (i != 0) builder.AppendLine("");

                builder.AppendLine($"	public static class {type.Name}Defs");
                builder.AppendLine("	{");
				var items = GetObjectsOfType(type);
				if (items != null)
				{
                    foreach (var item in items)
                    {
                        builder.AppendLine($"		public static readonly string {item.Key} = \"{item.Key}\";");
                    }
                }
                builder.AppendLine("    }");
            }
		}
        builder.AppendLine("}");

        byte[] fileData = Encoding.UTF8.GetBytes(builder.ToString());
        Engine.AssetLoader.Save(fileData, $"{DATA_OBJECTS_PATH}/Intellisense.cs");
    }
}
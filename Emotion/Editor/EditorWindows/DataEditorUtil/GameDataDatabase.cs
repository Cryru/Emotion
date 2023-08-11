using Emotion.Game.World2D.EditorHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using WinApi.Kernel32;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Editor.EditorWindows.DataEditorUtil.GameDataObject>;

#nullable enable

namespace Emotion.Editor.EditorWindows.DataEditorUtil
{
    public static class GameDataDatabase
    {
        public static bool Initialized { get; private set; } = false;

        private static Dictionary<Type, Dictionary<string, GameDataObject>>? _database;

        private const string DATA_OBJECTS_PATH = "Data";

        public static async Task Load()
        {
            if (Initialized) return;

            _database = new();
            var types = EditorUtility.GetTypesWhichInherit<GameDataObject>();
            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];
                string typeName = type.Name;
                var files = Engine.AssetLoader.GetAssetsInFolder($"{DATA_OBJECTS_PATH}/{typeName}");
                var tasks = new Task<GameDataObjectAsset?>[files.Length];
                for (int j = 0; j < files.Length; j++)
                {
                    string? file = files[j];
                    tasks[j] = Engine.AssetLoader.GetAsync<GameDataObjectAsset>(file);
                }
                await Task.WhenAll(tasks);

                var objectsOfThisType = new Dictionary<string, GameDataObject>();
                for (int j = 0; j < tasks.Length; j++)
                {
                    var task = tasks[j];
                    if (task != null && task.IsCompleted && task.Result != null && task.Result.Content != null)
                    {
                        var asset = task.Result;
                        var content = asset.Content;

                        string assetName = EnsureNonDuplicatedId(content.Id, objectsOfThisType);

                        // Just in case set identifying parameters again.
                        // This should result in nothing having changed :)
                        content.Id = assetName;
                        content.AssetPath = files[j];

                        objectsOfThisType.Add(content.Id, content);

                    }
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
            var dict = GetObjectsOfType(type);
            if (dict != null) return EnsureNonDuplicatedId(name, dict);
            return name;
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
            if (!_database.ContainsKey(t)) return null;
            return _database[t];
        }

        public static bool EditorAddObject(Type t, GameDataObject obj)
        {
            if (!Initialized) return false;
            AssertNotNull(_database);

            Dictionary<string, GameDataObject> objectsOfThisType;
            if(_database.ContainsKey(t))
            {
                objectsOfThisType = _database[t];
            }
            else
            {
                objectsOfThisType = new();
                _database[t] = objectsOfThisType;
            }

            string safeId = EnsureNonDuplicatedId(obj.Id, objectsOfThisType);
            obj.Id = safeId;
            objectsOfThisType.Add(safeId, obj);

            string path = GetAssetPath(obj);
            obj.AssetPath = path;
            var asAsset = GameDataObjectAsset.CreateFromContent(obj, path);
            return asAsset.Save();
        }

        public static void EditorReIndex(Type type)
        {
            var data = GetObjectsOfType(type);
            if (data == null) return;

            // Get all objects that no longer match their key.
            List<string>? toReAdd = null;
            foreach (var objPair in data)
            {
                var obj = objPair.Value;
                var objId = obj.Id;
                if(objId != objPair.Key)
                {
                    toReAdd ??= new List<string>();
                    toReAdd.Add(objPair.Key);
                }
            }

            // Add them back as their new Id.
            if (toReAdd != null)
            {
                for (int i = 0; i < toReAdd.Count; i++)
                {
                    var objName = toReAdd[i];
                    data.Remove(objName, out GameDataObject? val);
                    AssertNotNull(val);
                    data.Add(val.Id, val);
                }
            }
        }

        public static string GetAssetPath(GameDataObject obj)
        {
            var type = obj.GetType();
 
            return $"{DATA_OBJECTS_PATH}/{type.Name}/{obj.Id}.xml";
        }
    }
}

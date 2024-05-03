#nullable enable

#region Using

using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.Data;
using Emotion.IO;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Game.Data.GameDataObject>;

#endregion

namespace Emotion.Game.Data;

public static partial class GameDataDatabase
{
    // Class used for hiding editor functions from GameDataDatabase class scope.
    public class EditorAdapter
    {
        public static bool EditorAddObject(Type type, GameDataObject obj)
        {
            if (!Initialized) return false;
            AssertNotNull(_database);

            GameDataCache? dataCache;
            if (!_database.TryGetValue(type, out dataCache))
            {
                dataCache = new(type);
                _database.Add(type, dataCache);
            }

            string safeId = dataCache.EnsureNonDuplicatedId(obj.Id);
            obj.Id = safeId;
            obj.Index = dataCache.Objects.Count;
            dataCache.Objects.Add(obj);

            EditorReIndex(type);

            string path = GetAssetPath(obj);
            obj.LoadedFromFile = path;

            // todo: maybe leave file saving to the editor :P
            GameDataObjectAsset asAsset = GameDataObjectAsset.CreateFromContent(obj, path);
            return asAsset.Save();
        }

        public static void EditorDeleteObject(Type type, GameDataObject obj)
        {
            if (!Initialized) return;
            AssertNotNull(_database);

            // todo: maybe leave file saving to the editor :P
            // Delete file and asset from cache
            string assetPath = GetAssetPath(obj);
            Engine.AssetLoader.Destroy(assetPath);
            DebugAssetStore.DeleteFile(assetPath);

            GameDataCache? dataCache;
            if (_database.TryGetValue(type, out dataCache))
                dataCache.Objects.Remove(obj);

            EditorReIndex(type);
        }

        public static void EditorReIndex(Type type)
        {
            if (!Initialized) return;
            AssertNotNull(_database);

            GameDataCache? dataCache;
            if (_database.TryGetValue(type, out dataCache))
                dataCache.RecreateIdMap();

            GenerateCode();
            UpdateCsProjFile();
        }

        public static string EnsureNonDuplicatedId(string name, Type type)
        {
            if (_database == null) return name;
            if (_database.TryGetValue(type, out GameDataCache? cache))
                return cache.EnsureNonDuplicatedId(name);

            return name;
        }

        public static string GetAssetPath(GameDataObject obj)
        {
            Type type = obj.GetType();
            while (type.BaseType != typeof(GameDataObject))
            {
                if (type.BaseType == null) // Doesn't inherit GameDataObject?!?
                {
                    Assert(false);
                    type = obj.GetType();
                    break;
                }
                else
                {
                    type = type.BaseType;
                }
            }

            return $"{DATA_OBJECTS_PATH}/{type.Name}/{obj.Id}.xml";
        }

        public static void GenerateCode()
        {
            // Don't code gen in release mode lol
            if (Engine.Configuration == null || !Engine.Configuration.DebugMode) return;

            var builder = new StringBuilder();
            builder.AppendLine("// THIS IS AN AUTO-GENERATED FILE (by GameDataDatabase.cs) TO HELP WITH INTELLISENSE");
            builder.AppendLine("");
            builder.AppendLine("namespace GameData");
            builder.AppendLine("{");
            var types = GetGameDataTypes();
            if (types != null)
                for (var i = 0; i < types.Length; i++)
                {
                    Type type = types[i];

                    if (i != 0) builder.AppendLine("");

                    builder.AppendLine($"	public static class {type.Name}Defs");
                    builder.AppendLine("	{");
                    var items = GetObjectsOfType(type);
                    if (items != null)
                        foreach (GameDataObject item in items)
                        {
                            string itemIdSafe = item.Id.Replace("-", "_");
                            builder.AppendLine($"		public static readonly string {itemIdSafe} = \"{item.Id}\";");
                        }

                    builder.AppendLine("	}");
                }

            builder.AppendLine("}");

            byte[] fileData = Encoding.UTF8.GetBytes(builder.ToString());
            Engine.AssetLoader.Save(fileData, $"{DATA_OBJECTS_PATH}/Intellisense.cs");
        }

        // Patch the csproj file to copy to output the data xml files.
        public static void UpdateCsProjFile()
        {
            // Don't code gen in release mode lol
            if (Engine.Configuration == null || !Engine.Configuration.DebugMode) return;

            var csProjFile = EditorUtility.GetCsProjFilePath();
            if (csProjFile == null) return; // No file

            string csProjFileContents = File.ReadAllText(csProjFile);

            // Clean up old item group.
            string autoAddGroupStart = "  <ItemGroup Label=\"GameDataAutoAdded\">";
            string autoAddGroupEnd = "</ItemGroup>";
            int itemGroupStart = csProjFileContents.IndexOf(autoAddGroupStart);
            if (itemGroupStart != -1)
            {
                int itemGroupEnd = csProjFileContents.IndexOf(autoAddGroupEnd, itemGroupStart);
                if (itemGroupEnd == -1) return; // Invalid file.
                itemGroupEnd = itemGroupEnd + autoAddGroupEnd.Length;
                csProjFileContents = csProjFileContents.Remove(itemGroupStart, itemGroupEnd - itemGroupStart);
            }
            else
            {
                int endOfProjectTag = csProjFileContents.IndexOf("</Project>");
                if (endOfProjectTag == -1) return; // Invalid file

                csProjFileContents = csProjFileContents.Insert(endOfProjectTag, "\n\n");
                itemGroupStart = endOfProjectTag;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(autoAddGroupStart);

            var types = GetGameDataTypes();
            if (types != null)
                for (var i = 0; i < types.Length; i++)
                {
                    var type = types[i];
                    var items = GetObjectsOfType(type);
                    if (items == null) continue;

                    foreach (GameDataObject item in items)
                    {
                        var itemPath = GetAssetPath(item);
                        var itemFilePath = $"Assets\\{itemPath.Replace("/", "\\")}";

                        // Dont add items that are referenced elsewhere in the csproj.
                        // Assuming some form of manual setting.
                        if (csProjFileContents.Contains(itemFilePath, StringComparison.OrdinalIgnoreCase)) continue;

                        builder.AppendLine($"    <None Update=\"{itemFilePath}\">");
                        builder.AppendLine("      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>");
                        builder.AppendLine("    </None>");
                    }
                }

            builder.Append("  </ItemGroup>");
            csProjFileContents = csProjFileContents.Insert(itemGroupStart, builder.ToString());
            csProjFileContents = csProjFileContents.Replace("\r\n", "\n");
            File.WriteAllText(csProjFile, csProjFileContents);
        }

        public static string[]? GetObjectIdsOfType(Type? type)
        {
            if (!Initialized) return null;
            if (type == null) return null;
            AssertNotNull(_database);

            if (_database.TryGetValue(type, out GameDataCache? cache))
                return cache.IdMap.Keys.ToArray();

            return null;
        }
    }
}
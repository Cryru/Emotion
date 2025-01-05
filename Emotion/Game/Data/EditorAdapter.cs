#nullable enable

#region Using

using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.Data;
using Emotion.IO;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform;
using GameDataObjectAsset = Emotion.IO.XMLAsset<Emotion.Game.Data.GameDataObject>;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;

#endregion

namespace Emotion.Game.Data;

public static partial class GameDataDatabase
{
    // Class used for hiding editor functions from GameDataDatabase class scope.
    public static class EditorAdapter
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

            string generatedClassPath = GetGeneratedClassPathOSPath(obj);
            string? directory = Path.GetDirectoryName(generatedClassPath);
            AssertNotNull(directory);
            Directory.CreateDirectory(directory);

            string classCode = GetClassGenShimCode(type, obj);
            File.WriteAllText(generatedClassPath, classCode);

            return true;

            //string path = GetAssetPath(obj);
            //obj.LoadedFromFile = path;

            //// todo: maybe leave file saving to the editor :P
            //GameDataObjectAsset asAsset = GameDataObjectAsset.CreateFromContent(obj, path);
            //return asAsset.Save();
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

            //GenerateCode();
            //UpdateCsProjFile();
        }

        public static string EnsureNonDuplicatedId(string name, Type type)
        {
            if (_database == null) return name;
            if (_database.TryGetValue(type, out GameDataCache? cache))
                return cache.EnsureNonDuplicatedId(name);

            return name;
        }

        public static string GetClassGenShimCode(Type type, GameDataObject obj)
        {
            string className = $"{type.Name}_{obj.Id}";
            return $"namespace GameData;\n" +
                $"\n" +
                $"public static partial class {type.Name}Defs\n" +
                $"{{\n" +
                $"    public static {className} {obj.Id} = new();\n" +
                $"\n" +
                $"    public class {className} : {type.FullName}\n" +
                $"    {{\n" +
                $"        public {className}()\n" +
                $"        {{\n" +
                $"{GetClassGenConstructorCode(obj)}\n" +
                $"        }}\n" +
                $"    }}\n" +
                $"}}";
        }

        private static string GetClassGenConstructorCode(GameDataObject obj)
        {
            StringBuilder builder = new StringBuilder();

            var reflectorHandler = ReflectorEngine.GetTypeHandler(obj.GetType()) as IGenericReflectorComplexTypeHandler;
            AssertNotNull(reflectorHandler);

            ComplexTypeHandlerMember[]? members = reflectorHandler.GetMembers();
            AssertNotNull(members);

            bool first = true;
            foreach (ComplexTypeHandlerMember member in members)
            {
                if(!first) builder.Append("\n");
                first = false;

                builder.Append("            ");
                builder.Append($"{member.Name} = ");

                IGenericReflectorTypeHandler? memberHandler = member.GetTypeHandler();
                AssertNotNull(memberHandler);

                if (member.GetValueFromComplexObject(obj, out object? memberValue))
                {
                    if (memberValue == null)
                    {
                        builder.Append("null");
                    }
                    else
                    {
                        bool isString = memberHandler.Type == typeof(string);

                        if (isString)
                            builder.Append("\"");

                        memberHandler.WriteValueAsStringGeneric(builder, memberValue);

                        if (isString)
                            builder.Append("\"");
                    }
                    
                }

                builder.Append($";");
            }

            return builder.ToString();
        }

        public static string GetGeneratedClassPathOSPath(GameDataObject obj)
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

            PlatformBase host = Engine.Host;
            if (host is DesktopPlatform desktopHost)
            {
                string projectFolder = desktopHost.DeveloperMode_GetProjectFolder();
                if (projectFolder != "")
                    return Path.Join(projectFolder, "GameData", type.Name, obj.Id) + ".cs";
            }

            Assert(false, "Trying to get generated class path on a non-developer platform");
            return "";
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

            string dataCopyString = $"      <None Update=\"Assets\\**\\*.*\">\r\n        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\r\n      </None>";
            if (csProjFileContents.Contains(dataCopyString)) return; // Already present

            // If missing - insert it
            int endOfProjectTag = csProjFileContents.IndexOf("</Project>");
            if (endOfProjectTag == -1) return; // Invalid file

            StringBuilder itemGroup = new StringBuilder();
            itemGroup.AppendLine("");
            itemGroup.AppendLine("    <ItemGroup Label=\"GameDataAutoAdded\">");
            itemGroup.AppendLine(dataCopyString);
            itemGroup.AppendLine("    </ItemGroup>");
            csProjFileContents = csProjFileContents.Insert(endOfProjectTag - 1, itemGroup.ToString());
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
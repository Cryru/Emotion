#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Serialization.XML;
using System.IO;
using GameDataObjectAsset = Emotion.Core.Systems.IO.XMLAsset<Emotion.Game.Systems.GameData.GameDataObject>;

#endregion

namespace Emotion.Game.Systems.GameData;

public static partial class GameDatabase
{
    // Class used for hiding editor functions from GameDatabase class scope.
    public static class EditorAdapter
    {
        public static GameDataObject? CreateNew(Type type, List<GameDataObject> objs)
        {
            IGenericReflectorComplexTypeHandler? handler = ReflectorEngine.GetComplexTypeHandler(type);
            if (handler == null) return null;

            object? obj = handler.CreateNew();
            if (obj == null || obj is not GameDataObject dataObj) return null;

            dataObj.Id = EditorAdapter.EnsureNonDuplicatedId(dataObj.Id, objs);
            dataObj.Index = objs.Count;
            return dataObj;
        }

        public static void DeleteData(Type type, List<GameDataObject> objs)
        {
            string? assetFolder = GetOSAssetsFolder(type);
            string? codeFolder = GetCodeFolder(type);
            foreach (GameDataObject obj in objs)
            {
                string assetFile = Path.Join(assetFolder, $"{obj.Id}.xml");
                if (File.Exists(assetFile)) File.Delete(assetFile);

                string codeFile = Path.Join(codeFolder, $"{obj.Id}.cs");
                if (File.Exists(codeFile)) File.Delete(codeFile);

                if (_database.TryGetValue(type, out GameDataTable? table))
                {
                    table.DeleteObject(obj);
                }
            }
        }

        public static void SaveChanges(Type type, List<GameDataObject> objs)
        {
            _database.TryGetValue(type, out GameDataTable? typeTable);
            AssertNotNull(typeTable);

            string assetFolder = $"{ASSETS_DATA_FOLDER}/{type.Name}";
            string? codeFolder = GetCodeFolder(type);
            foreach (GameDataObject obj in objs)
            {
                GameDataObjectAsset ass = GameDataObjectAsset.CreateFromContent(obj);
                ass.SaveAs($"{assetFolder}/{obj.Id}.xml", false);

                EnsureObjClassFile(codeFolder, type, obj);

                string xml = XMLSerialization.To(obj);
                GameDataObject? backToObject = XMLSerialization.From<GameDataObject>(xml);
                AssertNotNull(backToObject);
                if (backToObject != null && typeTable != null)
                    typeTable.ReplaceObject(backToObject);
            }
        }

        private static void EnsureObjClassFile(string? codeFolder, Type dataType, GameDataObject obj)
        {
            if (codeFolder == null) return;

            string codeFile = Path.Join(codeFolder, $"{obj.Id}.cs");
            if (File.Exists(codeFile)) return;

            Directory.CreateDirectory(codeFolder);
            using FileStream fileStream = File.Open(codeFile, FileMode.Create);
            using StreamWriter writer = new StreamWriter(fileStream);

            string className = obj.Id;
            writer.WriteLine($"using DataType = global::{dataType.FullName};");
            writer.WriteLine();
            writer.WriteLine($"namespace GameData.{dataType.Name}Data;");
            writer.WriteLine();
            writer.WriteLine($"public partial class {dataType.Name}Defs");
            writer.WriteLine("{");
            writer.WriteLine($"    public static {className} {obj.Id} {{ get => {nameof(GameDatabase)}.{nameof(GameDatabase.GetObject)}<{className}>(\"{obj.Id}\")!; }}");
            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine($"public class {className} : DataType");
            writer.WriteLine("{");
            writer.WriteLine();
            writer.WriteLine("}");
        }

        public static string EnsureNonDuplicatedId(string name, IList<GameDataObject> definitions)
        {
            HashSet<string> idMap = new HashSet<string>();
            foreach (GameDataObject def in definitions)
            {
                idMap.Add(def.Id);
            }

            var counter = 1;
            string originalName = name;
            while (idMap.Contains(name)) name = originalName + "_" + counter++;
            return name;
        }

        private static string? GetOSAssetsFolder(Type typ)
        {
            if (AssetLoader.CanWriteAssets)
            {
                string projectFolder = AssetLoader.DevModeAssetFolder;
                if (!string.IsNullOrEmpty(projectFolder))
                    return Path.Join(projectFolder, ASSETS_DATA_FOLDER, typ.Name);
                else
                    return null;
            }
            return null;
        }

        private static string? GetCodeFolder(Type typ)
        {
            if (AssetLoader.CanWriteAssets)
            {
                string projectFolder = AssetLoader.DevModeProjectFolder;
                if (!string.IsNullOrEmpty(projectFolder))
                    return Path.Join(projectFolder, CLASS_DATA_FOLDER, typ.Name);
                else
                    return null;
            }

            Assert(false, "Trying to get generated class path on a non-developer platform");
            return null;
        }
    }
}
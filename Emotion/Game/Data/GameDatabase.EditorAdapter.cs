#nullable enable

#region Using

using System.IO;
using System.Text;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.WIPUpdates.One;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Utility.OptimizedStringReadWrite;

#endregion

namespace Emotion.Game.Data;

public static partial class GameDatabase
{
    // Class used for hiding editor functions from GameDataDatabase class scope.
    public static class EditorAdapter
    {
        private const string DATA_OBJECTS_PATH = "GameData"; // Project folder scoped
        private const string GENERATED_CODE_START = "#region Code Generated (DONT EDIT)";
        private const string GENERATED_CODE_END = "#endregion";
        private const string DEF_TYPE_ALL_DEFINITIONS = "AllDefinitions";
        private const string DATA_TYPE_CREATE_MODEL_FUNC = "CreateInstance";

        public static GameDataObject? CreateNew(Type typ)
        {
            // Editor functions dont work in release mode!
            if (Engine.Configuration == null || !Engine.Configuration.DebugMode)
                return null;

            if (!Initialized)
                return null;

            string undefinedClass = $"Undefined{typ.Name}Class";
            IGenericReflectorTypeHandler? handler = ReflectorEngine.GetTypeHandlerByName(undefinedClass);
            if (handler is not IGenericReflectorComplexTypeHandler complexHandler)
                return null;

            object? newObj = complexHandler.CreateNew();
            if (newObj == null)
                return null;

            return (GameDataObject?)newObj;
        }

        public static void SaveChanges(Type typ, List<GameDataObject> dataToSave)
        {
            // This game data type's folder.
            string folder = GetGeneratedClassPathOSPath(typ);
            Directory.CreateDirectory(folder);

            bool needHotReload = false;
            bool regenerateRegistry = false;
            foreach (GameDataObject data in dataToSave)
            {
                string thisDataPath = Path.Join(folder, $"{data.Id}.cs");
                if (data.LoadedFromModel == null) // New data - generate class
                {
                    data.LoadedFromModel = data.Id;

                    string classCode = GenerateGameDataClassWithShim(typ, data);
                    File.WriteAllText(thisDataPath, classCode);
                    needHotReload = true;

                    // Add new to registry
                    GameDataObject[] arr = GetObjectsOfType(typ);
                    arr = arr.AddToArray(data);
                    _definedData[typ] = arr;

                    // Update def class runtime and regenerate its code.
                    // The adapter will be missing if this is the first game data of this type,
                    // and it has never been hot reloaded.
                    IGameDataDefClassAdapter? defAdapter = GetGameDataDefAdapter(typ);
                    defAdapter?.AddObject(data);
                    regenerateRegistry = true;
                    
                    EngineEditor.ReportChange_ObjectProperty(data, nameof(GameDataObject.LoadedFromModel), null, data.Id);
                }
                else
                {
                    // Just properties have changed, or id as well.
                    string oldLoadedFromModel = data.LoadedFromModel;
                    bool idHasChanged = data.LoadedFromModel != data.Id;
                    if (idHasChanged)
                    {
                        string oldFile = Path.Join(folder, $"{data.LoadedFromModel}.cs");
                        bool oldFileExists = File.Exists(oldFile);
                        Assert(oldFileExists); // The old file should exist
                        if (!oldFileExists)
                            continue;

                        bool newFileExists = File.Exists(thisDataPath);
                        Assert(!newFileExists); // Id renamed to already has a generated file
                        if (newFileExists)
                            continue;

                        File.Move(oldFile, thisDataPath);

                        data.LoadedFromModel = data.Id;
                        EngineEditor.ReportChange_ObjectProperty(data, nameof(GameDataObject.LoadedFromModel), oldLoadedFromModel, data.Id);

                        regenerateRegistry = true;
                        needHotReload = true;
                    }

                    bool dataFileExists = File.Exists(thisDataPath);
                    Assert(dataFileExists);
                    if (!dataFileExists) // This should never happen, but prevent crash
                        continue;

                    // todo: dont read the whole file into memory, use streams.
                    string fileContent = File.ReadAllText(thisDataPath);
                    int generatedCodeStart = fileContent.IndexOf(GENERATED_CODE_START);
                    int generatedCodeEnd = fileContent.IndexOf(GENERATED_CODE_END, generatedCodeStart);

                    bool validSegment = generatedCodeStart != -1 && generatedCodeEnd != -1;
                    Assert(validSegment);
                    if (!validSegment)
                        continue;

                    ReadOnlySpan<char> fileContentSpan = fileContent.AsSpan();
                    using var writeStream = File.Open(thisDataPath, FileMode.Truncate);
                    using var writer = new StreamWriter(writeStream);

                    writer.Write(fileContentSpan.Slice(0, generatedCodeStart));
                    writer.Write(GenerateGameDataClass(typ, data));
                    writer.Write(fileContentSpan.Slice(generatedCodeEnd + GENERATED_CODE_END.Length));

                    // Recreate the model so its up to date with the changes.
                    // (use the old "loaded from model" value since its not updated yet.
                    GameDataObject? model = GetObject(typ, oldLoadedFromModel);
                    ReflectorEngine.CopyProperties(data, model);
                }
            }

            // The user needs to hot reload via visual studio.
            // This is because new classes have been generated
            if (needHotReload)
            {
                string defClass = GetGameDataTypeDefClassName(typ);
                if (_typesNeedHotReload.IndexOf(defClass) == -1)
                {
                    _typesNeedHotReload.Add(defClass);
                    OnHotReloadNeededChange?.Invoke(_typesNeedHotReload);
                }
            }

            // Update registry (aka new files added)
            if (regenerateRegistry)
            {
                // Resave registry file
                string masterFile = GenerateRegistryFile(typ);
                File.WriteAllText(Path.Join(folder, $"__Registry.cs"), masterFile);
            }
        }

        public static void EditorDeleteObject(Type typ, GameDataObject data)
        {
            // Editor functions dont work in release mode!
            if (Engine.Configuration == null || !Engine.Configuration.DebugMode)
                return;
            if (!Initialized) return;

            string folder = GetGeneratedClassPathOSPath(typ);
            string thisDataPath = Path.Join(folder, $"{data.LoadedFromModel}.cs");
            bool fileExists = File.Exists(thisDataPath);
            if (!fileExists) return;

            // Update the loaded object array
            GameDataObject[] objects = GetObjectsOfType(typ);
            int index = -1;
            for (int i = 0; i < objects.Length; i++)
            {
                GameDataObject? obj = objects[i];
                if (obj.Id == data.LoadedFromModel)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1) return;
            GameDataObject[] newArray = objects.RemoveFromArray(index);
            _definedData[typ] = newArray;

            string deletedFolder = Path.Join(folder, "___Deleted");
            Directory.CreateDirectory(deletedFolder);

            string deletedFilePath = Path.Join(deletedFolder, $"{data.LoadedFromModel}.txt");
            int ii = 1;
            while (File.Exists(deletedFilePath))
            {
                deletedFilePath = Path.Join(deletedFolder, $"{data.LoadedFromModel}_{ii}.txt");
                ii++;
            }
            File.Move(thisDataPath, deletedFilePath);

            // Regenerate the registry file
            string masterFile = GenerateRegistryFile(typ);
            File.WriteAllText(Path.Join(folder, $"__Registry.cs"), masterFile);

            string defClass = GetGameDataTypeDefClassName(typ);
            if (_typesNeedHotReload.IndexOf(defClass) == -1)
            {
                _typesNeedHotReload.Add(defClass);
                OnHotReloadNeededChange?.Invoke(_typesNeedHotReload);
            }
        }

        #region Hot Reload

        private static List<string> _typesNeedHotReload = new List<string>();

        /// <summary>
        /// Fired when the "need for some types to be hot reloaded" changes
        /// </summary>
        public static event Action<List<string>>? OnHotReloadNeededChange;
        
        internal static void OnHotReload(Type[]? typesUpdated)
        {
            if (typesUpdated == null) return;

            bool changes = false;
            foreach (Type typ in typesUpdated)
            {
                // Check if type we care about - which are game data def classes.
                if (!_typesNeedHotReload.Remove(typ.Name))
                    continue;
                changes = true;

                // Get the def type adapter
                IGameDataDefClassAdapter? defAdapter = GetGameDataDefAdapter(typ);
                if (defAdapter == null)
                {
                    // This was the registry's first generation.
                    IGenericReflectorComplexTypeHandler? adapterTypeHandler = ReflectorEngine.GetComplexTypeHandlerByName($"{typ.Name}EditorAdapter");
                    if (adapterTypeHandler != null)
                    {
                        defAdapter = (IGameDataDefClassAdapter?) adapterTypeHandler.CreateNew();
                        if (defAdapter != null) RegisterGameDataDefClassAdapter(defAdapter);
                    }
                }
                AssertNotNull(defAdapter);
                if (defAdapter == null) // HUH
                    continue;

                // Regenerate the static list
                GameDataObject[] newArray = defAdapter.ReloadList();

                // Update the runtime list with the new static list
                Type gameDataType = defAdapter.GetGameDataType();
                _definedData[gameDataType] = newArray;
            }

            if (changes)
                OnHotReloadNeededChange?.Invoke(_typesNeedHotReload);
        }

        #endregion

        #region Code Gen

        private static string GenerateGameDataClassWithShim(Type type, GameDataObject obj)
        {
            return $"using Emotion.Standard.Reflector;\n" +
                $"\n" +
                $"namespace GameData;\n" +
                $"\n" +
                $"public static partial class {GetGameDataTypeDefClassName(type)}\n" +
                $"{{\n" +
                $"    {GenerateGameDataClass(type, obj)}\n" +
                $"\n" +
                $"    {{\n" +
                $"        // Place custom code and stuff the generator won't touch here.\n" +
                $"    }}\n" +
                $"}}";
        }

        private static string GenerateGameDataClass(Type type, GameDataObject obj)
        {
            string className = $"{obj.Id}_Class";
            return $"{GENERATED_CODE_START}\n" +
                $"    public partial class {className}\n" +
                $"    {{\n" +
                $"        // The truth for property values of this data object.\n" +
                $"        public static {className} {DATA_TYPE_CREATE_MODEL_FUNC}()\n" +
                $"        {{\n" +
                $"            return new {className}()\n" +
                $"{GetClassGenConstructorCode(obj)};\n" +
                $"        }}\n" +
                $"\n" +
                $"        // Create a new instance of this data object.\n" +
                $"        public override {className} {nameof(GameDataObject.CreateCopy)}()\n" +
                $"        {{\n" +
                $"            var newInstance = new {className}();\n" +
                $"            {nameof(ReflectorEngine)}.{nameof(ReflectorEngine.CopyProperties)}(this, newInstance);\n" +
                $"            return newInstance;\n" +
                $"        }}\n" +
                $"\n" +
                $"        // Prevent initialization by game code\n" +
                $"        protected {className}()\n" +
                $"        {{\n" +
                $"        }}\n" +
                $"    }}\n" +
                $"\n" +
                $"    public partial class {className} : global::{type.FullName}\n" +
                $"    {GENERATED_CODE_END}";
        }

        private static string GetClassGenConstructorCode(GameDataObject obj) // todo: move to serialization
        {
            IGenericReflectorComplexTypeHandler? reflectorHandler = ReflectorEngine.GetComplexTypeHandler(obj.GetType());
            AssertNotNull(reflectorHandler);

            StringBuilder builder = new StringBuilder();
            ValueStringWriter writer = new ValueStringWriter(builder);
            reflectorHandler.WriteAsCode(obj, ref writer);
            return builder.ToString();
        }

        // todo: can this be moved to reflector?
        // yes: we can generate editor adapters always and not have a different case for "the first time game data is created"
        // maybe: however can we do the whole "all definitions" thing like that?
        // yes: less code to generate here the better
        // yes: we can get rid of the static handler and use the adapter to get the list for normal initialization too and the adapter init in hot reload
        private static string GenerateRegistryFile(Type type)
        {
            GameDataObject[] definitions = GetObjectsOfType(type);

            StringBuilder definitionsList = new StringBuilder(definitions.Length * 10);
            for (int i = 0; i < definitions.Length; i++)
            {
                var def = definitions[i];
                definitionsList.Append("            ");
                definitionsList.Append(def.Id);
                definitionsList.Append(",\n");
            }

            StringBuilder modelDeclarations = new StringBuilder(definitions.Length * 10);
            for (int i = 0; i < definitions.Length; i++)
            {
                GameDataObject def = definitions[i];
                modelDeclarations.Append($"    public static {def.Id}_Class {def.Id} {{ get; private set; }}\n");
            }

            StringBuilder definitionInitializations = new StringBuilder(definitions.Length * 10);
            for (int i = 0; i < definitions.Length; i++)
            {
                GameDataObject def = definitions[i];
                definitionInitializations.Append($"        {def.Id} = {def.Id}_Class.{DATA_TYPE_CREATE_MODEL_FUNC}();\n");
            }

            string defClassName = GetGameDataTypeDefClassName(type);
            string dataClassName = type.FullName ?? string.Empty;
            string editorAdapterName = $"{defClassName}EditorAdapter";

            return "using System;\n" +
                "using Emotion.Game.Data;\n" +
                $"using static Emotion.{nameof(Emotion.Game)}.{nameof(Emotion.Game.Data)}.{nameof(GameDatabase)}.{nameof(EditorAdapter)};" +
                "\n" +
                "namespace GameData;\n" +
                "\n" +
                "[System.CodeDom.Compiler.GeneratedCode(\"Emotion Game Data Generator\", \"1.0\")]\n" +
                $"[Emotion.Standard.Reflector.{nameof(ReflectorStaticClassSupportAttribute)}]\n" +
                $"public static partial class {defClassName}\n" +
                "{\n" +
                modelDeclarations +
                "\n" +
                $"    public static {dataClassName}[] {DEF_TYPE_ALL_DEFINITIONS}\n" +
                "    {\n" +
                "        get => _allDefs;\n" +
                "        set\n" +
                "        {\n" +
                "        }\n" +
                "    }\n" +
                $"    private static {dataClassName}[] _allDefs = CreateAllDefinitions();\n" +
                $"\n" +
                $"    private static {dataClassName}[] CreateAllDefinitions()\n" +
                "    {\n" +
                definitionInitializations +
                "\n" +
                "        return [\n" +
                definitionsList +
                "        ];\n" +
                "    }\n" +
                "\n" +
                $"    static {defClassName}()\n" +
                "    {\n" +
                $"        {nameof(GameDatabase.EditorAdapter.RegisterGameDataDefClassAdapter)}(new {editorAdapterName}());\n" +
                "    }\n" +
                "\n" +
                $"    public class {editorAdapterName} : {nameof(IGameDataDefClassAdapter)}\n" +
                "    {\n" +
                $"        public Type {nameof(IGameDataDefClassAdapter.GetGameDataType)}()\n" +
                "        {\n" +
                $"            return typeof({dataClassName});\n" +
                "        }\n" +
                "\n" +
                $"        public {nameof(GameDataObject)}[] {nameof(IGameDataDefClassAdapter.AddObject)}({nameof(GameDataObject)} gm)\n" +
                "        {\n" +
                $"            _allDefs = _allDefs.{nameof(ArrayExtensions.AddToArray)}(({dataClassName})gm);\n" +
                $"            return _allDefs;\n" +
                "        }\n" +
                "\n" +
                $"        public {nameof(GameDataObject)}[] {nameof(IGameDataDefClassAdapter.ReloadList)}()\n" +
                "        {\n" +
                $"            _allDefs = CreateAllDefinitions();\n" +
                $"            return _allDefs;\n" +
                "        }\n" +
                "    }\n" +
                "}";
        }

        #endregion

        #region Helpers

        public static string GetGameDataTypeDefClassName(Type gameDataType)
        {
            return $"{gameDataType.Name}Defs";
        }

        public static ComplexTypeHandlerMemberBase? GetStaticAllDefinitionsMember(Type typ)
        {
            string defTypName = EditorAdapter.GetGameDataTypeDefClassName(typ);
            return GetStaticAllDefinitionsMember(defTypName);
        }

        private static ComplexTypeHandlerMemberBase? GetStaticAllDefinitionsMember(string defClassName)
        {
            var handler = ReflectorEngine.GetTypeHandlerByName(defClassName);
            if (handler is StaticComplexTypeHandler staticTypeHandler)
            {
                ComplexTypeHandlerMemberBase? member = staticTypeHandler.GetMemberByName(DEF_TYPE_ALL_DEFINITIONS);
                AssertNotNull(member);
                return member;
            }
            return null;
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

        private static string GetGeneratedClassPathOSPath(Type typ)
        {
            PlatformBase host = Engine.Host;
            if (host is DesktopPlatform desktopHost)
            {
                string projectFolder = desktopHost.DeveloperMode_GetProjectFolder();
                if (projectFolder != "")
                    return Path.Join(projectFolder, DATA_OBJECTS_PATH, typ.Name);
            }

            Assert(false, "Trying to get generated class path on a non-developer platform");
            return "";
        }

        #endregion

        #region Def Class Adapter

        // Will be called on game data Initialize since AllDefinitions uses the class, which will
        // call its static constructor and cause an adapter to be added.

        private static Dictionary<Type, IGameDataDefClassAdapter> _dataTypeToDefEditorAdapter = new();

        public interface IGameDataDefClassAdapter
        {
            public Type GetGameDataType();

            public GameDataObject[] AddObject(GameDataObject gm);

            public GameDataObject[] ReloadList();
        }

        public static void RegisterGameDataDefClassAdapter(IGameDataDefClassAdapter adapter)
        {
            Type type = adapter.GetGameDataType();
            _dataTypeToDefEditorAdapter[type] = adapter;
        }

        private static IGameDataDefClassAdapter? GetGameDataDefAdapter(Type typ)
        {
            if (_dataTypeToDefEditorAdapter.TryGetValue(typ, out IGameDataDefClassAdapter? adapter))
                return adapter;
            return null;
        }

        #endregion
    }
}
using Emotion.CodeSerializationLib;
using System;
using System.IO;
using System.Reflection;
using System.Text;

// v1.0

// Game data format

// DataClass (ex. MyGame.Data.Monster)
// Id (ex. RawrMon)
// PropertyName = PropertyText

namespace Emotion.GameDataLib
{
    public static class GameDataGenerator
    {
        public static string WriteClassDef(StringBuilder writeTo, Stream gameDataFileStream)
        {
            using (StreamReader reader = new StreamReader(gameDataFileStream))
            {
                return WriteClassDef(writeTo, reader);
            }
        }

        public static string WriteClassDef(StringBuilder writeTo, string gameDataFileContent)
        {
            using (StringReader reader = new StringReader(gameDataFileContent))
            {
                return WriteClassDef(writeTo, reader);
            }
        }

        public static string WriteClassDef(StringBuilder writeTo, TextReader reader)
        {
            string dataClass = reader.ReadLine();
            string dataId = reader.ReadLine();

            string dataClassShorthand = dataClass.Substring(dataClass.LastIndexOf('.') + 1);

            writeTo.AppendLine("// Generated File");
            writeTo.AppendLine($"namespace GameData.{dataClassShorthand}Defs;");
            writeTo.AppendLine();

            writeTo.AppendLine($"public partial class {dataId} : global::{dataClass}");
            writeTo.AppendLine("{");

            writeTo.AppendLine($"    public {dataId}()");
            writeTo.AppendLine("    {");

            const string PROP_START_TAG = "prop_start";
            int propStartLength = PROP_START_TAG.Length;
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;
                writeTo.Append("        ");
                writeTo.AppendLine(line);
            }

            writeTo.AppendLine("    }");
            writeTo.AppendLine("}");

            return $"{dataClass}.{dataId}";
        }

        public static void WriteSerialized(StringBuilder writeTo, object gameData)
        {
            Type typ = gameData.GetType();
            Type oneUnder = typ.BaseType;
            if (oneUnder.Name != "GameDataObject")
                typ = oneUnder;

            writeTo.AppendLine(typ.FullName.ToString());
            writeTo.AppendLine(GetGameDataId(gameData));

            CodeSerialization.Serialize(0, writeTo, typ, gameData);
        }

        private static string GetGameDataId(object obj)
        {
            Type typ = obj.GetType();
            FieldInfo field = typ.GetField("Id");
            return (string)field.GetValue(obj);
        }
    }
}

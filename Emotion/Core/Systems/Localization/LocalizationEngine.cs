#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Editor;
using System.Text;

namespace Emotion.Core.Systems.Localization;

public static class LocalizationEngine
{
    public static LocalizationOption[] GetLocalizationOptions()
    {
        return GameDatabase.GetObjectsOfType<LocalizationOption>();
    }

    private const string _masterFilename = "Localization/MasterFile.csv";
    private static Dictionary<string, string>? _masterFile;

    private static Dictionary<string, string>? _currentLanguageDict;

    public static void Initialize()
    {
        if (Engine.Configuration.DebugMode && Engine.AssetLoader.Exists(_masterFilename))
        {
            var masterFile = Engine.AssetLoader.Get<TextAsset>(_masterFilename);
            string masterFileContent = masterFile?.Content ?? string.Empty;
            _masterFile = LocalizationCSVToDictionary(masterFileContent);
        }
    }

    public static bool SetLanguage(LocalizationOption option)
    {
        if (option is DefaultLanguage || string.IsNullOrEmpty(option.LocalizationCSVFile))
        {
            _currentLanguageDict = null;
            return false;
        }

        var fileAsset = Engine.AssetLoader.Get<TextAsset>(option.LocalizationCSVFile);
        if (fileAsset?.Content == null)
        {
            _currentLanguageDict = null;
            return false;
        }

        _currentLanguageDict = LocalizationCSVToDictionary(fileAsset.Content);
        return true;
    }

    public static string LocalizeString(string myStr)
    {
        if (string.IsNullOrEmpty(myStr)) return myStr;

        if (Engine.Configuration.DebugMode && AssetLoader.CanWriteAssets)
        {
            if (float.TryParse(myStr, out float _)) return myStr;

            AssertNotNull(_masterFile);

            // Check if the string is present in the master file.
            string myStrSafe = MakeStringCSVSafe(myStr);
            if (!_masterFile.ContainsKey(myStrSafe))
            {
                Engine.Log.Warning($"Found string missing from master localization file - {myStrSafe}!", "Localization");
                _masterFile.Add(myStrSafe, myStrSafe);
                Engine.AssetLoader.Save(Encoding.Default.GetBytes(DictionaryToLocalizationCSV(_masterFile)), _masterFilename);
            }
        }

        if (_currentLanguageDict == null) return myStr;
        if (_currentLanguageDict != null)
        {
            string myStrSafe = MakeStringCSVSafe(myStr);
            if (_currentLanguageDict.TryGetValue(myStrSafe, out string? translated))
                return translated;
        }

        return myStr;
    }

    private static string MakeStringCSVSafe(string str)
    {
        Assert(Engine.Configuration.DebugMode);
        str = str.ReplaceLineEndings("<newline>");
        str = str.Replace("|", "");
        return str;
    }

    private static Dictionary<string, string> LocalizationCSVToDictionary(string fileContent)
    {
        var dict = new Dictionary<string, string>();

        ReadOnlySpan<char> stringSpan = fileContent.AsSpan();
        while (stringSpan.Length > 0)
        {
            int nextNewLine = stringSpan.IndexOf("\n", StringComparison.InvariantCulture);
            ReadOnlySpan<char> thisLine;
            if (nextNewLine == -1)
                thisLine = stringSpan;
            else
                thisLine = stringSpan.Slice(0, nextNewLine);

            if (thisLine.Length == 0) break;

            int splitIndex = thisLine.IndexOf("|");
            ReadOnlySpan<char> key = thisLine.Slice(0, splitIndex);
            ReadOnlySpan<char> value = thisLine.Slice(splitIndex + 1);
            dict.Add(key.ToString(), value.ToString());

            if (stringSpan.Length == thisLine.Length) break;
            stringSpan = stringSpan.Slice(thisLine.Length + 1);
        }

        return dict;
    }

    private static string DictionaryToLocalizationCSV(Dictionary<string, string> dict)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in dict)
        {
            sb.Append(item.Key);
            sb.Append("|");
            sb.Append(item.Value);
            sb.Append("\n");
        }
        return sb.ToString();
    }
}

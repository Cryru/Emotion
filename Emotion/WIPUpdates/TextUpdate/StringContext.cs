#nullable enable

using Emotion;
using Emotion.Game.Localization;
using Emotion.Utility;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Emotion.WIPUpdates.TextUpdate;

public sealed class StringContext
{
    private Dictionary<string, object>? _data;
    private Action? _onParamChange;
    private IStringTagResolvingObject? _resolveObj;

    public StringContext()
    {

    }

    public StringContext(Action onParamChange)
    {
        _onParamChange = onParamChange;
    }

    public void SetResolveObject(IStringTagResolvingObject obj)
    {
        _resolveObj = obj;
    }

    public void SetParam(string name, object val)
    {
        _data ??= new Dictionary<string, object>();

        bool changed = false;
        if (_data.TryGetValue(name, out object? storedVal))
        {
            if (!Helpers.AreObjectsEqual(val, storedVal))
            {
                _data[name] = val;
                changed = true;
            }
        }
        else
        {
            _data.Add(name, val);
            changed = true;
        }
        if (changed) _onParamChange?.Invoke();
    }

    public string ResolveParam(string name, bool localize = false)
    {
        if (name == "newline") return "\n";

        object? val = null;
        if (_resolveObj != null)
            val = _resolveObj.ResolveParam(this, name);

        if (val == null)
        {
            if (_data == null || !_data.TryGetValue(name, out val)) return "Unknown Param";
        }

        if (val is TranslatedString transStr)
        {
            string? stringData = transStr.StringData;
            if (stringData == null) return string.Empty;
            if (localize) stringData = LocalizationEngine.LocalizeString(stringData);
            return ResolveString(stringData);
        }

        return val?.ToString() ?? string.Empty;
    }

    public string ResolveString(string input, bool localize = false)
    {
        if (localize) input = LocalizationEngine.LocalizeString(input);

        ReadOnlySpan<char> inputSpan = input.AsSpan();
        int start, end;
        int currentIndex = 0;
        int length = input.Length;
        StringBuilder output = new StringBuilder();

        while (currentIndex < length)
        {
            start = input.IndexOf('<', currentIndex);
            if (start == -1)
            {
                // No more tags, copy the rest of the string
                output.Append(inputSpan.Slice(currentIndex));
                break;
            }

            // Copy up to the tag
            int textLength = start - currentIndex;
            output.Append(inputSpan.Slice(currentIndex, textLength));

            end = input.IndexOf('>', start);
            if (end == -1)
            {
                currentIndex++;
                continue;
            }

            // Process the tag
            ReadOnlySpan<char> tagContent = inputSpan.Slice(start + 1, end - start - 1);
            string processedTag = ResolveParam(tagContent.AsString(), localize);

            // Copy processed tag into output
            output.Append(processedTag);

            // Move current index past the end of the tag
            currentIndex = end + 1;
        }

        return output.ToString();
    }
}

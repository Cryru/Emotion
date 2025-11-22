#nullable enable

namespace Emotion.Primitives.DataStructures;

/// <summary>
/// An NTree whose branch data type is string.
/// Implements the various NTree APIs with spans to prevent string allocations.
/// </summary>
public class NTreeString<T> : NTree<string, T>
{
    public NTreeString() : base()
    {

    }

    protected NTreeString(string name, NTreeString<T> parent) : base(name, parent)
    {
    }

    public string GetFullPathToRoot(char sep, ReadOnlySpan<char> appendAtEnd, bool toLower)
    {
        var pathStack = new Stack<NTree<string, T>>();

        int strLength = appendAtEnd.Length;
        NTree<string, T>? current = this;
        while (current != null)
        {
            pathStack.Push(current);
            if (current.Name != null)
                strLength += current.Name.Length + 1;
            current = current.Parent;
        }

        Span<char> str = stackalloc char[strLength];

        Span<char> strCur = str;
        while (pathStack.TryPop(out NTree<string, T>? branch))
        {
            string? branchName = branch.Name;
            if (branchName == null) continue;

            int written = branchName.AsSpan().ToLowerInvariant(strCur);
            if (written == -1)
            {
                Assert(false, "Error at NTreeString.GetFullPathToRoot");
                return string.Empty;
            }
            strCur[written] = sep;
            strCur = strCur.Slice(written + 1);
        }

        appendAtEnd.ToLowerInvariant(strCur);
        return str.ToString();
    }

    public NTreeString<T>? GetBranch(ReadOnlySpan<char> branchName, bool caseInsensitive = true)
    {
        StringComparison compareType = caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        foreach (NTreeString<T> subBranch in Branches)
        {
            if (branchName.Equals(subBranch.Name, compareType))
                return subBranch;
        }

        return null;
    }

    public NTreeString<T>? GetBranchFromPath(ReadOnlySpan<char> branchStr, ReadOnlySpan<char> sep, bool skipFinal, out ReadOnlySpan<char> last, bool caseInsensitive = true)
    {
        last = ReadOnlySpan<char>.Empty;

        NTreeString<T>? current = this;
        MemoryExtensions.SpanSplitEnumerator<char> spanEnum = branchStr.SplitAny(sep);
        foreach (Range part in spanEnum)
        {
            if (skipFinal && part.End.Value == branchStr.Length)
            {
                last = branchStr[part];
                return current;
            }
            else
            {
                ReadOnlySpan<char> partStr = branchStr[part];
                current = current.GetBranch(partStr, caseInsensitive);
                if (current == null) return null;
            }
        }

        return current;
    }

    public NTreeString<T> AddGetBranch(ReadOnlySpan<char> branchName, bool caseInsensitive = true)
    {
        NTreeString<T>? existingBranch = GetBranch(branchName);
        if (existingBranch != null)
            return existingBranch;

        string branchNameStr;
        if (caseInsensitive)
        {
            Span<char> buffer = stackalloc char[branchName.Length];
            branchName.ToLowerInvariant(buffer);
            branchNameStr = buffer.ToString();
        }
        else
        {
            branchNameStr = branchName.ToString();
        }

        NTreeString<T> newBranch = new NTreeString<T>(branchNameStr, this);
        Branches.Add(newBranch);
        return newBranch;
    }

    public NTreeString<T> AddGetBranchFromPath(ReadOnlySpan<char> branchStr, ReadOnlySpan<char> sep, bool skipFinal, out ReadOnlySpan<char> last, bool caseInsensitive = true)
    {
        last = ReadOnlySpan<char>.Empty;

        NTreeString<T> current = this;
        MemoryExtensions.SpanSplitEnumerator<char> spanEnum = branchStr.SplitAny(sep);
        foreach (Range part in spanEnum)
        {
            if (skipFinal && part.End.Value == branchStr.Length)
            {
                last = branchStr[part];
                return current;
            }
            else
            {
                ReadOnlySpan<char> partStr = branchStr[part];
                current = current.AddGetBranch(partStr, caseInsensitive);
            }
        }

        return this;
    }
}
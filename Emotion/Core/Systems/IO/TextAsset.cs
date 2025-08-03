#nullable enable

#region Using

using System.Buffers;
using System.Text;

#endregion

namespace Emotion.Core.Systems.IO;

public class TextAsset : Asset
{
    /// <summary>
    /// The context of the text file without any encoding mark and normalized new lines to \n
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
        ReadOnlySpan<byte> span = data.Span;
        Encoding encoding = Helpers.GuessStringEncoding(span, out byte[] byteHeader); // Commonly the string is xml.

        // Remove BOM header
        if (byteHeader.Length > 0)
        {
            span = span.Slice(byteHeader.Length);

            // Sometimes the BOM mark is present multiple times due to
            // shitty text editors adding it over and over again.
            // Make sure we sanitize this completely
            while (span.StartsWith(byteHeader))
            {
                span = span.Slice(byteHeader.Length);
            }
        }

        // Decode the string in the detected encoding using a temporary memory.
        int charCount = encoding.GetCharCount(span);
        char[] stringDecodeMemory = ArrayPool<char>.Shared.Rent(charCount);
        int charCountConverted = encoding.GetChars(span, stringDecodeMemory);
        AssertEqual(charCount, charCountConverted);

        // Remove Windows new lines
        int writeIdx = 0;
        for (int i = 0; i < charCountConverted; i++)
        {
            var c = stringDecodeMemory[i];
            if (c != '\r')
            {
                stringDecodeMemory[writeIdx] = c;
                writeIdx++;
            }
        }
        
        // Allocate a string object and copy our decoded memory to it.
        Content = new string(stringDecodeMemory, 0, writeIdx);
        ArrayPool<char>.Shared.Return(stringDecodeMemory);
    }

    protected override void ReloadInternal(ReadOnlyMemory<byte> data)
    {
        CreateInternal(data);
    }

    protected override void DisposeInternal()
    {
        Content = string.Empty;
    }
}
#nullable enable

#region Using

using Emotion.Standard.Serialization.XML;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Emotion.Core.Systems.IO;

// Non generic marker class to differentiate XMLAsset classes
public abstract class XMLAssetMarkerClass : Asset
{
    static XMLAssetMarkerClass()
    {
        RegisterFileExtensionSupport<XMLAssetMarkerClass>([".xml"]);
    }

    public XMLAssetMarkerClass()
    {
        _useNewLoading = true;
    }

    /// <summary>
    /// Whether the file was successfully parsed,
    /// and an instance of the type was created.
    /// </summary>
    public abstract bool HasContent();

    /// <summary>
    /// Save the file to the asset store.
    /// </summary>
    public abstract bool Save();

    /// <summary>
    /// Save the file to the asset store with the provided name.
    /// </summary>
    public abstract bool SaveAs(string name, bool backup = true);
}

public abstract class XMLAssetBase<T> : XMLAssetMarkerClass
{
    /// <summary>
    /// The file deserialized as its type.
    /// </summary>
    public T? Content { get; protected set; }
}

public abstract class XMLAssetBase<T, TSelf> : XMLAssetBase<T>, IAssetContainingObject<T>
    where TSelf : XMLAssetBase<T, TSelf>, new()
{
    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        CreateInternal(data);
        yield break;
    }

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
        // we need to support the legacy because of RenderComposer loading default shaders.
        try
        {
            ReadOnlySpan<byte> sp = data.Span;
            Encoding encoding = Helpers.GuessStringEncoding(sp);
            if (encoding == System.Text.Encoding.Default)
            {
                ReadOnlySpan<char> utf16 = MemoryMarshal.Cast<byte, char>(data.Span);
                Content = XMLSerialization.From<T>(utf16);
            }
            else if (encoding == System.Text.Encoding.UTF8)
            {
                Content = XMLSerialization.From<T>(sp);
            }
        }
        catch (Exception ex)
        {
            Engine.Log.Error(new Exception($"Couldn't parse XML asset of type {GetType()}!", ex));
        }
    }

    protected override void DisposeInternal()
    {
    }


    public T? GetObject()
    {
        return Content;
    }

    /// <inheritdoc />
    public override bool HasContent()
    {
        return Content != null;
    }

    /// <summary>
    /// Create a new xml file from content and a name.
    /// </summary>
    public static TSelf CreateFromContent(T content, string name = "Untitled")
    {
        return new TSelf()
        {
            Content = content,
            Name = name,
            Processed = true
        };
    }

    /// <summary>
    /// Load a xml file via the asset loader, or create a new one if missing.
    /// </summary>
    public static TSelf LoadOrCreate(string name)
    {
        if (Engine.AssetLoader.Exists(name)) return Engine.AssetLoader.Get<TSelf>(name);

        // If the file doesn't exist - create it.
        TSelf newFile = CreateFromContent(Activator.CreateInstance<T>(), name);
        newFile.Save();
        return newFile;
    }


    /// <inheritdoc />
    public override bool Save()
    {
        return SaveAs(Name);
    }

    /// <inheritdoc />
    public override bool SaveAs(string name, bool backup = true)
    {
        string? data = XMLSerialization.To(Content);
        if (data == null)
        {
            Engine.Log.Error($"Couldn't serialize content of type {typeof(T)}.", MessageSource.Other);
            return false;
        }

        bool saved = Engine.AssetLoader.Save(name, data);
        if (!saved) Engine.Log.Warning($"Couldn't save file {name}.", MessageSource.Other);
        return saved;
    }
}

/// <summary>
/// An asset containg a xml serialized type.
/// </summary>
public sealed class XMLAsset<T> : XMLAssetBase<T, XMLAsset<T>>
{
}
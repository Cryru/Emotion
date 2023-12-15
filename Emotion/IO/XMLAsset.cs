#region Using

using System.Text;
using Emotion.Standard.XML;

#endregion

#nullable enable

namespace Emotion.IO;

// Non generic marker class to differentiate XMLAsset classes
public abstract class XMLAssetMarkerClass : Asset
{
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

/// <summary>
/// A file in XML structure.
/// </summary>
/// <typeparam name="T">The class to deserialize to.</typeparam>
public class XMLAsset<T> : XMLAssetMarkerClass
{
    /// <summary>
    /// The contents of the file.
    /// </summary>
    public T? Content { get; protected set; }

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
        try
        {
            Content = XMLFormat.From<T>(data);
        }
        catch (Exception ex)
        {
            Engine.Log.Error(new Exception($"Couldn't parse XML asset of type {GetType()}!", ex));
        }
    }

    protected override void DisposeInternal()
    {
    }

    /// <inheritdoc />
    public override bool HasContent()
    {
        return Content != null;
    }

    /// <inheritdoc />
    public override bool Save()
    {
        return SaveAs(Name);
    }

    /// <inheritdoc />
    public override bool SaveAs(string name, bool backup = true)
    {
        string? data = XMLFormat.To(Content);
        if (data == null)
        {
            Engine.Log.Error($"Couldn't serialize content of type {typeof(T)}.", MessageSource.Other);
            return false;
        }

        bool saved = Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(data), name, backup);
        if (!saved) Engine.Log.Warning($"Couldn't save file {name}.", MessageSource.Other);
        return saved;
    }

    /// <summary>
    /// Create a new xml file from content and a name.
    /// </summary>
    public static XMLAsset<T> CreateFromContent(T content, string name = "Untitled")
    {
        return new()
        {
            Content = content,
            Name = name
        };
    }

    /// <summary>
    /// Load a xml file via the asset loader, or create a new one if missing.
    /// </summary>
    public static XMLAsset<T> LoadSaveOrCreate(string name)
    {
        if (Engine.AssetLoader.Exists(name)) return Engine.AssetLoader.Get<XMLAsset<T>>(name)!;

        // If the file doesn't exist - create it.
        XMLAsset<T> newFile = CreateFromContent(Activator.CreateInstance<T>(), name);
        newFile.Save();
        return newFile;
    }
}
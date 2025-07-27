#nullable enable

using Emotion.Serialization.XML;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.Utility.OptimizedStringReadWrite;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System.Text.Json;

namespace Emotion.Standard.Reflector.Handlers.Base;

public abstract class ReflectorTypeHandlerBase<T> : IGenericReflectorTypeHandler
{
    public abstract string TypeName { get; }

    public abstract Type Type { get; }

    public virtual TypeEditor? GetEditor()
    {
        return null;
    }

    public virtual void PostInit()
    {
        // nop
    }

    public virtual bool CanCreateNew()
    {
        return true;
    }

    public virtual object? CreateNew()
    {
        return default(T);
    }

    #region Serialization Read

    public ResponseT? ParseFromJSON<ResponseT>(ref Utf8JsonReader reader)
    {
        T? resp = ParseFromJSON(ref reader);
        if (resp is ResponseT respT)
            return respT;

        return default;
    }

    public virtual T? ParseFromJSON(ref Utf8JsonReader reader)
    {
        return default;
    }

    public ResponseT? ParseFromXML<ResponseT>(ref ValueStringReader reader)
    {
        T? resp = ParseFromXML(ref reader);
        if (resp is ResponseT respT)
            return respT;

        return default;
    }

    public virtual T? ParseFromXML(ref ValueStringReader reader)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Serialization Write

    public void WriteAsCode<OwnerT>(OwnerT? value, ref ValueStringWriter writer)
    {
        if (value == null)
        {
            writer.WriteString("null");
            return;
        }

        if (value is T valueAsT)
            WriteAsCode(valueAsT, ref writer);
    }

    public virtual void WriteAsCode(T value, ref ValueStringWriter writer)
    {

    }

    public void WriteAsXML<OwnerT>(OwnerT? value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0)
    {
        if (value == null)
        {
            //writer.WriteString("null");
            return;
        }

        if (value is T valueAsT)
            WriteAsXML(valueAsT, ref writer, addTypeTags, config, indent);
    }

    public virtual void WriteAsXML(T value, ref ValueStringWriter writer, bool addTypeTags, XMLConfig config, int indent = 0)
    {

    }

    #endregion
}
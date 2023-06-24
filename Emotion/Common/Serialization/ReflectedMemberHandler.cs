#region Using

using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

#nullable enable

namespace Emotion.Common.Serialization;

/// <summary>
/// Handles the interop between the serializers and C# reflection.
/// </summary>
public class ReflectedMemberHandler
{
	/// <summary>
	/// The name of the property this object handles.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The type this member was declared in.
	/// </summary>
	public Type? DeclaredIn
	{
		get => _prop?.DeclaringType ?? _field?.DeclaringType ?? null;
	}

	private readonly PropertyInfo? _prop;
	private readonly FieldInfo? _field;

	/// <summary>
	/// Create a new reflection handler for the specified property.
	/// </summary>
	/// <param name="prop"></param>
	public ReflectedMemberHandler(PropertyInfo prop)
	{
		_prop = prop;
		Name = _prop.Name;
	}

	/// <summary>
	/// Create a new reflection handler for the specified field.
	/// </summary>
	/// <param name="field"></param>
	public ReflectedMemberHandler(FieldInfo field)
	{
		_field = field;
		Name = _field.Name;
	}

	/// <summary>
	/// Get the value of the field or property this handler manages from the object instance.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public object? GetValue(object obj)
	{
		return _prop?.GetValue(obj) ?? _field?.GetValue(obj);
	}

	/// <summary>
	/// Set the value of the field or property this handler manages from the object instance.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetValue(object obj, object val)
	{
		_prop?.SetValue(obj, val);
		_field?.SetValue(obj, val);
	}

	/// <summary>
	/// Get an attribute from the reflected member, if any.
	/// </summary>
	public T? GetAttribute<T>() where T : Attribute
	{
		return _prop?.GetCustomAttribute<T>() ?? _field?.GetCustomAttribute<T>();
	}
}
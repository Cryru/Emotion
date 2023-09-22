#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;

#endregion

namespace Emotion.Game.ThreeDee;

/// <summary>
/// Holds state information referring to a mesh entity.
/// </summary>
[DontSerialize]
public class MeshEntityMetaState
{
	/// <summary>
	/// Whether the mesh index should be rendered.
	/// </summary>
	public bool[] RenderMesh = Array.Empty<bool>();

	/// <summary>
	/// A color to tint all vertices of the entity in.
	/// Is multiplied by the material color.
	/// </summary>
	public Color Tint = Color.White;

	/// <summary>
	/// Whether this object should ignore the light model and
	/// be rendered as its diffuse color.
	/// </summary>
	public bool IgnoreLightModel = false;

	public class MeshMaterialShaderParameter
	{
		public string ParamName;
		public object Value;

		public MeshMaterialShaderParameter(string paramName, object val)
		{
			ParamName = paramName;
			Value = val;
		}
	}

	public string? ShaderName;
	public ShaderAsset? ShaderAsset;
	private Dictionary<string, object>? _shaderParameters;
	private Dictionary<string, Action<ShaderProgram, string, object>>? _shaderParameterApplyFuncs;

	public MeshEntityMetaState(MeshEntity? entity)
	{
		if (entity?.Meshes == null) return;

		RenderMesh = new bool[entity.Meshes.Length];
		for (var i = 0; i < RenderMesh.Length; i++)
		{
			RenderMesh[i] = true;
		}
	}

	public async Task SetShader(string path)
	{
		_shaderParameters = new();
		_shaderParameterApplyFuncs = new();
		ShaderAsset = await Engine.AssetLoader.GetAsync<ShaderAsset>(path);
	}

	public void SetShaderParam(string name, object value)
	{
		if (_shaderParameters == null || _shaderParameterApplyFuncs == null) return;
		_shaderParameters[name] = value;

		// Parameters can't change types.
		if (_shaderParameterApplyFuncs.ContainsKey(name)) return;
		_shaderParameterApplyFuncs[name] = value switch
		{
			float => ApplyUniformFloat,
			Vector2 => ApplyUniformVec2,
			Vector3 => ApplyUniformVec3,
			Vector4 => ApplyUniformVec4,
			Color => ApplyUniformColor,
			_ => _shaderParameterApplyFuncs[name]
		};
	}

	public void ApplyShaderUniforms(ShaderProgram program)
	{
		if (_shaderParameters == null || _shaderParameterApplyFuncs == null) return;
		foreach ((string? paramName, object? val) in _shaderParameters)
		{
			_shaderParameterApplyFuncs[paramName](program, paramName, val);
		}
	}

	// todo: do this better

	private void ApplyUniformFloat(ShaderProgram program, string name, object val)
	{
		program.SetUniformFloat(name, (float) val);
	}

	private void ApplyUniformVec2(ShaderProgram program, string name, object val)
	{
		program.SetUniformVector2(name, (Vector2) val);
	}

	private void ApplyUniformVec3(ShaderProgram program, string name, object val)
	{
		program.SetUniformVector3(name, (Vector3) val);
	}

	private void ApplyUniformVec4(ShaderProgram program, string name, object val)
	{
		program.SetUniformVector4(name, (Vector4) val);
	}

	private void ApplyUniformColor(ShaderProgram program, string name, object val)
	{
		program.SetUniformColor(name, (Color) val);
	}
}
#nullable enable

using Emotion.Core.Systems.IO;
using OpenGL;
using System.Text;

namespace Emotion.Graphics.Shader;

public class ShaderGroupAsset : TextAsset, IAssetContainingObject<ShaderGroup>
{
    public ShaderGroup? ShaderGroup = null;

    public ShaderGroupAsset()
    {
        _useNewLoading = true;
    }

    public ShaderGroup? GetObject()
    {
        return ShaderGroup;
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        base.CreateInternal(data);

        // Process asset dependencies (shader includes)
        bool es = Gl.CurrentShadingVersion.GLES;

        StringBuilder include = new StringBuilder();
        int includeCount = 1;

        if (es)
        {
            TextAsset includeFile = Engine.AssetLoader.Get<TextAsset>("Shaders/GLESSupport.c");
            yield return includeFile;

            include.Append($"\n#line 0 {includeCount}\n");
            include.Append(includeFile.Content);
            includeCount++;
        }

        foreach (string includePath in ForEachInclude(base.Content))
        {
            TextAsset includeFile = Engine.AssetLoader.Get<TextAsset>(includePath, this, false, true);
            yield return includeFile;

            include.Append($"\n#line 0 {includeCount}\n");
            include.Append(includeFile.Content);
            includeCount++;
        }

        string versionString = $"#version {Gl.CurrentShadingVersion.VersionId}{(es ? " es" : "")}\n";
        ShaderGroup?.Dispose(); // Hot reloading
        ShaderGroup = new ShaderGroup(Name, versionString, base.Content, include);
        ShaderGroup.Init();
    }

    private static IEnumerable<string> ForEachInclude(string source)
    {
        const string includeToken = "INCLUDE_FILE";
        int tokenLength = includeToken.Length;

        int offset = 0;
        while (offset < source.Length)
        {
            int idx = source.IndexOf(includeToken, offset);
            if (idx < 0) yield break; // No more

            idx += tokenLength;

            // Skip whitespaces
            while (idx < source.Length && char.IsWhiteSpace(source[idx]))
                idx++;
            if (idx == source.Length) break;

            // Check if opening
            if (source[idx] == '<')
            {
                // Read to closing
                int end = source.IndexOf('>', idx + 1);
                if (end != -1)
                {
                    yield return source.Substring(idx + 1, end - idx - 1);
                }

                offset = end + 1;
            }
        }
    }
}
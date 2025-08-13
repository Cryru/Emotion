#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Shading;
using OpenGL;
using System.Text;

namespace Emotion.Graphics.Shader;

public class NewShaderAsset : TextAsset, IAssetContainingObject<ShaderProgram>
{
    public ShaderProgram? CompiledShader;

    public ShaderProgram? GetObject()
    {
        return CompiledShader;
    }

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
        // Old shader asset compatibility
        if (Name.Contains(".xml"))
        {
            ShaderProgram? oldLegacyShader = CompiledShader;
            var oldAsset = new ShaderAsset();
            oldAsset.AssetLoader_CreateLegacy(data);
            CompiledShader = oldAsset.Shader;
            oldLegacyShader?.Dispose();

            return;
        }

        base.CreateInternal(data);

        // Content <- The content of our file :)
        bool es = Gl.CurrentShadingVersion.GLES;
        string versionString = $"#version {Gl.CurrentShadingVersion.VersionId}{(es ? " es" : "")}\n";

        StringBuilder vertexShader = new StringBuilder();
        vertexShader.Append(versionString);
        vertexShader.Append("\n#define VERT_SHADER 1\n");
        vertexShader.Append(Content);
        vertexShader.Append("\n");
        vertexShader.Append("void main()\n{\nVertexShaderMain();\n}");

        StringBuilder fragmentShader = new StringBuilder();
        fragmentShader.Append(versionString);
        fragmentShader.Append("\n#define FRAG_SHADER 1\n");
        fragmentShader.Append(Content);
        fragmentShader.Append("\n");
        fragmentShader.Append("\nout vec4 fragColor;\n");
        fragmentShader.Append("void main()\n{\nfragColor = FragmentShaderMain();\n}");

        ShaderProgram? newShader = GLThread.ExecuteGLThread(ShaderFactory.CreateShaderRaw, vertexShader.ToString(), fragmentShader.ToString());
        ShaderProgram? oldShader = CompiledShader;
        CompiledShader = newShader;
        oldShader?.Dispose(); // When hot reloading we need to destroy the previous shader
    }
}

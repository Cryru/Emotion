using Emotion.Common.Threading;
using Emotion.Graphics.Shading;
using Emotion.IO;
using OpenGL;
using System.Text;

namespace Emotion.Graphics.Shader;

public class NewShaderAsset : TextAsset
{
    public ShaderProgram CompiledShader;

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
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

        CompiledShader?.Dispose();
        CompiledShader = GLThread.ExecuteGLThread(ShaderFactory.CreateShaderRaw, vertexShader.ToString(), fragmentShader.ToString());
    }
}

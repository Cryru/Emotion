﻿#region Using

using Emotion.Common.Input;
using Emotion.Common.Threading;
using Emotion.Graphics.Shading;

#endregion

namespace Emotion.IO
{
    public class ShaderDescription
    {
        /// <summary>
        /// The path to the fragment shader relative to the shader description asset.
        /// </summary>
        public string Frag { get; set; }

        /// <summary>
        /// The path to the fragment shader relative to the shader description asset.
        /// </summary>
        public string Vert { get; set; }

        /// <summary>
        /// The path to another shader description to fallback to if the creation of this shader fails.
        /// </summary>
        public string Fallback { get; set; } = "Shaders/DefaultShader.xml";
    }

    public class ShaderAsset : XMLAsset<ShaderDescription>
    {
        /// <summary>
        /// The description of the shader. Legacy property, redirect to Content.
        /// </summary>
        public ShaderDescription Description
        {
            get => Content;
        }

        /// <summary>
        /// Whether using the fallback shader.
        /// </summary>
        public bool IsFallback { get; set; }

        /// <summary>
        /// The name of the fallback shader used - if any.
        /// </summary>
        public string FallbackName { get; set; }

        /// <summary>
        /// The compilation constant used, if this asset is a shader variation.
        /// </summary>
        public string CompilationConstant { get; set; }

        /// <summary>
        /// The actual shader compiled from this asset.
        /// </summary>
        public ShaderProgram Shader { get; protected set; }

        protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
        {
            yield return base.Internal_LoadAssetRoutine(data);

            // Fallback to default.
            Content ??= new ShaderDescription();
            Compile();
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            // Deserialize the shader description.
            base.CreateInternal(data);
            // Fallback to default.
            Content ??= new ShaderDescription();
            Compile();
        }

        private Dictionary<string, ShaderAsset> _variations = new();

        /// <summary>
        /// Create a shader variation by compiling it with a specified compile constant
        /// constant. The asset created by this function isn't managed by the AssetLoader.
        /// </summary>
        /// <returns></returns>
        public ShaderAsset GetShaderVariation(string compileConstant)
        {
            lock (_variations)
            {
                if (_variations.TryGetValue(compileConstant, out ShaderAsset shaderAsset)) return shaderAsset;

                var newAsset = new ShaderAsset
                {
                    Content = Content,
                    Name = $"{Name} #{compileConstant}"
                };
                newAsset.Compile(compileConstant);
                newAsset.CompilationConstant = compileConstant;
                _variations.Add(compileConstant, newAsset);

                return newAsset;
            }
        }

        private void Compile(string compileConstant = null)
        {
            // Get the text contents of the shader files referenced. If any of them are missing substitute with the default one.
            string assetDirectory = AssetLoader.GetDirectoryName(Name);

            TextAsset vertShader = null;
            if (!string.IsNullOrEmpty(Content.Vert))
            {
                string path = AssetLoader.GetNonRelativePath(assetDirectory, Content.Vert, false);
                vertShader = Engine.AssetLoader.Get<TextAsset>(path, false);
                if (vertShader == null) Engine.Log.Warning($"Couldn't find shader file {path}. Using default.", MessageSource.AssetLoader);
            }

            vertShader ??= ShaderFactory.DefaultProgram_Vert;

            TextAsset fragShader = null;
            if (!string.IsNullOrEmpty(Content.Frag))
            {
                string path = AssetLoader.GetNonRelativePath(assetDirectory, Content.Frag, false);
                fragShader = Engine.AssetLoader.Get<TextAsset>(path, false);
                if (fragShader == null) Engine.Log.Warning($"Couldn't find shader file {path}. Using default.", MessageSource.AssetLoader);
            }

            fragShader ??= ShaderFactory.DefaultProgram_Frag;

            var shaderLogName = $"v:{vertShader!.Name}, f:{fragShader!.Name} {compileConstant}";
            Engine.Log.Info($"Loading shader {shaderLogName}...", MessageSource.AssetLoader);

            // Create the shader, or at least try to.
            PerfProfiler.ProfilerEventStart("Compilation", "Loading");
            ShaderProgram compiledProgram = GLThread.ExecuteGLThread(ShaderFactory.CreateShader, vertShader.Content, fragShader.Content, compileConstant);
            PerfProfiler.ProfilerEventEnd("Compilation", "Loading");

            // Reloading shader. Keep reference of current object, substitute OpenGL pointer only.
            if (Shader != null)
            {
                if (compiledProgram == null) return;
                if (!compiledProgram.Valid) return;

                DisposeInternal();
                Shader.CopyFrom(compiledProgram);
                Shader.DebugName = Name;
            }
            else
            {
                Shader = compiledProgram;
            }

            // Check if compilation was successful.
            if (Shader != null && Shader.Valid)
            {
                IsFallback = false;
                FallbackName = null;
                Engine.Log.Info($"Compiled {shaderLogName}!", MessageSource.AssetLoader);
                return;
            }

            Engine.Log.Warning($"Shader {Name} compilation failed. Falling back.", MessageSource.AssetLoader);
            IsFallback = true;

            // If there is no fallback, fallback to default.
            if (string.IsNullOrEmpty(Content.Fallback))
            {
                Engine.Log.Warning("No fallback specified, falling back to default.", MessageSource.AssetLoader);
                FallbackName = "Default";
                Shader = ShaderProgram.CreateCopied(ShaderFactory.DefaultProgram);
                return;
            }

            var fallBackShader = Engine.AssetLoader.Get<ShaderAsset>(Content.Fallback);
            // If fallback not found, fallback to default.
            if (fallBackShader == null)
            {
                Engine.Log.Warning($"Fallback {Content.Fallback} not found. Falling back to default.", MessageSource.AssetLoader);
                FallbackName = "Default";
                Shader = ShaderProgram.CreateCopied(ShaderFactory.DefaultProgram);
                return;
            }

            Engine.Log.Warning($"Shader {Name} fell back to {Content.Fallback}.", MessageSource.AssetLoader);
            FallbackName = Content.Fallback;
            Shader = ShaderProgram.CreateCopied(fallBackShader.Shader);
        }

        protected override void DisposeInternal()
        {
            if (!IsFallback) Shader.Dispose();
        }

        public static ShaderProgram DefaultProgram { get => ShaderFactory.DefaultProgram; }
    }
}
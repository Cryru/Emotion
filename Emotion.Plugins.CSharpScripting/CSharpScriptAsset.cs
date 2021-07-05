#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Emotion.IO;

#endregion

namespace Emotion.Plugins.CSharpScripting
{
    public class CSharpScriptAsset : Asset
    {
        public ScriptAssemblyContext Context { get; protected set; }
        public Assembly Assembly { get; protected set; }

        private static Dictionary<string, Assembly> _compiledAssemblies = new();

        public CSharpScriptAsset()
        {
            // Constructor for AssetLoader
        }

        public CSharpScriptAsset(string source)
        {
            CreateFromString(source);
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            string source = Encoding.UTF8.GetString(data.Span).Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
            CreateFromString(source);
        }

        private void CreateFromString(string source)
        {
            Context = new ScriptAssemblyContext();
            MemoryStream stream = CSharpScriptEngine.CompileCode(source);
            if (stream != null)
            {
                Assembly = Context.LoadFromStream(stream);
                _compiledAssemblies.Add(Name, Assembly);
            }
        }

        protected override void DisposeInternal()
        {
            Assembly = null;
            Context = null;
        }
    }
}
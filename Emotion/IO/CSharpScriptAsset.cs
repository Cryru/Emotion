#region Using

using System.IO;
using System.Reflection;
using System.Text;
using Emotion.Scripting;

#endregion

namespace Emotion.IO
{
    public class CSharpScriptAsset : Asset
    {
        public ScriptAssemblyContext Context { get; protected set; }
        public Assembly Assembly { get; protected set; }

        public CSharpScriptAsset()
        {
            // Constructor for AssetLoader
        }

        public CSharpScriptAsset(string source)
        {
            CreateFromString(source);
        }

        protected override void CreateInternal(byte[] data)
        {
            string source = Encoding.UTF8.GetString(data).Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
            CreateFromString(source);
        }

        private void CreateFromString(string source)
        {
            Context = new ScriptAssemblyContext();
            MemoryStream stream = CSharpScriptEngine.CompileCode(source);
            if (stream != null)
                Assembly = Context.LoadFromStream(stream);
        }

        protected override void DisposeInternal()
        {
            Context.Unload();
            Assembly = null;
            Context = null;
        }
    }
}
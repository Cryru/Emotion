#region Using

using System.Text;
using System.Threading.Tasks;
using Emotion.Core.Systems.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

#endregion

#nullable enable

namespace Emotion.Core.Systems.Scripting
{
    public sealed class CSharpScriptAsset : Asset
    {
        public class ScriptingArgs
        {
            public object? Param;
        }

        private static ScriptOptions? _scriptingEnvironment;
        private string _sourceCode;

        public CSharpScriptAsset()
        {
            // Constructor for AssetLoader
            _sourceCode = null!;
        }

        public CSharpScriptAsset(string source)
        {
            _sourceCode = source;
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            _sourceCode = Encoding.UTF8.GetString(data.Span).Replace("\r", "").Replace("\uFEFF", "").Replace("ï»¿", "");
        }

        public Task<object> Execute(object? arg = null)
        {
            if (_scriptingEnvironment == null)
            {
                var options = ScriptOptions.Default;
                options = options.AddReferences(Helpers.AssociatedAssemblies);
                for (var i = 0; i < Helpers.AssociatedAssemblies.Length; i++)
                {
                    // ONE
                    //Assembly? assembly = Helpers.AssociatedAssemblies[i];
                    //Type[] allTypes = assembly.GetTypes();
                    //for (var j = 0; j < allTypes.Length; j++)
                    //{
                    //    var type = allTypes[j];
                    //    if (type.Namespace == null) continue;
                    //    options = options.AddImports(type.Namespace);
                    //}
                }

                _scriptingEnvironment = options;
            }

            var args = new ScriptingArgs
            {
                Param = arg
            };

            return CSharpScript.EvaluateAsync(_sourceCode, _scriptingEnvironment, args);
        }

        protected override void DisposeInternal()
        {
            _sourceCode = null!;
        }
    }
}
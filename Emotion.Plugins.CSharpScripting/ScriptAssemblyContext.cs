#region Using

using System.Reflection;
using System.Runtime.Loader;

#endregion

namespace Emotion.Plugins.CSharpScripting
{
    public class ScriptAssemblyContext : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soul.Engine.Enums
{
    /// <summary>
    /// Sources of debugging messages.
    /// </summary>
    public enum DebugMessageSource
    {
        Debug,
        Boot,
        Error,
        SceneManager,
        ScriptModule,
        PhysicsModule,
        Execution
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Soul.Engine.Enums
{
    public enum ErrorOrigin
    {
        Unknown = 0,
        SoulLib = 1,
        Scripting = 2,
        Breath = 3,
        SceneManager = 4,
        SceneLogic = 5
    }
}

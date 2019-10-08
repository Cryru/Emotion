using System;
using System.Collections.Generic;
using System.Text;
using Emotion;
using Emotion.Common;
using Emotion.Test.Tests;

namespace ScriptTest
{
    public static class Test
    {
        public static void Main()
        {
            // Is called if no ScriptMain function exists.
        }

        public static int ScriptMain(object obj)
        {
            return obj.GetHashCode();
        }
    }
}

﻿#region Using

using Emotion.Common;
using Emotion.Game.Scripting;
using Emotion.Test;

#endregion

namespace Tests.Classes
{
    public static class ScriptingTestApi
    {
        public static int CallCount;

        public static void Call()
        {
            CallCount++;
        }

        public static void CallDecrement()
        {
            CallCount--;
        }
    }

    [Test("Scripting", true)]
    public class ScriptingTest
    {
        [Test]
        public void TestBasicScript()
        {
            var script = Engine.AssetLoader.Get<CSharpScriptAsset>("Scripts/MainScript.cs");
            int callCount = ScriptingTestApi.CallCount;
            script.Execute().Wait();
            Assert.True(ScriptingTestApi.CallCount == callCount + 1);
            Engine.AssetLoader.Destroy("Scripts/MainScript.cs");

            script = Engine.AssetLoader.Get<CSharpScriptAsset>("Scripts/CustomMainScript.cs");
            script.Execute().Wait();
            Assert.True(ScriptingTestApi.CallCount == callCount);
            Engine.AssetLoader.Destroy("Scripts/CustomMainScript.cs");
        }

        [Test]
        public void TestScriptArgs()
        {
            var script = Engine.AssetLoader.Get<CSharpScriptAsset>("Scripts/CustomMainArgsScript.cs");
            const string objectPass = "yo";
            object result = script.Execute(objectPass).Result;
            Assert.True((int) result == objectPass.GetHashCode());
            Engine.AssetLoader.Destroy("Scripts/CustomMainArgsScript.cs");
        }
    }
}
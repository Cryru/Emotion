#region Using

using System;
using Adfectus.Common;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected with the scripting engine.
    /// </summary>
    [Collection("main")]
    public class Scripting
    {
        private string _lastPrint = "";

        /// <summary>
        /// Test whether execution of scripts, and exposing functions works.
        /// </summary>
        [Fact]
        public void TestExecAndExposedFunc()
        {
            string testScript = @"function printTest(str) {
    testPrint(str);
}

for(let i = 0; i < 10; i++) {
  printTest(i);
}";

            Engine.ScriptingEngine.Expose("testPrint", (Action<string>) ScriptingPrint);
            Engine.ScriptingEngine.RunScript(testScript);
            Assert.Equal("9", _lastPrint);

            _lastPrint = "";

            Engine.ScriptingEngine.RunScriptAsync(testScript).Wait();
            Assert.Equal("9", _lastPrint);

            _lastPrint = "";
        }

        private void ScriptingPrint(string msg)
        {
            _lastPrint = msg;
        }
    }
}
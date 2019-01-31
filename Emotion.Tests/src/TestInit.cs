// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Drawing;
using System.Numerics;
using Emotion.Debug.Logging;
using Emotion.Engine;
using Emotion.Tests.Interoperability;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests
{
    [TestClass]
    public class TestInit
    {
        /// <summary>
        /// The logger to use.
        /// </summary>
        public static LoggingProvider Logger = new DebugLogger();

        /// <summary>
        /// The Emotion host used for testing.
        /// </summary>
        public static TestHost TestingHost = new TestHost();

        /// <summary>
        /// Test starting function which doesn't require the testing framework to be loaded.
        /// </summary>
        public static void StartTestForeign()
        {
            StartTest(null);
        }

        [AssemblyInitialize]
        public static void StartTest(TestContext _)
        {
            Context.Flags.CloseEnvironmentOnQuit = false;
            Context.Flags.AdditionalAssetAssemblies = new[] {typeof(TestInit).Assembly};
            Context.Host = TestingHost;
            Context.Log = Logger;
            Context.Setup();
            Context.Run();
        }

        [ClassCleanup]
        public static void TestEnd()
        {
            Context.Quit();
        }

        /// <summary>
        /// Test whether the testing harness works.
        /// </summary>
        [TestMethod]
        public void HarnessTest()
        {
            TestingHost.RunCycle();
            Bitmap bitmap = TestingHost.TakeScreenshot();
            Assert.AreEqual(new Vector2(bitmap.Size.Width, bitmap.Size.Height), TestingHost.Size);
        }
    }
}
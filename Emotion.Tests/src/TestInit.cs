// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;
using System.Numerics;
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
        /// The Emotion host used for testing.
        /// </summary>
        public static TestHost TestingHost = new TestHost();

        [AssemblyInitialize]
        public static void StartTest(TestContext _)
        {
            Context.Flags.CloseEnvironmentOnQuit = false;
            Context.Flags.AdditionalAssetAssemblies = new[] {typeof(TestInit).Assembly};
            Context.Host = TestingHost;
            Context.Log = new DebugLogger();
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
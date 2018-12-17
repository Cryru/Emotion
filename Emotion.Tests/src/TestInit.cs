// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;
using Emotion.Engine;
using Emotion.Tests.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests
{
    [TestClass]
    public class TestInit
    {
        private static TestHost _testingHost = new TestHost();

        [ClassInitialize]
        public static void StartTest(TestContext _)
        {
            Context.Host = _testingHost;
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
        public void TestHarnessTest()
        {
            _testingHost.RunCycle();
            Bitmap bitmap = _testingHost.TakeScreenshot();
            Assert.AreEqual(new System.Numerics.Vector2(bitmap.Size.Width, bitmap.Size.Height), _testingHost.Size);
        }
    }
}
#region Using

using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.IO;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    [CollectionDefinition("main")]
    public class TestHarness : ICollectionFixture<HarnessActual>
    {
    }

    public class HarnessActual : IDisposable
    {
        /// <summary>
        /// Whether the run the harness in another thread.
        /// </summary>
        public static bool RunOnAnotherThread = true;

        /// <summary>
        /// A mutex to be set when the Engine starts running.
        /// </summary>
        public static ManualResetEvent RunEvent = new ManualResetEvent(false);

        public HarnessActual()
        {
            /*
             * On MacOS the window must be created and updated on the main thread. This prevents the engine from not
             * being blocking. Therefore it must be blocking on all other platforms. The tests however are not
             * intended to be ran on MacOS and the Engine can be initialized on another thread.
             *
             * When the tests are ran through Emotion.ExecTest however they are intended to support MacOS,
             * so the engine is started on the main thread and the tests are ran on another thread - essentially the
             * reverse of this.
             */

            if (RunOnAnotherThread)
                // ReSharper disable once RedundantCast
                Task.Run((Action) StartEngine);
            else
                StartEngine();

            RunEvent.WaitOne();
        }

        private void StartEngine()
        {
            Engine.Flags.PauseOnFocusLoss = false;
            EngineBuilder builder = new EngineBuilder().SetupAssets(additionalAssetSources: new AssetSource[] {new EmbeddedAssetSource(typeof(HarnessActual).Assembly, "Assets")})
                .SetupFlags(new Vector2(960, 540), false, targetTPS: 0);
            Engine.Setup(builder);
            Directory.CreateDirectory("ReferenceImages");
            RunEvent.Set();
            Engine.Run();
        }

        public void Dispose()
        {
            Engine.Quit();
        }
    }
}
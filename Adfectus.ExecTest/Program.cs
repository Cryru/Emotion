#region Using

using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.ImGuiNet;
using Adfectus.Logging;
using Adfectus.Platform.DesktopGL;
using Adfectus.Primitives;
using Adfectus.Scenography;
using Adfectus.Tests;
using ImGuiNET;
using Xunit;
using Helpers = Adfectus.Utility.Helpers;

#endregion

namespace Adfectus.ExecTest
{
    public class TestScene : Scene
    {
        private static int _iterations = 1;

        private static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "test")
            {
                if (args.Length > 1) int.TryParse(args[1], out _iterations);

                // Attach network logger.
                //NetworkLogger networkLogger = new NetworkLogger();
                //networkLogger.Connect("logs7.papertrailapp.com", 29219, Environment.UserName);
                //Engine.Setup(null, networkLogger);
                TestBench();
                return;
            }

            Engine.Flags.RenderFlags.IntegerScale = true;
            Engine.Setup<DesktopPlatform>(new EngineBuilder()
                .SetupFlags(debugMode: true)
                .AddGenericPlugin(new ImGuiNetPlugin())
            );
            Engine.SceneManager.SetScene(new TestScene());
            Engine.Run();
        }

        /// <summary>
        /// Run the tests.
        /// </summary>
        private static void TestBench()
        {
            // Set on this thread.
            HarnessActual.RunOnAnotherThread = false;
            HarnessActual harnessInit = null;

            Task.Run(() =>
            {
                // Wait for the harness to start running the engine.
                HarnessActual.RunEvent.WaitOne();

                // Run the tests.
                TestBenchRun();

                // Stop the engine.
                Engine.Quit();
            });

            harnessInit = new HarnessActual();
        }

        private static void TestBenchRun()
        {
            // Find all test classes by the Collection attribute.
            Type[] testClasses = typeof(HarnessActual).Assembly.GetTypes().AsParallel().Where(x => x.GetCustomAttributes(typeof(CollectionAttribute), true).Length > 0).ToArray();

            for (int i = 0; i < _iterations; i++)
            {
                Engine.Log.Info($"Starting test bench iterations {i + 1}...", MessageSource.Other);

                foreach (Type classType in testClasses)
                {
                    Engine.Log.Info($"Running test class {classType}...", MessageSource.Other);

                    // Create an instance of the test class.
                    object testClassInstance = Activator.CreateInstance(classType);

                    // Find all test functions in this class.
                    MethodInfo[] functions = classType.GetMethods().AsParallel().Where(x => x.GetCustomAttributes(typeof(FactAttribute), true).Length > 0).ToArray();

                    foreach (MethodInfo func in functions)
                    {
                        Engine.Log.Info($"Running test {func.Name}...", MessageSource.Other);
                        try
                        {
                            func.Invoke(testClassInstance, new object[] { });
                        }
                        catch (Exception ex)
                        {
                            if (Debugger.IsAttached) break;

                            Engine.Log.Error($"Test {func.Name} failed - {ex}", MessageSource.Other);
                            continue;
                        }

                        Engine.Log.Info($"Test {func.Name} complete!", MessageSource.Other);
                    }

                    Engine.Log.Info($"Test class {classType} complete!", MessageSource.Other);
                }

                Engine.Log.Info($"Test bench complete for iteration {i + 1}!", MessageSource.Other);
            }
        }

        public override void Load()
        {
        }

        private float _hostWidth = 640;
        private float _hostHeight = 360;

        public override void Update()
        {
        }

        public override void Draw()
        {
            Engine.Renderer.Render(new Vector3(0, 0, 0), Engine.Renderer.BaseTarget.Size, Color.CornflowerBlue);

            ImGui.NewFrame();

            ImGui.Begin("Resolution Test");
            ImGui.InputFloat("Width", ref _hostWidth);
            ImGui.InputFloat("Height", ref _hostHeight);
            ImGui.Text($"X: {_hostWidth / Engine.Renderer.BaseTarget.Size.X} / Y: {_hostHeight / Engine.Renderer.BaseTarget.Size.Y}");
            ImGui.Text($"Aspect: {Helpers.GetAspectRatio(_hostWidth, _hostHeight)}");
            if (ImGui.Button("Apply")) Engine.Host.Size = new Vector2(_hostWidth, _hostHeight);

            ImGui.End();

            Engine.Renderer.RenderGui();
        }

        public override void Unload()
        {
        }
    }
}
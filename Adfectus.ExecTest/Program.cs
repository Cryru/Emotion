#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.Game;
using Adfectus.Game.Time;
using Adfectus.Game.Time.Routines;
using Adfectus.Graphics;
using Adfectus.Graphics.Text;
//using Adfectus.ImGuiNet;
using Adfectus.Input;
using Adfectus.Logging;
using Adfectus.Primitives;
using Adfectus.Scenography;
//using Adfectus.Steam;
using Adfectus.Tests;
using Xunit;

#endregion

namespace Adfectus.ExecTest
{
    public class TestScene : Scene
    {
        private List<Vector2> clickLocation = new List<Vector2>();
        private static int _iterations = 1;
        //private static SteamPlugin _pl;

        private static void Main(string[] args)
        {
            //MonitorSimulator.main();
            //args = new string[2] {"test", "1"};
            //HarnessActual harnessInit = new HarnessActual();
            //Adfectus.Tests.Shaders a = new Shaders();
            //a.ShaderBrokenLoad();
            //return;
            //harnessInit.Dispose();
            //return;

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

            //new EngineBuilder().SetupFlags(new Vector2(384, 216))
            //Engine.Settings.RenderSettings.TargetTPS = 0;
            //_pl = new SteamPlugin();
            //new EngineBuilder().AddGenericPlugin(_pl
            ////ImGuiPlugin = new ImGuiNetPlugin();
           // ImGuiPlugin.LoadFont("calibri.ttf", 15, 15);
            Engine.Setup();//new EngineBuilder().AddGenericPlugin(ImGuiPlugin));
            Engine.SceneManager.SetScene(new TestScene());
            
            Vector2 first = new Vector2(10, 0);
            Vector2 second = new Vector2(100, 0);

            Adfectus.Game.Time.Timer a = new Adfectus.Game.Time.Timer();
            a.Tween(1000, first, second, TweenType.In, TweenMethod.Cubic);
            a.AdvanceTime(100);

            Engine.Run();

            bool aaaaa = true;
        }

       // private static ImGuiNetPlugin ImGuiPlugin;

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

        private Adfectus.Game.Text.RichText test;
        private CoroutineManager _coroutineManager;

        public override void Load()
        {
            test = new Game.Text.RichText(new Vector3(10, 10, 0), new Vector2(200, 200), Engine.AssetLoader.Get<Font>("debugFont.otf").GetFontAtlas(20));
            test.SetText("sdadasdas");
            _coroutineManager = new CoroutineManager();
        }

        private float loc = 0;

        public override void Update()
        {
            if (Engine.InputManager.IsMouseKeyDown(MouseKey.Left))
            {
                //Engine.SoundManager.Play(Engine.AssetLoader.Get<SoundFile>("Sounds/noice.wav"), "test");

                Vector2 pos = Engine.InputManager.GetMousePosition();

                if (pos.X < 0) pos.X = 0;
                if (pos.Y < 0) pos.Y = 0;

                clickLocation.Add(pos);
            }

            if (Engine.InputManager.IsKeyHeld("W")) Engine.Renderer.Camera.Y -= 5;

            if (Engine.InputManager.IsKeyHeld("S")) Engine.Renderer.Camera.Y += 5;

            if (Engine.InputManager.IsKeyHeld("A")) Engine.Renderer.Camera.X -= 5;

            if (Engine.InputManager.IsKeyHeld("D")) Engine.Renderer.Camera.X += 5;

            if (Engine.InputManager.IsKeyDown("Space"))
            {
                Engine.ScriptingEngine.RunScript(@"function printTest(str) {
    help();
    print(str);
}

for(let i = 0; i < 10; i++) {
  printTest(i);
}");
            }
        }

        private Vector3 loce = new Vector3(0, 0, 0);
        private bool reverse = false;

        public override void Draw()
        {
            Engine.Renderer.Render(Vector3.Zero, Engine.GraphicsManager.RenderSize, Color.CornflowerBlue);
            loce.X += (0.3f * Engine.FrameTime) * (reverse ? -1 : 1);
            Engine.Renderer.Render(new Vector3(loce.X, 0, 0), new Vector2(50, Engine.GraphicsManager.RenderSize.Y), Color.White);
            Engine.Renderer.RenderCircle(new Vector3(0, 0, 0), 40,Color.Red/* ImGuiPlugin.Focused ? Color.Red : Color.Blue*/);
            Engine.Renderer.Render(new Vector3(loce.X + 50, 0, 0), new Vector2(50,  Engine.GraphicsManager.RenderSize.Y), Color.Black);
            if (loce.X >=  Engine.GraphicsManager.RenderSize.X - 100)
            {
                reverse = true;
            } else if (loce.X <= 0)
            {
                reverse = false;
            }

            Vector2 mousePos = Engine.InputManager.GetMousePosition();
            Engine.Renderer.Render(new Vector3(mousePos, 0), new Vector2(10, 10), Color.Pink);

            Color p = Color.Pink;
            p.A = 125;

            for (int i = 0; i < clickLocation.Count; i++)
            {
                Engine.Renderer.Render(new Vector3(clickLocation[i], 0), new Vector2(10, 10), p);
            }

            Engine.Renderer.RenderOutline(Engine.Renderer.Camera.Position, Engine.Renderer.Camera.Size, Color.Pink);

            Engine.Renderer.Render(test);

            Texture tt = Engine.AssetLoader.Get<Texture>("Textures/16bmp.bmp");
            Engine.Renderer.Render(new Vector3(200, 200, 10), new Vector2(100, 100), Color.White, tt);

            //ImGui.NewFrame();
            //ImGui.NewLine();
            //ImGuiPlugin.UseFont("calibri.ttf");
            //ImGui.Text("dasdsad");
            //ImGuiPlugin.UseFont(null);
            //ImGui.Text("dasdsad");
            //Texture tt = Engine.AssetLoader.Get<Texture>("Textures/logoAlpha.png");
            //Engine.Renderer.Render(new Vector3(200, 200, 10), new Vector2(100, 100), Color.White, tt);
            //ImGui.Image(new IntPtr(tt.Pointer), new Vector2(100, 100));
            //Engine.Renderer.RenderGui();
        }

        public override void Unload()
        {
        }
    }
}
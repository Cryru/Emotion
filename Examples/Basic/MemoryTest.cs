// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Diagnostics;
using Soul.Engine;
using Soul.Engine.ECS;
using Soul.Engine.ECS.Components;
using Soul.Engine.Graphics.Components;
using Soul.Engine.Modules;
using Soul.Engine.Scenography;

#endregion

namespace Examples.Basic
{
    public class MemoryTest : Scene
    {
        bool firstUpdate = true;
        Process _currentProcess = Process.GetCurrentProcess();
        private long startingRAM;
        public static void Main()
        {
            Settings.LoadingScene = typeof(LoadingScreen);
            Core.Setup(new MemoryTest());
        }

        protected override void Setup()
        {
            startingRAM = _currentProcess.PrivateMemorySize64 / 1024 / 1024;
          
            for (int i = 0; i < 10000; i++)
            {
                Entity temp = Entity.CreateBasicDrawable("TestEntity " + i);
                AddEntity(temp);
            }
        }

        protected override void Update()
        {
            if (firstUpdate)
            {
                _currentProcess = Process.GetCurrentProcess();
                long maxRAM = _currentProcess.PrivateMemorySize64 / 1024 / 1024;

                for (int i = 0; i < 10000; i++)
                {
                    RemoveEntity("TestEntity " + i);
                }

                long clearedRAM = _currentProcess.PrivateMemorySize64 / 1024 / 1024;

                Console.WriteLine(" Starting / Max / Cleared ");
                Console.WriteLine(" " + startingRAM + " / " + maxRAM + " / " + clearedRAM + " ");

                firstUpdate = false;
            }

            base.Update();
        }
    }

    public class LoadingScreen : Scene
    {
        protected override void Setup()
        {
            AssetLoader.LoadFont("Arial.ttf");

            Entity loadingText = Entity.CreateBasicText("loadingText");
            loadingText.GetComponent<Transform>().Position = new OpenTK.Vector2(0, 0);
            loadingText.GetComponent<Transform>().Size = new OpenTK.Vector2(100, 100);
            loadingText.GetComponent<TextData>().Text = "Loading..";
            loadingText.GetComponent<TextData>().Font = AssetLoader.GetFont("Arial.ttf");

            AddEntity(loadingText);
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
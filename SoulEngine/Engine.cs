using Raya.Enums;
using Raya.Graphics;
using Raya.Graphics.Primitives;
using Raya.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoulEngine
{
    public class Engine
    {
        public Context RayaContext;

        public Engine()
        {
            Console.WriteLine("Starting using Raya version " + RayaSettings.Version);

            // Test boot time.
            Stopwatch runTest = new Stopwatch();
            runTest.Start();

            RayaContext = new Context();
            Window RayaWindow = RayaContext.CreateWindow();

            // Boot ready.
            runTest.Stop();
            Console.WriteLine("Raya loaded in: " + runTest.ElapsedMilliseconds + " ms");

            // Run console thread.
            Thread consoleThread = new Thread(new ThreadStart(ConsoleThread));
            consoleThread.Start();

            // Main loop.
            int detail = 5;

            Image Image = new Image((uint)(960 / detail), (uint)(540 / detail));
            Texture Texture = new Texture(Image);
            Sprite Sprite = new Sprite(Texture);
            Sprite.Size = new Vector2(960, 540);

            Font font = new Font("data/arial.ttf");
            Text fpsMeter = new Text(RayaContext.FPS.ToString(), font);
            fpsMeter.FillColor = Color.Yellow;
            fpsMeter.OutlineColor = Color.Black;
            fpsMeter.OutlineThickness = 2;
            fpsMeter.CharacterSize = 17;
            fpsMeter.Position = new Vector2f(5, 5);

            while (RayaContext.Running)
            {
                //test.Rotate(5f);
                switch (command)
                {
                    case "borderless":
                        RayaContext.Window.Mode = WindowMode.Borderless;
                        command = "";
                        break;
                    case "fullscreen":
                        RayaContext.Window.Mode = WindowMode.Fullscreen;
                        command = "";
                        break;
                    case "normal":
                        RayaContext.Window.Mode = WindowMode.Window;
                        command = "";
                        break;
                    case "resize":
                        RayaContext.Window.Resizable = true;
                        command = "";
                        break;
                    case "nosize":
                        RayaContext.Window.Resizable = false;
                        command = "";
                        break;
                }

                if (command.Length > 6 && command.Substring(0, 6) == "detail")
                {
                    string[] split = command.Split(' ');
                    detail = int.Parse(split[1]);

                    Image = new Image((uint)(960 / detail), (uint)(540 / detail));
                    Texture = new Texture(Image);
                    Sprite = new Sprite(Texture)
                    {
                        Size = new Vector2(960, 540)
                    };
                    command = "";
                }

                RayaContext.Tick();

                for (int x = 0; x < 960 / detail; x += 1)
                {
                    for (int y = 0; y < 540 / detail; y += 1)
                    {
                        int C = generateRandomNumber(0, 255);
                        Image.SetPixel((uint)x, (uint)y, new Color(C, C, C));
                    }
                }

                Texture.Update(Image);
                fpsMeter.DisplayedString = RayaContext.FPS.ToString();


                RayaContext.StartDraw();

                RayaContext.Window.Draw(Sprite);
                RayaContext.Window.Draw(fpsMeter);

                RayaContext.EndDraw();
            }
        }

        void ConsoleThread()
        {
            while (RayaContext.Running)
            {
                command = Console.ReadLine();
            }
        }

        private static Random generator = new Random();
        public static int generateRandomNumber(int Min = 0, int Max = 100)
        {
            return generator.Next(Min, Max + 1); //We add one because by Random.Next does not include max.
        }

        public static string command = "";
    }
}

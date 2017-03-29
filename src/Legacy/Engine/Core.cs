using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Legacy.Objects;
using Microsoft.Xna.Framework;
using SoulEngine.Objects;
using SoulEngine.Events;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SoulEngine.Legacy
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // This code is part of the SoulEngine backwards compatibility layer.       //
    // Original Repository: https://github.com/Cryru/SoulEngine-2016            //
    //////////////////////////////////////////////////////////////////////////////
    public static class Core
    {
        public static string Name
        {
            get
            {
                return Info.Name;
            }
        }
        public static string Version
        {
            get
            {
                return Info.Version;
            }
        }
        public static string GUID
        {
            get
            {
                return Info.GUID;
            }
        }

        public static TextObject debugText;
        public static TextObject fpsText;

        public static float frametime
        {
            get
            {
                return Context.Core.frameTime;
            }
        }

        public static GraphicsDeviceManager graphics
        {
            get
            {
                return Context.GraphicsManager;
            }
        }
        public static SpriteBatch ink
        {
            get
            {
                return Context.ink;
            }
        }
        public static Engine host;
        public static Camera maincam
        {
            get
            {
                return Context.Camera;
            }
        }
        public static ScreenAdapter ScreenAdapter
        {
            get
            {
                return Context.Screen;
            }
        }
        public static GameTime gameTime;

        public static Objects.Texture blankTexture
        {
            get
            {
                return new Objects.Texture(AssetManager.BlankTexture);
            }
        }
        public static Objects.Texture missingTexture
        {
            get
            {
                return new Objects.Texture(AssetManager.MissingTexture);
            }
        }

        public static List<Ticker> Timers = new List<Ticker>();
        public static List<Screen> Screens = new List<Screen>();

        public static int RandomSeed = 0;

        public static Objects.Event onUpdate = new Objects.Event();
        public static Objects.Event onDraw = new Objects.Event();
        public static Objects.Event onClosing = new Objects.Event();

        public static void Setup()
        {
            ESystem.Add(new Listen(EType.GAME_TICKSTART, Update));
            ESystem.Add(new Listen(EType.GAME_FRAMESTART, Draw));
            ESystem.Add(new Listen(EType.GAME_FRAMEEND, Draw_End));

            if (Settings.StartScreen != null) LoadScreen(Settings.StartScreen, 0);
        }

        private static void LoadGlobalObjects()
        {
            debugText = new TextObject(Font: AssetManager.DefaultFont);
            debugText.Tags.Add("debugText", "");
            debugText.Color = Color.Yellow;
            debugText.Outline = true;
            debugText.autoSizeX = true;
            debugText.autoSizeY = true;
            debugText.ChildrenInheritVisibiity = false;

            debugText.EnableBackground(blankTexture, Color.Black, 0.5f, 0);

            fpsText = new TextObject(Font: AssetManager.DefaultFont);
            fpsText.Tags.Add("fpsText", "");
            fpsText.Color = Color.Yellow;
            fpsText.Outline = true;
            fpsText.autoSizeX = true;
            fpsText.autoSizeY = true;
            fpsText.ChildrenInheritVisibiity = false;

            fpsText.EnableBackground(blankTexture, Color.Black, 0.5f, 0);
        }

        public static void Update(Events.Event e)
        {
            GameTime gameTime = (GameTime) e.Data;

            Core.gameTime = gameTime;

            if (Settings.debug && Settings.debugUpdate)
            {
                debugText.Text = Name + " " + Version + "\r\n" + "Window Resolution: " + Settings.win_width + "x" + Settings.win_height + "\r\n"
                    + "Render Resolution: " + Settings.game_width + "x" + Settings.game_height + "\r\n" + "Camera Zoom: " + maincam.Zoom + "\r\n" +
                    "Globals (T/U/D): " + Timers.Count + " / " + onUpdate.Count() + " / " + onDraw.Count();
            }

            if (Settings.displayFPS)
            {
                if (Settings.fpsUpdate) fpsText.Text = "FPS: " + Debugging.DebugScene.FPS;
                fpsText.Location = new Vector2(Settings.game_width - fpsText.Width, 0);
            }

            FullScreenKeyToggle();
        }
        public static void Draw(Events.Event e)
        {
            GameTime gameTime = (GameTime)e.Data;

            graphics.GraphicsDevice.Clear(Settings.fillcolor);

            DrawOnScreen();

            ink.Draw(blankTexture.Image, new Rectangle(0, 0, Settings.game_width, Settings.game_height), Settings.drawcolor);
            ink.End();
        }
        public static void Draw_End(Events.Event e)
        {
            GameTime gameTime = (GameTime)e.Data;

            DrawOnScreen();

            if (Settings.displayFPS)
            {
                fpsText.Draw();
            }

            if (Settings.debug)
            {
                debugText.Draw();
            }
            ink.End();
        }

        public static void FullScreenKeyToggle()
        {
            if (Input.isKeyDown(Keys.LeftAlt) && Input.KeyDownTrigger(Keys.Enter))
            {
                Settings.win_fullscreen = !Settings.win_fullscreen;
            }
        }

        public static void LoadScreen(Screen startScreen, int priority)
        {

        }

        public static Vector2 GetScreenSize()
        {
            return Functions.GetScreenSize();
        }

        public static float DegreesToRadians(int angle)
        {
            return MathHelper.ToRadians(angle);
        }
        public static int RadiansToDegrees(float radian)
        {
            return (int) MathHelper.ToDegrees(radian);
        }

        public static float getDistance(Vector2 p1, Vector2 p2)
        {
            double a = Math.Abs(p1.X - p2.X);
            double b = Math.Abs(p1.Y - p2.Y);

            float c = (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));

            return c;
        }

        public static Vector2 StringToVector2(string s, char separator = ',')
        {
            string[] split = s.Split(separator);
            return new Vector2(float.Parse(split[0]), float.Parse(split[1]));
        }

        public static Vector2 PointToScreen(Vector2 Point)
        {
            return maincam.ViewToScreen(Point);
        }

        public static void DrawOnScreen()
        {
            ink.Start(Enums.DrawChannel.Screen);
        }
        public static void DrawOnWorld()
        {
            ink.Start(Enums.DrawChannel.World);
        }
        public static void DrawOnNone()
        {
            ink.Start(Enums.DrawChannel.Terminus);
        }

    }
}

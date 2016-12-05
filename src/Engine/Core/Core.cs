using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Graphics;
using SoulEngine.Objects;


namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The engine's core. Most of the stuff required for everything to run is here.
    /// </summary>
    public static class Core
    {
        #region "Declarations"
        #region "Engine Information"
        /// <summary>
        /// The name of the engine.
        /// </summary>
        public static string Name = "SoulEngine";
        /// <summary>
        /// The version of the engine.
        /// </summary>
        public static string Version = "0.94d_4";
        /// <summary>
        /// The GUID of the application. Used on windows to prevent multi-instancing.
        /// The default SoulEngine GUID - 130F150C-0000-0000-0000-050E07090E05
        /// </summary>
        public static string GUID = "130F150C-0000-0000-0000-050E07090E05";
        #endregion
        #region "Debug Variables and Objects"
        /// <summary>
        /// The debug information text object.
        /// </summary>
        public static TextObject debugText;
        /// <summary>
        /// The FPS text object.
        /// </summary>
        public static TextObject fpsText;
        #endregion
        #region "Frame Data"
        /// <summary>
        /// The time it took in ms to render the last frame.
        /// </summary>
        public static float frametime;
        #endregion
        #region "Internal Objects"
        /// <summary>
        /// The graphics device.
        /// </summary>
        public static GraphicsDeviceManager graphics;
        /// <summary>
        /// The drawing object.
        /// </summary>
        public static SpriteBatch ink;
        /// <summary>
        /// The game instance for use by the engine.
        /// </summary>
        public static Engine host;
        /// <summary>
        /// The main camera.
        /// </summary>
        public static Camera2D maincam;
        /// <summary>
        /// The screen's boxing adapter.
        /// </summary>
        public static BoxingViewportAdapter ScreenAdapter;
        /// <summary>
        /// An object for tracking time.
        /// </summary>
        public static GameTime gameTime;
        #endregion
        #region "Internal Content"
        /// <summary>
        /// A blank texture.
        /// </summary>
        public static Objects.Texture blankTexture;
        /// <summary>
        /// A texture to display when attempting to load a missing texture.
        /// </summary>
        public static Objects.Texture missingTexture;
        /// <summary>
        /// The default font for when a font is missing, or when rendering debug text.
        /// </summary>
        public static SpriteFont fontDebug;
        #endregion
        #region "Systems"
        /// <summary>
        /// Timers that will be run every frame.
        /// </summary>
        public static List<Timer> Timers = new List<Timer>();
        /// <summary>
        /// The screens that are rendered.
        /// </summary>
        public static List<Screen> Screens = new List<Screen>();
        /// <summary>
        /// A seed used for generating random numbers.
        /// </summary>
        public static int RandomSeed = 0;
        #endregion
        #region "Events"
        /// <summary>
        /// These are triggered at the end of every tick, before the tick ending code.
        /// </summary>
        public static Objects.Internal.Event onUpdate = new Objects.Internal.Event();
        /// <summary>
        /// These are triggered at the end of every frame, before the frame ending code.
        /// </summary>
        public static Objects.Internal.Event onDraw = new Objects.Internal.Event();
        /// <summary>
        /// Triggered just before the window closes.
        /// </summary>
        public static Objects.Internal.Event onClosing = new Objects.Internal.Event();
        #endregion
        #endregion

        #region "Boot"
        /// <summary>
        /// Creates the "Game" class instance.
        /// </summary>
        public static void Setup()
        {
            //Setup the host. The content, ink and graphics device are initalized here.
            host = new Engine();

            //Start the host.
            host.Run();

            //The code after run is executed after the game is closed.
            host.Dispose();
        }
        /// <summary>
        /// Setups the engine.
        /// </summary>
        public static void StartSequence()
        {
            //Load setings file.
            Settings.ReadSettings();

            //Allow fast exit.
            host.Window.AllowAltF4 = true;

            //Set the window's name.
            host.Window.Title = Settings.win_name;

            //Setup the screen, and the screen adapter.
            ScreenSettingsRefresh();
            ScreenAdapter = new BoxingViewportAdapter(host.Window, host.GraphicsDevice, Settings.game_width, Settings.game_height);
            
            //Setup the camera.
            maincam = new Camera2D(ScreenAdapter);

            //Load the global resources
            LoadGlobalContent();

            //Load the global objects.
            LoadGlobalObjects();

            //Load the starting screen.
            if (Settings.StartScreen != null) LoadScreen(Settings.StartScreen, 0);
        }
        /// <summary>
        /// Loads and setups global content.
        /// </summary>
        private static void LoadGlobalContent()
        {
            //Load the missingTexture from its color array.
            missingTexture = new Objects.Texture(new Texture2D(graphics.GraphicsDevice, 100, 100));
            missingTexture.Image.SetData<Color>(Content.MissingTexture.data);
            missingTexture.ImageName = "missing";

            //Create the blank texture.
            blankTexture = new Objects.Texture(new Texture2D(graphics.GraphicsDevice, 1, 1));
            Color[] data = new Color[] { Color.White, Color.White }; //Fill texture.
            blankTexture.Image.SetData<Color>(data); //Assign data to texture.
            blankTexture.Image.Name = "blank"; //Set a name for the texture.

            //Load default fonts
            fontDebug = Content.Load.Font("Fonts/Font_DEBUG");
        }
        /// <summary>
        /// Loads and setups the global objects.
        /// </summary>
        private static void LoadGlobalObjects()
        {
            //Setup the debugText object.
            debugText = new TextObject(Font: fontDebug);
            debugText.Tags.Add("debugText", "");
            debugText.Color = Color.Yellow;
            debugText.Outline = true;
            debugText.autoSizeX = true;
            debugText.autoSizeY = true;
            debugText.ChildrenInheritVisibiity = false;

            //Load the debugtext background.
            debugText.EnableBackground(blankTexture, Color.Black, 0.5f, 0);

            //Setup the fps text object.
            fpsText = new TextObject(Font: fontDebug);
            fpsText.Tags.Add("fpsText", "");
            fpsText.Color = Color.Yellow;
            fpsText.Outline = true;
            fpsText.autoSizeX = true;
            fpsText.autoSizeY = true;
            fpsText.ChildrenInheritVisibiity = false;

            //Load the FPS display background.
            fpsText.EnableBackground(blankTexture, Color.Black, 0.5f, 0);
        }
        #endregion
        #region "Loops"
        /// <summary>
        /// Is executed every frame on the CPU.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            //Assign the random seed.
            RandomSeed = gameTime.TotalGameTime.Milliseconds;

            //Update the gametime variable.
            Core.gameTime = gameTime;

            //Update the showing mouse setting.
            host.IsMouseVisible = Settings.win_renderMouse;

            //Update the debug text.
            if (Settings.debug && Settings.debugUpdate)
            {
                //Write the FPS and framework info to the debug text.
                debugText.Text = Core.Name + " " + Core.Version + "\r\n" + "Window Resolution: " + Settings.win_width + "x" + Settings.win_height + "\r\n"
                    + "Render Resolution: " + Settings.game_width + "x" + Settings.game_height + "\r\n" + "Camera Zoom: " + maincam.Zoom + "\r\n" +
                    "Globals (T/U/D): " + Timers.Count + " / " + onUpdate.Count() + " / " + onDraw.Count();
            }

            //Update the fps counter.
            if (Settings.displayFPS)
            {
                if(Settings.fpsUpdate) fpsText.Text = "FPS: " + lastFrames;
                fpsText.Location = new Vector2(Settings.game_width - fpsText.Width, 0);
            }

            //Update the render time for the last frame.
            frametime = gameTime.ElapsedGameTime.Milliseconds;

            //Update the input for the current frame.
            Input.UpdateInput();

            //Run the fullscreen key toggling code.
            FullScreenKeyToggle();

            //Run the hooked timers.
            for (int i = Timers.Count - 1; i >= 0; i--)
            {
                //Check if the timer has finished running.
                if (Timers[i].State == Timer.TimerState.Done)
                {
                    //If finished, purge it.
                    Timers.RemoveAt(i);
                }
                else
                {
                    //if not finished, run it.
                    Timers[i].Run();
                }
            }

            //Update hooked methods.
            onUpdate.Trigger();
        }
        /// <summary>
        /// Is run when the frame ends, before we go on to the next one.
        /// </summary>
        public static void Update_End(GameTime gameTime)
        {

        }
        /// <summary>
        /// Is executed every frame on the GPU.
        /// </summary>
        public static void Draw(GameTime gameTime)
        {
            //Color everything in the fillcolor.
            graphics.GraphicsDevice.Clear(Settings.fillcolor);
            //Start a draw sequence on the screen. As opposed to in the world.
            DrawOnScreen();
            //Draw the render space color.
            ink.Draw(blankTexture.Image, new Rectangle(0, 0, Settings.game_width, Settings.game_height), Settings.drawcolor);
            //End drawing.
            ink.End();
            //Update hooked methods.
            onDraw.Trigger();
        }
        /// <summary>
        /// Is run when the frame ends, before we go on to the next one.
        /// </summary>
        public static void Draw_End(GameTime gameTime)
        {
            //Draw on the screen.
            DrawOnScreen();
            //Check if we are drawing the FPS counter.
            if (Settings.displayFPS)
            {
                fpsText.Draw();
            }
            //Check if we are drawing the debug string.
            if (Settings.debug)
            {
                debugText.Draw();
            }
            ink.End();

            //Update the FPS counter. This is done here as the draw loops are run on the GPU.
            FPSCounterUpdate(gameTime);
			
			//Prepare the input for the next frame.
            Input.UpdateInput_End();
        }
        #endregion
        #region "Fullscreen and FPS Counter Functions"
        /// <summary>
        /// The width of the main window kept while in fullscreen.
        /// </summary>
        public static int TEMPwin_width = Settings.win_width;
        /// <summary>
        /// The height of the main window kept while in fullscreen.
        /// </summary>
        public static int TEMPwin_height = Settings.win_height;

        /// <summary>
        /// Checks if the fullscreen key is toggled and executes the appropriate code.
        /// </summary>
        public static void FullScreenKeyToggle()
        {
            if (Input.isKeyDown(Keys.LeftAlt) && Input.KeyDownTrigger(Keys.Enter))
            {
                //Invert the fullscreen variable.
                Settings.win_fullscreen = !Settings.win_fullscreen;
                //Refresh the fullscreen state.
                ScreenSettingsRefresh();
            }
        }
        /// <summary>
        /// Applies the screen settings.
        /// </summary>
        public static void ScreenSettingsRefresh()
        {
            //Check if fullscreen is on.
            if (Settings.win_fullscreen == true)
            {
                //Remove the window borders.
                host.Window.IsBorderless = true;
                //Set the size of the window to the screen size.
                Settings.win_width = (int) GetScreenSize().X;
                Settings.win_height = (int) GetScreenSize().Y;

                //Move the window to the top left.
                host.Window.Position = new Point(0, 0);
            }

            //If fullscreen is off.
            if (Settings.win_fullscreen == false)
            {
                //Show the window borders.
                host.Window.IsBorderless = false;
                //Set the size of the window to the stored size.
                Settings.win_width = TEMPwin_width;
                Settings.win_height = TEMPwin_height;
            }

            //Setup the screen again with the new values.
            graphics.PreferredBackBufferWidth = Settings.win_width;
            graphics.PreferredBackBufferHeight = Settings.win_height;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// The frames rendered in the current second.
        /// </summary>
        public static int curFrames = 0;
        /// <summary>
        /// The frames rendered in the last second.
        /// </summary>
        public static int lastFrames = 0;
        /// <summary>
        /// The current second number.
        /// </summary>
        public static int curSec = 0;
        /// <summary>
        /// Updates the FPS counter.
        /// </summary>
        /// <param name="gameTime">The timestamp.</param>
        public static void FPSCounterUpdate(GameTime gameTime)
        {
            //Check if the current second has passed.
            if (!(curSec == gameTime.TotalGameTime.Seconds))
            {
                curSec = gameTime.TotalGameTime.Seconds; //Assign the current second to a variable.
                lastFrames = curFrames; //Set the current second's frames to the last second's frames as a second has passed.
                curFrames = 0;
            }
            curFrames += 1;
        }
        #endregion
        #region "Functions"
        /// <summary>
        /// Returns the resolution of the screen.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetScreenSize()
        {
            return new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        }
        /// <summary>
        /// Converts the angle in degrees to radians.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>The degrees in radians.</returns>
        public static float DegreesToRadians(int angle)
        {
            //Check if the angle is over the maximum.
            recheckMax:
            if (angle > 360)
            {
                angle -= 360;
                goto recheckMax;
            }
            //Check if the angle is over the minimum.
            recheckMin:
            if (angle < 0)
            {
                angle += 360;
                goto recheckMin;
            }

            return (((float)Math.PI) / 180) * angle;
            //Divide Pi by 180 and multiply by the angle.
        }
        /// <summary>
        /// Converts the radians to angles.
        /// </summary>
        /// <param name="radian">Angle in radians.</param>
        /// <returns>The radians in degrees</returns>
        public static int RadiansToDegrees(float radian)
        {
            return (int)((180 / ((float)Math.PI)) * radian);
            //Divide 180 by Pi and multiply by the radians. Convert to an integer.
        }
        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns></returns>
        public static float getDistance(Vector2 p1, Vector2 p2)
        {
            //Subtract the Xs and Ys to get the points of the triangle.
            double a = Math.Abs(p1.X - p2.X);
            double b = Math.Abs(p1.Y - p2.Y);

            //Pythagorean theorem.
            float c = (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));

            return c;
        }
        /// <summary>
        /// Converts a string to a Vector2.
        /// </summary>
        /// <param name="s">The string itself.</param>
        /// <param name="separator">What the two values of the vector are separated by in the string.</param>
        /// <returns></returns>
        public static Vector2 StringToVector2(string s, char separator = ',')
        {
            string[] split = s.Split(separator);
            return new Vector2(float.Parse(split[0]), float.Parse(split[1]));
        }
        /// <summary>
        /// Converts a point in the world to it's location on the screen.
        /// </summary>
        /// <param name="Point">The point to transform.</param>
        /// <returns></returns>
        public static Vector2 PointToScreen(Vector2 Point)
        {
            return Vector2.Transform(Point, Matrix.Invert(maincam.GetViewMatrix()));
        }
        /// <summary>
        /// Starts drawing on the screen..
        /// </summary>
        /// <param name="drawMode">The drawing mode.</param>
        public static void DrawOnScreen(DrawMode drawMode = DrawMode.Default)
        {
            switch (drawMode)
            {
                case DrawMode.AA:
                    ink.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, RasterizerState.CullNone,
   null, ScreenAdapter.GetScaleMatrix());
                    break;
                case DrawMode.Pixelly:
                    ink.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, RasterizerState.CullNone,
   null, ScreenAdapter.GetScaleMatrix());
                    break;
                case DrawMode.Default:
                    ink.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenAdapter.GetScaleMatrix());
                    break;
            }
        }
        /// <summary>
        /// Starts drawing through the camera.
        /// </summary>
        /// <param name="drawMode">The drawing mode.</param>
        public static void DrawOnWorld(DrawMode drawMode = DrawMode.Default)
        {
            switch (drawMode)
            {
                case DrawMode.AA:
                    ink.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, RasterizerState.CullNone,
   null, maincam.GetViewMatrix());
                    break;
                case DrawMode.Pixelly:
                    ink.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, RasterizerState.CullNone,
   null, maincam.GetViewMatrix());
                    break;
                case DrawMode.Default:
                    ink.Begin(SpriteSortMode.Deferred, null, null, null, null, null, maincam.GetViewMatrix());
                    break;
            }
        }
        /// <summary>
        /// Starts drawing, warped through no matrix.
        /// </summary>
        /// <param name="drawMode">The drawing mode.</param>
        public static void DrawOnNone(DrawMode drawMode = DrawMode.Default)
        {
            switch (drawMode)
            {
                case DrawMode.AA:
                    ink.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp, null, RasterizerState.CullNone,
   null, null);
                    break;
                case DrawMode.Pixelly:
                    ink.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, RasterizerState.CullNone,
   null, null);
                    break;
                case DrawMode.Default:
                    ink.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);
                    break;
            }
        }
        #endregion
        #region "Screens"
        /// <summary>
        /// Loads a screen.
        /// </summary>
        /// <param name="Screen">The screen to load.</param>
        /// <param name="Priority">The priority of the screen.</param>
        public static void LoadScreen(Screen Screen, int Priority)
        {
            Screens.Add(Screen);
            Screen.Priority = Priority;
            RefreshScreens();
            Screen.PhysicsSetup();
        }
        /// <summary>
        /// Unloads the screen.
        /// </summary>
        /// <param name="Screen">The screen to unload.</param>
        public static void UnloadScreen(Screen Screen)
        {
            if (Screens.IndexOf(Screen) != -1) Screens.Remove(Screen);
            RefreshScreens();
        }
        /// <summary>
        /// Reorders the screens based on priority.
        /// </summary>
        public static void RefreshScreens()
        {
            Screens.OrderBy(x => x.Priority);
        }
        /// <summary>
        /// Runs the update functions of all screens.
        /// </summary>
        public static void UpdateScreens()
        {
            for (int i = 0; i < Screens.Count; i++)
            {
                CheckIfObjectsHaveBeenLoaded(i);
                Screens[i].Update();
            }
        }
        /// <summary>
        /// Runs the draw functions of all screens.
        /// </summary>
        public static void DrawScreens()
        {
            for (int i = 0; i < Screens.Count; i++)
            {
                CheckIfObjectsHaveBeenLoaded(i);
                Screens[i].Draw();
            }
        }
        /// <summary>
        /// Checks if the objects have been loaded.
        /// </summary>
        private static void CheckIfObjectsHaveBeenLoaded(int index)
        {
            if(Screens[index].ObjectsLoaded == false)
            {
                Screens[index].LoadObjects();
                Screens[index].ObjectsLoaded = true;
            }
        }
        #endregion
    }
    /// <summary>
    /// The drawing modes.
    /// </summary>
    public enum DrawMode
    {
        /// <summary>
        /// Anti-Aliasing
        /// </summary>
        AA,
        /// <summary>
        /// Pixelly
        /// </summary>
        Pixelly,
        /// <summary>
        /// Some smoothing.
        /// </summary>
        Default
    }
}

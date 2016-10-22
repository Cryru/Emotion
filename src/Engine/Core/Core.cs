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
    class Core
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
        public static string Version = "0.80";
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
#if ANDROID
        /// <summary>
        /// Hosts the Android's context host.
        /// </summary>
        public static Android.Content.Context androidHost;
#endif
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
        /// Methods that are run every frame on the CPU.
        /// </summary>
        public static Objects.Internal.Event<string> Updates = new Objects.Internal.Event<string>();
        /// <summary>
        /// Methods that are run every frame on the GPU.
        /// </summary>
        public static Objects.Internal.Event<string> DrawUpdates = new Objects.Internal.Event<string>();
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
            ScreenAdapter = new BoxingViewportAdapter(host.Window, graphics, Settings.game_width, Settings.game_height);

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
        public static void LoadGlobalContent()
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
            fontDebug = LoadFont("Font_DEBUG");
        }
        /// <summary>
        /// Loads and setups the global objects.
        /// </summary>
        public static void LoadGlobalObjects()
        {
            //Setup the debugText object.
            debugText = new TextObject(Font: fontDebug);
            debugText.Tags.Add("debugText");
            debugText.Color = Color.Yellow;
            debugText.Outline = true;
            debugText.autoSizeX = true;
            debugText.autoSizeY = true;
            debugText.Background = true;

            //Load the debugtext background.
            debugText.backgroundImage = blankTexture;
            debugText.backgroundColor = Color.Black;
            debugText.backgroundOpacity = 0.5f;

            //Setup the fps text object.
            fpsText = new TextObject(Font: fontDebug);
            fpsText.Tags.Add("fpsText");
            fpsText.Color = Color.Yellow;
            fpsText.Outline = true;
            fpsText.autoSizeX = true;
            fpsText.autoSizeY = true;
            fpsText.Background = true;

            //Load the FPS display background.
            fpsText.backgroundImage = blankTexture;
            fpsText.backgroundColor = Color.Black;
            fpsText.backgroundOpacity = 0.5f;
        }
        #endregion
        #region "Loops"
        /// <summary>
        /// Is executed every frame on the CPU.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            //Update the input for the current frame.
            Input.UpdateInput();

            //Update the showing mouse setting.
#if WINDOWS
            host.IsMouseVisible = Settings.win_renderMouse;
#endif

            //Update the debug text.
            if (Settings.debug == true && Settings.debugUpdate == true)
            {
                //Write the FPS and framework info to the debug text.
                debugText.Text = Core.Name + " " + Core.Version + "\r\n" + "Window Resolution: " + Settings.win_width + "x" + Settings.win_height + "\r\n"
                    + "Render Resolution: " + Settings.game_width + "x" + Settings.game_height + "\r\n" + "Camera Zoom: " + maincam.Zoom + "\r\n" + "Global Timers Running: " + Timers.Count;
            }

            //Update the fps counter.
            if(Settings.displayFPS)
            {
                fpsText.Text = "FPS: " + lastFrames;
                fpsText.Location = new Vector2(Settings.game_width - fpsText.Width, 0);
            }

            //Update the render time for the last frame.
            frametime = gameTime.ElapsedGameTime.Milliseconds;

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
            Updates.Trigger("");
        }
        /// <summary>
        /// Is run when the frame ends, before we go on to the next one.
        /// </summary>
        public static void Update_End(GameTime gameTime)
        {
            //Prepare the input for the next frame.
            Input.UpdateInput_End();
        }
        /// <summary>
        /// Is executed every frame on the GPU.
        /// </summary>
        public static void Draw(GameTime gameTime)
        {
            //Color everything in the fillcolor.
            graphics.GraphicsDevice.Clear(Settings.fillcolor);
            //Start a draw sequence on the screen. As opposed to in the world.
            DrawScreen();
            //Draw the render space color.
            ink.Draw(blankTexture.Image, new Rectangle(0, 0, Settings.game_width, Settings.game_height), Settings.drawcolor);
            //End drawing.
            ink.End();

            //Update hooked methods.
            DrawUpdates.Trigger("");
        }
        /// <summary>
        /// Is run when the frame ends, before we go on to the next one.
        /// </summary>
        public static void Draw_End(GameTime gameTime)
        {
            DrawScreen();
            //Check if we are drawing the FPS counter.
            if (Settings.displayFPS == true)
            {
                fpsText.Draw();
            }
            //Check if we are drawing the debug string.
            if (Settings.debug == true)
            {
                debugText.Draw();
            }
            ink.End();

            //Update the FPS counter. This is done here as the draw loops are run on the GPU.
            FPSCounterUpdate(gameTime);
        }
        #endregion
        #region "Meta Functions"
        /// <summary>
        /// Meta Functions are the primary functions of the engine and it's modules.
        /// 
        /// Variables that are used in the functions below but are not separated in this region are
        /// settings variables like "win_fullscreen", "displayFPS", and such which are present in the
        /// varriable block above as they are expected to be used by the user.
        /// </summary>
        //---------------------------------------------------------------------------------

        //---------------------------------------------------------------------------------
        //Fullscreen management.
        public static int TEMPwin_width = Settings.win_width; //The width of the main window kept while in fullscreen.
        public static int TEMPwin_height = Settings.win_height; //The height of the main window kept while in fullscreen.
        public static bool win_fullscreen_oldstateDown; //A trigger for the fullscreen key.

        //Checks for when the fullscreen key is being toggled.
        public static void FullScreenKeyToggle()
        {
#if !ANDROID //Android doesn't have a fullscreen and non-fullscreen mode per say.
            //Check for fullcreen switching. (Default hotkey is Alt+Enter).
            if (Input.isKeyDown(Keys.LeftAlt) && Input.isKeyDown(Keys.Enter))
            {
                //Tell the trigger that the button has been pressed.
                win_fullscreen_oldstateDown = true;
            }
            //If the keys have been let go and the trigger claims they were pressed...
            else if (win_fullscreen_oldstateDown == true)
            {
                //Reset trigger.
                win_fullscreen_oldstateDown = false;
                //Invert the fullscreen variable.
                Settings.win_fullscreen = !Settings.win_fullscreen;
                //Refresh the fullscreen state.
                ScreenSettingsRefresh();
            }
#endif
        }
        //Applies the current fullscreen variable.
        public static void ScreenSettingsRefresh()
        {
#if !ANDROID //Android doesn't have a fullscreen and non-fullscreen mode per say.
            //Check if fullscreen is on.
            if (Settings.win_fullscreen == true)
            {
                //Remove the window borders.
                host.Window.IsBorderless = true;
                //Set the size of the window to the screen size.
                Settings.win_width = GetScreenWidth();
                Settings.win_height = GetScreenHeight();

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

                //Center window
                host.Window.Position = new Point(GetScreenWidth() / 2 - Settings.win_width / 2, GetScreenHeight() / 2 - Settings.win_height / 2);
            }

            //Setup the screen again with the new values.
            graphics.PreferredBackBufferWidth = Settings.win_width;
            graphics.PreferredBackBufferHeight = Settings.win_height;
            graphics.ApplyChanges();
#endif
#if ANDROID
            graphics.IsFullScreen = Settings.win_hidebar;
            graphics.SupportedOrientations = Settings.win_orientation;
#endif
        }

        //---------------------------------------------------------------------------------
        //Updates the FPS Counter.
        public static int curFrames = 0; //The frames rendered in the current frame.
        public static int lastFrames = 0; //The frames rendered in the last second.
        public static int curSec = 0; //The current second.

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

        //---------------------------------------------------------------------------------
        public enum DrawMode //The available draw modes.
        {
            AA,
            Pixelly,
            Default
        }

        //Starts a draw cycle on the screen.
        public static void DrawScreen(DrawMode drawMode = DrawMode.Default)
        {
            switch (drawMode)
            {
                case DrawMode.AA:
                    ink.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, RasterizerState.CullNone,
   null, ScreenAdapter.GetScaleMatrix());
                    break;
                case DrawMode.Pixelly:
                    ink.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, RasterizerState.CullNone,
   null, ScreenAdapter.GetScaleMatrix());
                    break;
                case DrawMode.Default:
                    ink.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenAdapter.GetScaleMatrix());
                    break;
            }

            //---------------------------------------------------------------------------------
        }
        //Starts a draw cycle on the camera/world.
        public static void DrawWorld(DrawMode drawMode = DrawMode.Default)
        {
            switch (drawMode)
            {
                case DrawMode.AA:
                    ink.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, RasterizerState.CullNone,
   null, maincam.GetViewMatrix());
                    break;
                case DrawMode.Pixelly:
                    ink.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, RasterizerState.CullNone,
   null, maincam.GetViewMatrix());
                    break;
                case DrawMode.Default:
                    ink.Begin(SpriteSortMode.Deferred, null, null, null, null, null, maincam.GetViewMatrix());
                    break;
            }
        }

        //---------------------------------------------------------------------------------
        //Returns a the point of the screen warped through the scaler.
        public static Point PointToScreen(Point Point, string renderLayer = "Screen")
        {
            if (renderLayer == "Screen") return ScreenAdapter.PointToScreen(Point);

            return Vector2.Transform(Point.ToVector2(), Matrix.Invert(maincam.GetViewMatrix())).ToPoint();
        }

        //---------------------------------------------------------------------------------
        #endregion
        #region "Library Functions"
        /// <summary>
        /// Library functions are the functions of the engine designed to be used by the user.
        /// Some functions are exceptions to this rule, like the "LoadScreen" function as they are
        /// thought to be more primary.
        /// </summary>

        //Returns the resolution of the screen of the device currently running.
        public static int GetScreenWidth()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        }
        public static int GetScreenHeight()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        //Converts degrees to radians.
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
        public static int RadiansToDegrees(float radian)
        {
            return (int)((180 / ((float)Math.PI)) * radian);
            //Divide 180 by Pi and multiply by the radians. Convert to an integer.
        }

        //Loads a texture by name, if the texture doesn't exist it will return the missing image image.
        public static Texture2D LoadTexture(string name)
        {
            if (GetContentExist(name))
            {
                try
                {
                    return host.Content.Load<Texture2D>("SCon/" + name);
                }
                catch
                {
                    return missingTexture.Image;
                }
            }
            else
            {
                return missingTexture.Image;
            }
        }

        //Loads a font by name, if the font doesn't exist it will return the debug font.
        public static SpriteFont LoadFont(string name, string folder = "Fonts")
        {
            if (GetContentExist(folder + "/" + name))
            {
                try
                {
                    return host.Content.Load<SpriteFont>("SCon/" + folder + "/" + name);
                }
                catch
                {
                    return fontDebug;
                }
            }
            else
            {
                return fontDebug;
            }
        }

        //Returns a boolean based on whether the content file exists.
        public static bool GetContentExist(string name)
        {
#if ANDROID //On android checking if files exist is done through the assets module.
            string assetpath = "Content/SCon"; //First we get the root of the assets where the content is stored.
            if (name.Contains("/")) assetpath += "/" + name.Substring(0, name.LastIndexOf('/')); //Next we append any folders from the name.
            string filename; //Then we remove the folders from the name if any.
            if (name.Contains("/")) filename = name.Substring(name.LastIndexOf('/') + 1) + ".xnb"; else filename = name + ".xnb";

            string[] allAssets = androidHost.Assets.List(assetpath); //We get a list of all assets in that folder.
            for (int i = 0; i < allAssets.Length; i++) //And we check if any of the assets in the folder are of the same name.
            {
                if(filename == allAssets[i])
                {
                    return true;
                }
            }
            return false;
#endif
#if __UNIFIED__
            return true;
#endif
            //Assign the path of the file.
            string contentpath = "Content\\SCon\\" + name.Replace("/", "\\") + ".xnb";
            //Check if the file exists.
            if (File.Exists(contentpath))
            {
                return true;
            }
            return false;
        }

        #region "Object Position and Size"
        //Centers the object within the window.
        public static void CenterObject(ObjectBase obj)
        {
            obj.X = Settings.game_width / 2 - obj.Width / 2;
            obj.Y = Settings.game_height / 2 - obj.Height / 2;
        }

        //Centers the object within the target object.
        public static void CenterObjectWithinObject(ObjectBase obj, ObjectBase tarObj)
        {
            obj.X = tarObj.Width / 2 - obj.Width / 2;
            obj.Y = tarObj.Height / 2 - obj.Height / 2;
        }

        //Resizes the object in a certain way.
        public static void ObjectFullscreen(ObjectBase obj)
        {
            obj.Width = Settings.game_width;
            obj.Height = Settings.game_height;
            obj.X = 0;
            obj.Y = 0;
        }

        //Copies the location and size of the target object.
        public static void ObjectCopy(ObjectBase obj, ObjectBase tarObj)
        {
            ObjectCopyLocation(obj, tarObj);
            ObjectCopySize(obj, tarObj);
        }

        //Copies the location of the target object.
        public static void ObjectCopyLocation(ObjectBase obj, ObjectBase tarObj)
        {
            obj.X = tarObj.X;
            obj.Y = tarObj.Y;
        }

        //Copies the size of the target object.
        public static void ObjectCopySize(ObjectBase obj, ObjectBase tarObj)
        {
            obj.Width = tarObj.Width;
            obj.Height = tarObj.Height;
        }
        #endregion

        //Returns the distance between two points.
        public static float getDistance(Vector2 p1, Vector2 p2)
        {
            //Subtract the Xs and Ys to get the points of the triangle.
            double a = Math.Abs(p1.X - p2.X);
            double b = Math.Abs(p1.Y - p2.Y);

            //Pythagorean theorem.
            float c = (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));

            return c;
        }

        //Returns an Image as a two dimensional color array.
        public static Color[,] ImageTo2DArray(Texture2D image)
        {
            //Get the one dimensional array.
            Color[] colorsOne = new Color[image.Width * image.Height];
            image.GetData(colorsOne);

            //Define a two dimensional array.
            Color[,] colorsTwo = new Color[image.Width, image.Height];
            //Load the two dimensional array.
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                    colorsTwo[x, y] = colorsOne[x + y * image.Width];

            //Return it.
            return colorsTwo;
        }
        //Transforms a two dimensional to a image.
        public static Texture2D ImageFrom2DArray(Color[,] dataTwo)
        {
            //Create a new texture.
            Texture2D newTexture = new Texture2D(Core.graphics.GraphicsDevice, dataTwo.GetLength(0), dataTwo.GetLength(1));
            List<Color> dataOne = new List<Color>();

            for (int x = 0; x < dataTwo.GetLength(1); x++)
            {
                for (int y = 0; y < dataTwo.GetLength(0); y++)
                {
                    dataOne.Add(dataTwo[y, x]);
                }
            }

            newTexture.SetData(dataOne.ToArray());

            return newTexture;
        }

        #endregion
        #region "Soul Library Functions"
        //---------------------------------------------------------------------------------------------------------------------------
        //Writes the specified string to the specified file, if it doesn't exist it will be created.
        public static void IO_WriteFile(string filepath, string datatowrite)
        {
            try
            {
                StreamWriter writer = File.CreateText(filepath);
                //Write the contents.
                writer.Write(datatowrite);
                //Close the writer to free the file
                writer.Close();
                writer.Dispose();
            }
            catch { }

        }
        //---------------------------------------------------------------------------------------------------------------------------
        //Writes the specified string array to the specified file, if it doesn't exist it will be created.
        public static void IO_WriteFile_Array(string filepath, string[] datatowrite)
        {
            IO_WriteFile(filepath, ArrayToString(datatowrite));
        }
        //---------------------------------------------------------------------------------------------------------------------------
        //Reads the specified file and returns a string holding the contents.
        public static string IO_ReadFile(string filepath)
        {
            string content;
            //Try to read the file.
            try
            {
                StreamReader reader = File.OpenText(filepath);
                //Transfer the contents onto the earlier defined variable.
                content = reader.ReadToEnd();
                //Close the reader to free the file.
                reader.Close();
                reader.Dispose();
                //Return the contents.
                return content;
            }
            catch { return null; }
        }
        //---------------------------------------------------------------------------------------------------------------------------
        //Reads the specified file and returns a string holding the contents.
        public static string[] IO_ReadFile_Array(string filepath)
        {
            string[] content;
            //Try to read the file.
            try
            {

#if ANDROID
                Stream mapfile = androidHost.Assets.Open(filepath);
                StreamReader reader = new StreamReader(mapfile, System.Text.Encoding.UTF8);
#endif
#if !ANDROID
                StreamReader reader = File.OpenText(filepath);
#endif
                //Transfer the contents onto the earlier defined variable.
                content = SplitNewLine(reader.ReadToEnd());
#if !ANDROID
                //Close the reader to free the file.
                reader.Close();
                reader.Dispose();
#endif
                //Return the contents.
                return content;
            }
            catch { return null; }
        }
        //---------------------------------------------------------------------------------------------------------------------------
        //Splits a string at a new line. Might not be needed.
        public static string[] SplitNewLine(string s)
        {
            return s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
        //---------------------------------------------------------------------------------------------------------------------------
        //String Array > Single String
        public static string ArrayToString(string[] arr, string LineSeparator = "/NEWLINE/")
        {
            //Check if the array is empty, not long enough or with an invalid delete index.
            if (arr == null || arr.Length == 0)
            {
                return "Empty Array";
            }
            //Convert line separator tags.
            if (LineSeparator == "/NEWLINE/") LineSeparator = Environment.NewLine;
            //Declare a variable to hold the line.
            string singleline = arr[0];

            //Check if the array is longer than one.
            if (!(arr.Length == 1))
            {
                //Look through all lines.
                for (int i = 1; i <= arr.Length - 1; i++)
                {
                    singleline += LineSeparator + arr[i];
                }
            }
            //Return line.
            return singleline;
        }
        //Single String > String Array
        public static string[] StringToArray(string s, char LineSeparator)
        {
            string[] arr;

            //Convert line separator tags.
            arr = s.Split(LineSeparator);
            //Remove empty array entries.

            //Return the converter string.
            return arr;
        }
        //---------------------------------------------------------------------------------------------------------------------------
        //String > Vector2
        public static Vector2 StringToVector2(string s, char separator = ',')
        {
            string[] split = s.Split(separator);
            return new Vector2(float.Parse(split[0]), float.Parse(split[1]));
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
            Screen.LoadObjects();
            RefreshScreens();
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
                Screens[i].Draw();
            }
        }
        #endregion
    }
}

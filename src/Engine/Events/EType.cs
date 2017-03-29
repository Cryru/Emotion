using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Events
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The types of events used by SE's custom event system.
    /// </summary>
    public partial class EType
    {
        #region "PRIMARY"
        /// <summary>
        /// Unknown type.
        /// </summary>
        public const string PRIMARY_GENERIC = "GENERIC";
        #endregion
        #region "TICKER"
        /// <summary>
        /// Triggered by a ticker each time it ticks. Data - The current tick's number.
        /// </summary>
        public const string TICKER_TICK = "TICKER_TICK";
        /// <summary>
        /// Triggered by a ticker when it has reached its tick limit.
        /// </summary>
        public const string TICKER_DONE = "TICKER_DONE";
        #endregion
        #region "INPUT"
        /// <summary>
        /// Triggered when text input is detected. Data - The character as a string.
        /// </summary>
        public const string INPUT_TEXT = "INPUT_TEXT";
        #endregion
        #region "GAME"
        /// <summary>
        /// Instantly triggered when the game is closed. Will not be triggered if forcelly closed or in case of a crash.
        /// </summary>
        public const string GAME_CLOSED = "GAME_CLOSED";
        /// <summary>
        /// Triggered when a tick update cycle begins.
        /// </summary>
        public const string GAME_TICKSTART = "GAME_TICKSTART";
        /// <summary>
        /// Triggered when a tick update cycle ends.
        /// </summary>
        public const string GAME_TICKEND = "GAME_TICKEND";
        /// <summary>
        /// Triggered at the start of a new frame.
        /// </summary>
        public const string GAME_FRAMESTART = "GAME_FRAMESTART";
        /// <summary>
        /// Triggered at the end of a new frame.
        /// </summary>
        public const string GAME_FRAMEEND = "GAME_FRAMEEND";
        #endregion
        #region "WINDOW"
        /// <summary>
        /// Triggered when the size of the window changes.
        /// </summary>
        public const string WINDOW_SIZECHANGED = "WINDOW_SIZECHANGED";
        /// <summary>
        /// Triggered when the window's display mode changes.
        /// </summary>
        public const string WINDOW_DISPLAYMODECHANGED = "WINDOW_DISPLAYMODECHANGED";
        #endregion
        #region "INPUT"
        /// <summary>
        /// Triggered when a key is pressed.
        /// </summary>
        public const string KEY_PRESSED = "KEY_PRESSED";
        /// <summary>
        /// Triggered when a key is let go.
        /// </summary>
        public const string KEY_UNPRESSED = "KEY_UNPRESSED";
        #endregion
        #region "ANIMATION"
        /// <summary>
        /// Triggered when an animation is done.
        /// </summary>
        public const string ANIM_FINISHED = "ANIM_FINISHED";
        #endregion
        #region "UI INPUT STATUS"
        /// <summary>
        /// Triggered when the mouse enters the a mouse input component.
        /// </summary>
        public const string MOUSEINPUT_ENTERED = "MOUSEINPUT_ENTERED";
        /// <summary>
        /// Triggered when the mouse leaves a mouse input component.
        /// </summary>
        public const string MOUSEINPUT_LEFT = "MOUSEINPUT_LEFT";
        /// <summary>
        /// Triggered when the mouse clicks a mouse input component.
        /// </summary>
        public const string MOUSEINPUT_CLICKDOWN = "MOUSEINPUT_CLICKDOWN";
        /// <summary>
        /// Triggered when the mouse lets go a mouse input component.
        /// </summary>
        public const string MOUSEINPUT_CLICKUP = "MOUSEINPUT_CLICKUP";
        #endregion
    }
}
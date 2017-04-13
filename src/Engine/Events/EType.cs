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
        /// Triggered when a key is pressed, the sender is the key.
        /// </summary>
        public const string KEY_PRESSED = "KEY_PRESSED";
        /// <summary>
        /// Triggered when a key is let go, the sender is the key.
        /// </summary>
        public const string KEY_UNPRESSED = "KEY_UNPRESSED";
        /// <summary>
        /// Triggered when the mouse moves, the sender is the previous position, 
        /// current position can be gotten from the Input library.
        /// </summary>
        public const string MOUSE_MOVED = "MOUSE_MOVED";
        /// <summary>
        /// Triggered when the mouse wheel is scrolled up, the sender is scroll difference.
        /// </summary>
        public const string MOUSE_SCROLLUP = "MOUSE_SCROLLUP";
        /// <summary>
        /// Triggered when the mouse wheel is scrolled down, the sender is scroll difference.
        /// </summary>
        public const string MOUSE_SCROLLDOWN = "MOUSE_SCROLLDOWN";
        #endregion
        #region "ANIMATION"
        /// <summary>
        /// Triggered when an animation is done.
        /// </summary>
        public const string ANIM_FINISHED = "ANIM_FINISHED";
        #endregion
        #region "MOUSE INPUT STATUS"
        /// <summary>
        /// Triggered when the mouse enters a mouse input component.
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
        /// <summary>
        /// Triggered when the mouse moves inside a mouse input component, the data is the previous position.
        /// </summary>
        public const string MOUSEINPUT_MOVED = "MOUSEINPUT_MOVED";
        /// <summary>
        /// Triggered when the mouse wheel is scrolled up on the mouse input, the data is the scroll difference.
        /// </summary>
        public const string MOUSEINPUT_SCROLLUP = "MOUSEINPUT_SCROLLUP";
        /// <summary>
        /// Triggered when the mouse wheel is scrolled down on the mouse input, the data is the scroll difference.
        /// </summary>
        public const string MOUSEINPUT_SCROLLDOWN = "MOUSEINPUT_SCROLLDOWN";
        #endregion
        #region "Networking"
        /// <summary>
        /// Triggered when the client cannot find the server.
        /// </summary>
        public const string NETWORK_NOSERVER = "NETWORK_NOSERVER";
        /// <summary>
        /// Triggered when the client has successfully logged in to the server.
        /// </summary>
        public const string NETWORK_LOGGEDIN = "NETWORK_LOGGEDIN";
        /// <summary>
        /// Triggered when a message has been received from the server. 
        /// The sender is the SoulServer.MType of the message and the data is the message itself.
        /// </summary>
        public const string NETWORK_MESSAGE = "NETWORK_MESSAGE";
        #endregion
    }
}
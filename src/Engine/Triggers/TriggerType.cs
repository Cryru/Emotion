using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SoulEngine.Triggers
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The types of triggers used by SE.
    /// </summary>
    public static class TriggerType
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
        /// Triggered when the size of the game changes.
        /// </summary>
        public const string GAME_SIZECHANGED = "GAME_SIZECHANGED";
        #endregion
    }
}
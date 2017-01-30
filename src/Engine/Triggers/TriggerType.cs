using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//////////////////////////////////////////////////////////////////////////////
// SoulEngine - A game engine based on the MonoGame Framework.              //
// Public Repository: https://github.com/Cryru/SoulEngine                   //
//////////////////////////////////////////////////////////////////////////////
namespace SoulEngine.Triggers
{
    public static class TriggerType
    {
        /// <summary>
        /// Unknown type.
        /// </summary>
        public const string TRIGGER_GENERIC = "TRIGGER_GENERIC";
        /// <summary>
        /// Triggered when the game is closed. Will not be triggered if forcelly closed or in case of a crash.
        /// </summary>
        public const string TRIGGER_GAME_CLOSED = "TRIGGER_GAME_CLOSED";
        /// <summary>
        /// Triggered by a ticker each time it ticks. Data - The current tick's number.
        /// </summary>
        public const string TRIGGER_TICKER_TICK = "TRIGGER_TICKER_TICK";
        /// <summary>
        /// Triggered by a ticker when it has reached its tick limit.
        /// </summary>
        public const string TRIGGER_TICKER_DONE = "TRIGGER_TICKER_DONE";
    }
}
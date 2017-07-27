using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Modules
{
    class TimingManager : IModuleDrawable
    {
        // The list of running tickers.
        List<Ticker> Tickers;

        /// <summary>
        /// Ticker updating is in the draw rather than the update because we want it to run once per frame.
        /// </summary>
        public void Draw()
        {
            // If paused, don't update tickers.
            if (Context.Core.Paused) return;

            // Run through all registered tickers.
            for (int i = Tickers.Count - 1; i >= 0; i--)
            {
                // If the ticker is ready or disposed, remove it.
                if (Tickers[i] == null && Tickers[i].State == Enums.TickerState.Done)
                {
                    Tickers.RemoveAt(i);
                }
                else
                {
                    // Otherwise update it.
                    Tickers[i].Update();
                }
            }
        }

        public bool Initialize()
        {
            Tickers = new List<Ticker>();

            return true;
        }

        /// <summary>
        /// Registeres a new ticker.
        /// </summary>
        /// <param name="Ticker">The ticker object.</param>
        public void RegisterTicker(Ticker Ticker)
        {
            // Add the ticker to the list of running tickers.
            Tickers.Add(Ticker);
        }

        /// <summary>
        /// Removes a ticker from the active tickers list.
        /// </summary>
        /// <param name="Ticker">The ticker object.</param>
        public void RemoveTicker(Ticker Ticker)
        {
            Tickers.Remove(Ticker);
        }
    }
}

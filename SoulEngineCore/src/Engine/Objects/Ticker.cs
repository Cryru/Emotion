using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Enums;
using SoulEngine.Events;
using SoulEngine.Modules;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Used for timing events independent of FPS, but on real time.
    /// </summary>
    public class Ticker : IDisposable
    {
        #region "Declarations"
        #region "Settings"
        /// <summary>
        /// The delay between ticks.
        /// </summary>
        public float Delay;
        #endregion
        #region "Information"
        /// <summary>
        /// The number of times the ticker has ticked.
        /// </summary>
        public int Ticks
        {
            get
            {
                return _Ticks;
            }
        }
        /// <summary>
        /// The limit of how many times the ticker should tick.
        /// If -1 then the ticker will tick forever.
        /// </summary>
        public int Limit;
        /// <summary>
        /// Returns true if the current tick is the last.
        /// </summary>
        public bool isLastTick
        {
            get
            {
                return _Ticks == Limit && _State != TickerState.Done;
            }
        }
        /// <summary>
        /// The total time it would take for all ticks to run.
        /// </summary>
        public int TotalTime
        {
            get
            {
                if (Limit == -1) return -1;
                return Limit * (int)Delay;
            }
        }
        /// <summary>
        /// The time the ticker started.
        /// </summary>
        public DateTime TimeStarted
        {
            get
            {
                return _TimeStarted;
            }
        }
        /// <summary>
        /// The time that has passed since the ticker started in milliseconds.
        /// </summary>
        public float TimeSinceStart
        {
            get
            {
                return Ticks * Delay;
            }
        }
        #endregion
        #region "State"
        /// <summary>
        /// The state of the ticker.
        /// </summary>
        public TickerState State
        {
            get
            {
                return _State;
            }
        }
        /// <summary>
        /// The state of the ticker. Private holder.
        /// </summary>
        private TickerState _State = TickerState.None;
        #endregion
        #region "Private Workings"
        /// <summary>
        /// The variable that records time.
        /// </summary>
        private float time;
        /// <summary>
        /// The number of time the ticker has ticked. Private holder.
        /// </summary>
        private int _Ticks;
        /// <summary>
        /// The time the ticker started.
        /// </summary>
        private DateTime _TimeStarted;
        #endregion
        #region "Tags"
        /// <summary>
        /// Additional data attached to the ticker.
        /// </summary>
        public List<object> Tags = new List<object>();
        #endregion
        #endregion

        #region "Events"
        /// <summary>
        /// Triggered when the ticker ticks.
        /// </summary>
        public event EventHandler<EventArgs> OnTick;
        /// <summary>
        /// Triggered when ticker is done ticking.
        /// </summary>
        public event EventHandler<EventArgs> OnDone;
        /// <summary>
        /// Triggered when the ticker starts or is resumed.
        /// </summary>
        public event EventHandler<EventArgs> OnStart;
        /// <summary>
        /// Triggered when the ticker is paused.
        /// </summary>
        public event EventHandler<EventArgs> OnPause;
        #endregion

        /// <summary>
        /// Creates a ticker with the specified parameters.
        /// </summary>
        /// <param name="Delay">The delay between ticks.</param>
        /// <param name="Limit">The number of times the ticker should tick. If below 0 it will go on forever.</param>
        /// <param name="StartNow">Whether to start the ticker immediately or start paused.</param>
        public Ticker(float Delay = 100, int Limit = -1, bool StartNow = false)
        {
            this.Delay = Delay;
            this.Limit = Limit;

            //Record starting time.
            _TimeStarted = DateTime.Now;

            //Check if ticking should start.
            if (StartNow) Start(); else Pause();

            //Execute the ticker on each frame for accurate timing.
            if(Context.Core.isModuleLoaded<TimingManager>())
            {
                Context.Core.Module<TimingManager>().RegisterTicker(this);
            }
            else
            {
                Context.Core.Module<ErrorManager>().RaiseError("Using tickers requires the TimingManager module to be loaded.", 101);
            }
        }

        /// <summary>
        /// Is run every tick by the Core. Invokes triggers and calculates time passing.
        /// </summary>
        public void Update()
        {
            switch(_State) //Check which state we are in.
            {
                case TickerState.Paused: //If paused.
                case TickerState.Done: //If done.
                    break; //All three of this don't require any action.

                case TickerState.Running: //If running.

                    //Increment the time by the time that passed since the last frame.
                    time += Context.Core.frameTime;

                    //If enough time has passed then tick.
                    if (time > Delay)
                    {
                        OnTick?.Invoke(this, EventArgs.Empty);
                        time -= Delay;
                        _Ticks++;
                    }

                    //Check if at limit.
                    if (_Ticks == Limit && Limit > 0)
                    {
                        //Set the ticks to the limit, in case they are higher.
                        _Ticks = Limit;

                        //In which case run the ending event.
                        _State = TickerState.Done;
                        OnDone?.Invoke(this, EventArgs.Empty);
                        //Destroy the ticker.
                        Dispose();
                    }

                    break;
            }
        }

        #region "Operations"
        /// <summary>
        /// Pause the ticker.
        /// </summary>
        public void Pause()
        {
            if (State == TickerState.Running || State == TickerState.None)
            {
                _State = TickerState.Paused;
                OnPause?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// Start the ticker if paused.
        /// </summary>
        public void Start()
        {
            if (State == TickerState.Paused || State == TickerState.None)
            {
                _State = TickerState.Running;
                OnStart?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        //Other
        #region "Disposing"
        /// <summary>
        /// Disposing flag to detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //Remove events.
                OnTick = null;
                OnDone = null;

                //Set disposing flag.
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
};
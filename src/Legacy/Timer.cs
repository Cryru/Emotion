using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects.Internal;

namespace SoulEngine.Legacy.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// A timer for timing events independent of FPS, but on real time.
    /// Uses rendertime to determine time passing.
    /// </summary>
    public class Timer
    {
        #region "Declarations"
        #region "Settings"
        /// <summary>
        /// The delay between ticks.
        /// </summary>
        public float Delay;
        /// <summary>
        /// Whether the timer is part of the global timers.
        /// Global timers are timers that are executed by the engine every frame.
        /// </summary>
        public bool globalTimer;
        #endregion
        #region "Information"
        /// <summary>
        /// The number of times the timer has ticked.
        /// </summary>
        public int Ticks
        {
            get
            {
                return timerTicks;
            }
        }
        /// <summary>
        /// The limit of how many times the timer should tick.
        /// If -1 then the timer will tick forever.
        /// </summary>
        public int Limit;
        /// <summary>
        /// Returns a true if the current tick is the last.
        /// </summary>
        public bool isLastTick
        {
            get
            {
                return Ticks == Limit && _State != TimerState.Done;
            }
        }
        #endregion
        #region "Events"
        /// <summary>
        /// When the timer ticks.
        /// </summary>
        public SoulEngine.Legacy.Objects.Internal.Event<Timer> onTick = new SoulEngine.Legacy.Objects.Internal.Event<Timer>();
        /// <summary>
        /// When the timer's tick limit has been reached.
        /// This will never be triggered for endless timers.
        /// </summary>
        public SoulEngine.Legacy.Objects.Internal.Event<Timer> onTickLimitReached = new SoulEngine.Legacy.Objects.Internal.Event<Timer>();
        #endregion
        #region "State"
        /// <summary>
        /// The state of the timer.
        /// </summary>
        public TimerState State
        {
            get
            {
                return _State;
            }
        }
        /// <summary>
        /// The state of the timer. Private holder.
        /// </summary>
        protected TimerState _State = TimerState.None;
        /// <summary>
        /// The state of the timer is held here while the timer is paused.
        /// </summary>
        protected TimerState pausedStateHolder;
        /// <summary>
        /// The timer states.
        /// </summary>
        public enum TimerState  //The possible states.
        {
            /// <summary>
            /// Error, or not setup.
            /// </summary>
            None,
            /// <summary>
            /// The timer is paused.
            /// Call "Start" to start it.
            /// </summary>
            Paused,
            /// <summary>
            /// The timer is running.
            /// </summary>
            Running,
            /// <summary>
            /// The timer has reached its tick limit and is waiting to trigger the event.
            /// </summary>
            WaitingForEvent,
            /// <summary>
            /// The timer has reached its tick limit and ended operation.
            /// </summary>
            Done
        }
        #endregion
        #region "Private Workings"
        /// <summary>
        /// The variable that records time.
        /// </summary>
        private float timerValue;
        /// <summary>
        /// The number of time the number has ticked. Private holder.
        /// </summary>
        private int timerTicks;
        #endregion
        #endregion

        /// <summary>
        /// Creates a timer with the specified parameters.
        /// By default the timer is added to the global timer list paused, and must be started with a call to the "Start" method.
        /// </summary>
        /// <param name="Delay">The delay between ticks.</param>
        /// <param name="Limit">The number of times the timer should tick. If below 0 it will go on forever.</param>
        /// <param name="startNow">Whether the timer should start ticking from the moment it is initialized or after Resume is called.</param>
        /// <param name="addGlobal">True by default, if enabled the timer will be added to the list of global timers that are run every frame.</param>
        public Timer(float Delay = 100, int Limit = -1, bool startNow = false, bool addGlobal = true)
        {
            this.Delay = Delay;
            this.Limit = Limit;

            //Check if we should start now.
            if(startNow)
            {
                _State = TimerState.Running;
                pausedStateHolder = TimerState.Paused;
            }
            else
            {
                _State = TimerState.Paused;
                pausedStateHolder = TimerState.Running;
            }
           
            //If the timer should be added to the global running timers.
            if (addGlobal) Core.Timers.Add(this);
            globalTimer = addGlobal;
        }

        /// <summary>
        /// Is run every frame. Calculates the time until the text tick, invokes events and handles ticks.
        /// </summary>
        public void Run()
        {

            switch(_State) //Check which state we are in.
            {
                case TimerState.None: //If not setup.
                case TimerState.Paused: //If paused.
                case TimerState.Done: //If done.
                    break; //All three of this don't require any action.

                case TimerState.Running: //If running.

                    timerValue += Core.frametime; //Increment the timer's value by the delta time.

                    //If enough time has passed then tick.
                    if (timerValue > Delay)
                    {
                        timerValue -= Delay;
                        timerTicks++;
                        onTick.Trigger(this);
                    }

                    //Check if at limit.
                    if (timerTicks == Limit && Limit > 0)
                    {
                        //Set the ticks to the limit, in case they are higher.
                        timerTicks = Limit;

                        //In which case run the ending event.
                        _State = TimerState.WaitingForEvent;
                    }

                    break;

                case TimerState.WaitingForEvent: //If finishing up.

                    _State = TimerState.Done;
                    onTickLimitReached.Trigger(this);
                    break;
            }
        }

        #region "Operations"
        /// <summary>
        /// Pause the timer.
        /// </summary>
        public virtual void Pause()
        {
            if (_State != TimerState.Paused)
            {
                pausedStateHolder = _State;
                _State = TimerState.Paused;
            }
        }
        /// <summary>
        /// Start the timer if paused.
        /// </summary>
        public virtual void Start()
        {
            if (_State == TimerState.Paused)
            {
                _State = pausedStateHolder;
            }
        }
        /// <summary>
        /// Reset the timer and restart.
        /// Once reset the timer needs to be started again.
        /// </summary>
        public virtual void Reset()
        {
            //Check if in the global timers.
            if (Core.Timers.IndexOf(this) == -1 && globalTimer == true)
            {
                Core.Timers.Add(this);
            }

            timerTicks = 0;
            _State = TimerState.Paused;
            pausedStateHolder = TimerState.Running;
        }
        #endregion
    }
}

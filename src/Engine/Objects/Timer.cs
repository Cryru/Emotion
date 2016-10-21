using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects.Internal;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // A timer for timing events independent of FPS.                            //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Timer
    {
        #region "Declarations"
        //Settings
        public double Delay; //The time to wait.
        public int Ticks //The number of times the timer has ticked. A readonly value for the user.
        {
            get
            {
                return timerTicks;
            }
        }
        public int Limit; //The limit of how many times the timer should tick.
        public bool globalTimer; //Whether the timer is part of the global timers.

        //Information
        public bool isLastTick
        {
            get
            {
                return Ticks == Limit && _State != TimerState.Done;
            }
        } //Returns a true if the current tick is the last.

        //Events
        public Event<Timer> onTick = new Event<Timer>(); //The event for when a tick occurs.
        public Event<Timer> onTickLimitReached = new Event<Timer>(); //The event for when the maximum ticks has been reached.

        //State
        public TimerState State //The state of the timer, readonly for user reading.
        {
            get
            {
                return _State;
            }
        }
        protected TimerState _State = TimerState.None; //The state of the timer, private for editing.
        protected TimerState pausedStateHolder; //The state of the timer when paused.
        public enum TimerState  //The possible states.
        {
            None, //Not setup.
            Paused, //For when the timer is paused.
            Running, //For when the timer is running.
            WaitingForEvent, //When waiting to execute the onTickLimitReached event.
            Done //When the limit has been reached.
        }

        //Private Workings
        private double timerValue; //The variable that counts.
        private int timerTicks; //The number of times the timer has ticked, private variable for editing.
        #endregion

        /// <summary>
        /// Creates a timer with the specified parameters.
        /// By default the timer is added to the global timer list paused, and must be started with a call to the "Start" method.
        /// </summary>
        /// <param name="Delay">The delay between ticks.</param>
        /// <param name="Limit">The number of times the timer should tick. If below 0 it will go on forever.</param>
        /// <param name="addGlobal">True by default, if enabled the timer will be added to the list of global timers that are run every frame.</param>
        public Timer(double Delay = 100, int Limit = -1, bool addGlobal = true)
        {
            this.Delay = Delay;
            this.Limit = Limit;

            _State = TimerState.Paused;
            pausedStateHolder = TimerState.Running;

            //If the timer should be added to the global running timers.
            if (addGlobal) Core.Timers.Add(this);
            globalTimer = addGlobal;
        }

        //Is run every frame.
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

                    //Check if above limit.
                    if (timerTicks > Limit && Limit > 0)
                    {
                        //Set the ticks to the limit, in case they are higher.
                        timerTicks = Limit;

                        //In which case run the ending event.
                        _State = TimerState.WaitingForEvent;
                    }

                    break;

                case TimerState.WaitingForEvent: //If finishing up.

                    onTickLimitReached.Trigger(this);
                    _State = TimerState.Done;
                    break;
            }
        }
        //Pause if running.
        public virtual void Pause()
        {
            if (_State != TimerState.Paused)
            {
                pausedStateHolder = _State;
                _State = TimerState.Paused;
            }
        }
        //Start if pausing.
        public virtual void Start()
        {
            if(_State == TimerState.Paused)
            {
                _State = pausedStateHolder;
            }
        }
        //Resets the timer.
        public virtual void Reset()
        {
            //Check if in the global timers.
            if(Core.Timers.IndexOf(this) == -1 && globalTimer == true)
            {
                Core.Timers.Add(this);
            }

            timerTicks = 0;
            _State = TimerState.Paused;
            pausedStateHolder = TimerState.None;
        }
    }
}

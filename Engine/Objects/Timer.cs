using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //Information
        public bool isLastTick
        {
            get
            {
                return Ticks == Limit && _State != TimerState.Done;
            }
        } //Returns a true if the current tick is the last.

        //Events
        public Action onTick; //The event for when a tick occurs.
        public Action onTickLimitReached; //The event for when the maximum ticks has been reached.

        //State
        public TimerState State //The state of the timer, readonly for user reading.
        {
            get
            {
                return _State;
            }
        }
        private TimerState _State = TimerState.None; //The state of the timer, private for editing.
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

        //The constructor.
        public Timer(double _Delay = 100, int _Limit = -1)
        {
            Delay = _Delay;
            Limit = _Limit;

            _State = TimerState.Paused;
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
                        onTick?.Invoke();
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

                    onTickLimitReached?.Invoke();
                    _State = TimerState.Done;
                    break;
            }
        }

        //Pause if running.
        public void Pause()
        {
            if (_State == TimerState.Running)
            {
                _State = TimerState.Paused;
            }
        }
        //Start if pausing.
        public void Start()
        {
            if(_State == TimerState.Paused)
            {
                _State = TimerState.Running;
            }
        }
        //Resets the timer.
        public void Reset()
        {
            timerTicks = 0;
            _State = TimerState.Paused;
        }
    }
}

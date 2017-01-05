using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Objects;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // An object used to wait and stall.                                        //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Wait
    {
        #region "Declarations"
        private Timer WaitTimer; //The timer for fading.

        public bool Ready = false; //Whether the timer is ready.

        public Action onReady; //The function to call after ready.
        #endregion

        //The initializer.
        public Wait(int timeMilliseconds, Action onReady = null)
        {
            //Specify the on ready function.
            this.onReady = onReady;

            //Setup the timer.
            WaitTimer = new Timer(timeMilliseconds, 1); //Setup the fade.
            WaitTimer.onTickLimitReached.Add(TimerDone);
        }
        
        //Is run when the timer ends.
        private void TimerDone()
        {
            Ready = true;
            onReady?.Invoke();
        }

        #region "Timer Interfacing"
        //Pauses the waiting timer.
        public void Pause()
        {
            WaitTimer.Pause();
        }

        //Resumes the waiting timer.
        public void Resume()
        {
            WaitTimer.Start();
        }
        #endregion

    }
}

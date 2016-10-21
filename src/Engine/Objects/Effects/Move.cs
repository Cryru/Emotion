using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    // An object used to move objects in delta time.                            //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Move
    {
        #region "Declarations"
        //Internals
        private Timer MoveTimer; //The timer for fading.
        private ObjectBase objectTarget; //The target to scale.
        private float XIncrement = 0; //How much to increment the width by per tick.
        private float YIncrement = 0; //How much to increment the height by per tick.

        //Reporters
        public bool Ready = false; //Whether the timer is ready.
        
        //Events
        public Action onReady; //The function to call after ready.

        //Settings
        private Vector2 targetLocation; //The scale to reach.
        private bool centerScale = true; //Whether to scale from the center out.
        #endregion

        //The initializer.
        public Move(int timeMilliseconds, ObjectBase objectTarget, Vector2 targetLocation, Action onReady = null, int smoothness = 50)
        {
            //Assign settings.
            this.targetLocation = targetLocation;
            this.objectTarget = objectTarget;

            //Specify the on ready function.
            this.onReady = onReady;

            //Calculate the increments.
            XIncrement = (targetLocation.X - objectTarget.X) / smoothness;
            YIncrement = (targetLocation.Y - objectTarget.Y) / smoothness;

            //Setup the timer.
            MoveTimer = new Timer(timeMilliseconds / smoothness, smoothness); //Setup the timer.
            MoveTimer.onTick.Add(MoveTick);
            MoveTimer.onTickLimitReached.Add(TimerDone);
        }
        
        //Is run every frame.
        private void MoveTick()
        {
            objectTarget.X += XIncrement;
            objectTarget.Y += YIncrement;

            //Check if last tick.
            if(MoveTimer.isLastTick)
            {
                //Ensure right location
                objectTarget.X = targetLocation.X;
                objectTarget.Y = targetLocation.Y;
            }
        }

        //Used to end the effect prematurely.
        public void End(bool applyChanges = true, bool invokeReady = false)
        {
            if(applyChanges == true)
            {
                objectTarget.X = targetLocation.X;
                objectTarget.Y = targetLocation.Y;
            }
            if(invokeReady == true)
            {
                TimerDone();
            }
            MoveTimer.Pause();
            MoveTimer = null;
        }

        //Is run when the timer ends.
        private void TimerDone()
        {
            Ready = true;
            onReady?.Invoke();
        }

        #region "Timer Interfacing"
        //Pauses the fader.
        public void Pause()
        {
            MoveTimer.Pause();
        }

        //Resumes the fader.
        public void Resume()
        {
            MoveTimer.Start();
        }
        #endregion
    }
}

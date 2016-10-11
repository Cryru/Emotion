using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev, MonoGame                                //
    //                                                                          //
    // An object used to scale objects in delta time.                           //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Scale
    {
        #region "Declarations"
        //Internals
        private Timer ScaleTimer; //The timer for fading.
        private ObjectBase objectTarget; //The target to scale.
        private float widthIncrement = 0; //How much to increment the width by per tick.
        private float heightIncrment = 0; //How much to increment the height by per tick.
        private Vector2 objectCenter; //The center of the object.

        //Reporters
        public bool Ready = false; //Whether the timer is ready.
        
        //Events
        public Action onReady; //The function to call after ready.

        //Settings
        private Vector2 targetScale; //The scale to reach.
        private bool centerScale = true; //Whether to scale from the center out.
        #endregion

        //The initializer.
        public Scale(int timeMilliseconds, ObjectBase objectTarget, Vector2 targetScale, Action onReady = null, bool centerScale = true, int smoothness = 50)
        {
            //Assign settings.
            this.targetScale = targetScale;
            this.centerScale = centerScale;
            this.objectTarget = objectTarget;

            //Specify the on ready function.
            this.onReady = onReady;

            //Calculate the increments.
            widthIncrement = (targetScale.X - objectTarget.Width) / smoothness;
            heightIncrment = (targetScale.Y - objectTarget.Height) / smoothness;

            //Determine direction of scale.
            if(widthIncrement < 0)
            {
                //Scaling in.
                objectCenter = objectTarget.Center; //Get the center of the object when full size.
            }
            else
            {
                //Scaling out.
                objectCenter = new Vector2(objectTarget.X + targetScale.X / 2, objectTarget.Y + targetScale.Y / 2); //Get the center of the object when full size.
            }   

            //Setup the timer.
            ScaleTimer = new Timer(timeMilliseconds / smoothness, smoothness); //Setup the timer.
            ScaleTimer.onTick = ScaleTick;
            ScaleTimer.onTickLimitReached = TimerDone;
            ScaleTimer.Start();

            //Attach the timer to the global timer runs.
            Core.Timers.Add(ScaleTimer);
        }
        
        //Is run every frame.
        private void ScaleTick()
        {
            objectTarget.Width += (int) Math.Round(widthIncrement);
            objectTarget.Height += (int) Math.Round(heightIncrment);

            if(centerScale == true)
            {
                objectTarget.Center = objectCenter;
            }

            //Check if last tick.
            if(ScaleTimer.isLastTick)
            {
                //Ensure right proportions.
                //This is done because of the float innacuracy can sometimes blow past the target scale, or not scale it enough.
                objectTarget.Width = (int)targetScale.X;
                objectTarget.Height = (int)targetScale.Y;

                if (centerScale == true)
                {
                    objectTarget.Center = objectCenter;
                }
            }
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
            ScaleTimer.Pause();
        }

        //Resumes the fader.
        public void Resume()
        {
            ScaleTimer.Start();
        }
        #endregion
    }
}

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
    // An object for fading in and out.                                         //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Fade
    {
        #region "Declarations"

        //Internals
        Timer FadeTimer; //The timer for fading.

        //Settings
        public FadeType Type
        {
            get
            {
                return _Type;
            }
        }//The type of fade.
        public enum FadeType
        {
            In,
            Out
        }

        //Data
        public bool Ready = false; //Whether the timer is ready.

        //Events
        public Action onReady; //The event to call when the fade is complete.

        //Private accessors.
        private FadeType _Type;
        private ObjectBase fadingObject; //The object to fade.
        #endregion

        //The initializer.
        public Fade(int timeMilliseconds, FadeType Type, Color Color = new Color(), ObjectBase _fadingObject = null, Action onReady = null)
        {
            this.onReady = onReady; //Assign the event.

            _Type = Type; //Set the fade type.

            FadeTimer = new Timer(timeMilliseconds / 100, 100); //Setup the timer.
            FadeTimer.onTick = TimerFadeTick;
            FadeTimer.onTickLimitReached = TimerFadeDone;
            FadeTimer.Start();

            if(_fadingObject == null)
            {
                fadingObject = new ObjectBase(new Texture(Core.blankTexture));
                Core.ObjectFullscreen(fadingObject);
            }
            else
            {
                fadingObject = _fadingObject;
            }

            //Set the color, and if it's empty then default to black.
            if(Color != new Color())
            {
                fadingObject.Color = Color;
            }

            //Set the opacity based on the fade type.
            if (Type == FadeType.In)
            {
                fadingObject.Opacity = 0f;
            }
            if(Type == FadeType.Out)
            {
                fadingObject.Opacity = 1f;
            }

            //Hook up to the object.
            Core.Timers.Add(FadeTimer);
        }
      
        //Is run when the timer ticks.
        private void TimerFadeTick()
        {
            if(Type == FadeType.In)
            {
                fadingObject.Opacity += 0.01f;
            }
            if(Type == FadeType.Out)
            {
                fadingObject.Opacity -= 0.01f;
            }
        }
        
        //Is run when the timer ends.
        private void TimerFadeDone()
        {
            Ready = true;
            onReady?.Invoke();
        }

        //The fader can also draw the fading object, this is used when using the default object.
        public void Draw()
        {
            fadingObject.Draw();
        }

        #region "Timer Interfacing"
        //Pauses the fader.
        public void Pause()
        {
            FadeTimer.Pause();
        }

        //Resumes the fader.
        public void Resume()
        {
            FadeTimer.Start();
        }
        #endregion

    }
}

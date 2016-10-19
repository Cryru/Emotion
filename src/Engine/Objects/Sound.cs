using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;

namespace SoulEngine.Objects
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // A sound object. Uses the SoundEffect importer and OpenAL.                //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Sound
    {
        #region "Declarations"
        //Properties
        public string Name
        {
            get
            {
                return _name;
            }
        }//The name of the file.
        public SoundEffect File
        {
            get
            {
                return _file;
            }
        }//The loaded content file.

        //Settings
        public float Pan = 0f; //The pan.
        public float Pitch = 0f; //The pitch
        public float Volume = 0f; //The volume.
        public bool Loop = false; //Whether to loop.

        //Privates
        private string _name = "";
        private SoundEffect _file;

        //Internals
        private bool errored = false;
        private List<SoundEffectInstance> soundInstances = new List<SoundEffectInstance>();

        //Fading
        private Timer fadeTimer;
        #endregion

        //Initializer
        public Sound(string soundName, float volume = 1f, bool loop = false)
        {
            //Check if the sound exists before loading it.
            if(Core.GetContentExist(soundName))
            {
                _file = Core.host.Content.Load<SoundEffect>(soundName);
            }
            else
            {
                //If not set the sound to errored.
                errored = true;
            }

            Volume = volume;
            Loop = loop;

        }

        //Play the sound, or start the loop.
        public void Play()
        {
            //Check if an error.
            if (errored) return;

            //Offset the volume if sound is off.
            float volumeOffset = Volume;
            if (Settings.sound == false) volumeOffset = 0;

            //Play the file at the specified pitch and pan.
            SoundEffectInstance newInst = File.CreateInstance();
            newInst.Pan = Pan;
            newInst.Pitch = Pitch;
            newInst.Volume = volumeOffset;
            newInst.IsLooped = Loop;

            //Check for instance limit.
            if(soundInstances.Count == 20)
            {
                soundInstances.RemoveAt(0);
            }
            //Add the instance to list.
            soundInstances.Add(newInst);
            newInst.Play();
        }
        //Stops all instances and resets the list.
        public void Stop()
        {
            for (int i = 0; i < soundInstances.Count; i++)
            {
                soundInstances[i].Stop();
            }
            soundInstances.Clear();
        }
        //Pause all instances.
        public void Pause()
        {
            for (int i = 0; i < soundInstances.Count; i++)
            {
                soundInstances[i].Pause();
            }
        }

        #region "Fading"
        public void FadeOut(int timeMilliseconds, Action onFadeDone = null)
        {
            //Set up the timer.
            fadeTimer = new Timer(timeMilliseconds / 100, 100);
            fadeTimer.Start();
            fadeTimer.onTick = fadeTick_Out;
            fadeTimer.onTickLimitReached = onFadeDone;

            //Hook the timer to the Core.
            Core.Timers.Add(fadeTimer);
        }
        public void FadeIn(int timeMilliseconds, Action onFadeDone = null)
        {
            //Set to volume to 0.
            Volume = 0;

            //Set up the timer.
            fadeTimer = new Timer(timeMilliseconds / 100, 100);
            fadeTimer.Start();
            fadeTimer.onTick = fadeTick_In;
            fadeTimer.onTickLimitReached = onFadeDone;

            //Hook the timer to the Core.
            Core.Timers.Add(fadeTimer);
        }

        private void fadeTick_Out()
        {
            //Decrease volume.
            if (Volume - 0.01f > 0)
                Volume -= 0.01f;
            else
                Volume = 0;

            //Apply the volume to the instances of sound.
            float volumeOffset = Volume;

            if (Settings.sound == false) volumeOffset = 0; //If sound is off set the volume to zero.

            for (int i = 0; i < soundInstances.Count; i++) //Apply to all instances.
            {
                soundInstances[i].Volume = volumeOffset;
            }
        }
        private void fadeTick_In()
        {
            //Decrease volume.
            if (Volume + 0.01f < 1)
                Volume += 0.01f;
            else
                Volume = 1;

            //Apply the volume to the instances of sound.
            float volumeOffset = Volume;

            if (Settings.sound == false) volumeOffset = 0; //If sound is off set the volume to zero.

            for (int i = 0; i < soundInstances.Count; i++) //Apply to all instances.
            {
                soundInstances[i].Volume = volumeOffset;
            }
        }
        #endregion

    }
}

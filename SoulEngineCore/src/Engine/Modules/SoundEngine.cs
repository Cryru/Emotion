using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Modules
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An animation component. Requires an ActiveTexture to be drawn.
    /// </summary>
    public class SoundEngine : IModuleUpdatable
    {
        #region "Declarations"
        private static Dictionary<SoundEffectInstance, string> SoundsPlaying = new Dictionary<SoundEffectInstance, string>();
        #endregion

        public bool Initialize()
        {
            return true;
        }

        /// <summary>
        /// Stops all sound on a particular channel.
        /// </summary>
        /// <param name="Channel"></param>
        public static void StopSound(string Channel, bool Fadeout = false)
        {
            SoundEffectInstance[] channelEffects = SoundsPlaying.Where(x => x.Value == Channel).Select(x => x.Key).ToArray();

            if (Fadeout)
            {
                Ticker fadeOut = new Ticker(10, 100, true);
                fadeOut.Tags.Add(channelEffects);
                fadeOut.OnTick += FadeOut_OnTick;
                fadeOut.OnDone += ShutdownChannel;
            }
            else
            {
                for (int i = 0; i < channelEffects.Length; i++)
                {
                    channelEffects[i].Stop();
                    SoundsPlaying.Remove(channelEffects[i]);
                }
            }
        }

        /// <summary>
        /// Plays a sound.
        /// </summary>
        /// <param name="Channel">The channel to play on.</param>
        /// <param name="Sound">The sound to play.</param>
        /// <param name="Volume">The volume to play at.</param>
        /// <param name="Pitch">The pitch of the sound.</param>
        /// <param name="Pan">The pan of the sound.</param>
        /// <param name="Loop">Whether to loop the sound.</param>
        public static void PlaySound(string Channel, SoundEffect Sound, float Volume, float Pitch = 0f, float Pan = 0f, bool Loop = false, bool Fadein = false)
        {
            if (Sound == null) return;

            SoundEffectInstance newInst = Sound.CreateInstance();
            newInst.Pan = Pan;
            newInst.Pitch = Pitch;
            newInst.Volume = Volume;
            newInst.IsLooped = Loop;

            if(Loop == true) SoundsPlaying.Add(newInst, Channel);
            newInst.Play();

            if(Fadein)
            {
                newInst.Volume = 0;
                Ticker fadeIn = new Ticker(10, 100, true);
                fadeIn.Tags.Add(newInst);
                fadeIn.Tags.Add(Volume);
                fadeIn.OnTick += FadeIn_OnTick;
            }
        }

        /// <summary>
        /// Updates the volume every frame.
        /// </summary>
        public void Update()
        {
            //Clamp within range.
            Settings.Volume = MathHelper.Clamp(Settings.Volume, 0, 100);

            if (!Settings.Sound) SoundEffect.MasterVolume = 0f;
            else SoundEffect.MasterVolume = (Settings.Volume / 100f);
        }

        #region "Effect Handlers"
        /// <summary>
        /// Handles the fade in ticker event.
        /// </summary>
        private static void FadeIn_OnTick(object sender, EventArgs e)
        {
            Ticker a = (Ticker)sender;
            SoundEffectInstance inst = (SoundEffectInstance)a.Tags[0];
            float targetVolume = (float)a.Tags[1];

            inst.Volume = Math.Min(1, inst.Volume + targetVolume / 100);
        }

        /// <summary>
        /// Handles the fade out ticker event.
        /// </summary>
        private static void FadeOut_OnTick(object sender, EventArgs e)
        {
            Ticker a = (Ticker)sender;
            SoundEffectInstance[] channel = (SoundEffectInstance[]) a.Tags[0];

            for (int i = 0; i < channel.Length; i++)
            {
                channel[i].Volume = Math.Max(0, channel[i].Volume - 0.01f);
            }
        }

        /// <summary>
        /// Handles fade out finish.
        /// </summary>
        private static void ShutdownChannel(object sender, EventArgs e)
        {
            Ticker a = (Ticker)sender;
            SoundEffectInstance[] channel = (SoundEffectInstance[])a.Tags[0];

            for (int i = 0; i < channel.Length; i++)
            {
                channel[i].Stop();
                SoundsPlaying.Remove(channel[i]);
            }
        }
        #endregion
    }
}

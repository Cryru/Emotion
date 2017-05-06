using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An animation component. Requires an ActiveTexture to be drawn.
    /// </summary>
    public static class SoundEngine
    {
        #region "Declarations"
        private static Dictionary<SoundEffectInstance, string> SoundsPlaying = new Dictionary<SoundEffectInstance, string>();
        #endregion

        /// <summary>
        /// Stops all sound on a particular channel.
        /// </summary>
        /// <param name="Channel"></param>
        public static void StopSound(string Channel)
        {
            SoundEffectInstance[] channelEffects = SoundsPlaying.Where(x => x.Value == Channel).Select(x => x.Key).ToArray();
            for (int i = 0; i < channelEffects.Length; i++)
            {
                channelEffects[i].Stop();
                SoundsPlaying.Remove(channelEffects[i]);
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
        public static void PlaySound(string Channel, SoundEffect Sound, float Volume, float Pitch = 0f, float Pan = 0f, bool Loop = false)
        {
            if (Sound == null) return;

            SoundEffectInstance newInst = Sound.CreateInstance();
            newInst.Pan = Pan;
            newInst.Pitch = Pitch;
            newInst.Volume = Volume;
            newInst.IsLooped = Loop;

            if(Loop == true) SoundsPlaying.Add(newInst, Channel);
            newInst.Play();
        }

        public static void Update()
        {
            if (!Settings.Sound) SoundEffect.MasterVolume = 0f;
            else SoundEffect.MasterVolume = Settings.Volume;

        }
    }
}

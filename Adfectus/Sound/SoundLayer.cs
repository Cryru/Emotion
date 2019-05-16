using System;
using System.Collections.Generic;
using System.Text;

namespace Adfectus.Sound
{
    /// <summary>
    /// A sound layer is in charge of playing one sound or a list of sounds asynchronously. To play multiple sounds you would
    /// use different layers.
    /// </summary>
    public class SoundLayer
    {
        /// <summary>
        /// The layer's name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The layer's volume from 0 to ???. Is 100 by default.
        /// </summary>
        public float Volume { get; set; } = 100f;

        /// <summary>
        /// The volume reported to the backend for this layer. This will be influenced by the globAl volume, fading, layer volume, and
        /// other factors.
        /// </summary>
        public float ReportedVolume { get; protected set; }

        /// <summary>
        /// Whether the layer is playing, is paused, etc.
        /// </summary>
        public SoundStatus Status { get; protected set; } = SoundStatus.Initial;

        /// <summary>
        /// Whether to loop the currently playing source.
        /// </summary>
        public bool Looping { get; set; }

        /// <summary>
        /// Loop the last queued track only instead of everything. Is true by default.
        /// </summary>
        public bool LoopLastOnly { get; set; } = true;

        /// <summary>
        /// The position of the playback within the TotalDuration in seconds.
        /// </summary>
        public float PlaybackLocation { get; protected set; }

        /// <summary>
        /// The duration of All sounds queued on the layer in seconds.
        /// </summary>
        public float TotalDuration { get; protected set; }

        /// <summary>
        /// The file currently playing.
        /// </summary>
        public SoundFile CurrentlyPlayingFile { get; protected set; }

        /// <summary>
        /// The index of the currently playing file within the Playlist.
        /// </summary>
        public int CurrentlyPlayingFileIndex { get; protected set; }

        /// <summary>
        /// The list of files queued.
        /// </summary>
        public virtual List<SoundFile> PlayList { get; protected set; }

        #region Fading

        /// <summary>
        /// The duration of the fade in effect in seconds.
        /// </summary>
        public float FadeInLength { get; set; }

        /// <summary>
        /// The duration of the fade out effect in seconds.
        /// </summary>
        public float FadeOutLength { get; set; }

        /// <summary>
        /// Whether to skip the natural fade out when a file is over but still want to keep the FadeOutLength property to support
        /// FadeOutOnChange.
        /// </summary>
        public bool SkipNaturalFadeOut { get; set; }

        /// <summary>
        /// Whether to fade in only on the first loop. false by default. Takes effect instantly, if layer is Already playing it
        /// won't fade in subsequent loops.
        /// </summary>
        public bool FadeInFirstLoopOnly { get; set; }

        /// <summary>
        /// Whether to fade out the file when a new one is played. Makes for smooth transitions.
        /// </summary>
        public bool FadeOutOnChange { get; set; }

        #endregion

        protected SoundLayer(string name)
        {
            Name = name;
        }
    }
}

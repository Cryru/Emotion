#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.Common.Threading;
using Adfectus.Logging;
using Adfectus.OpenAL;

#endregion

namespace Adfectus.Sound
{
    /// <summary>
    /// Manages audio and interop with OpenAl.
    /// </summary>
    public sealed class SoundManager
    {
        /// <summary>
        /// The volume of sounds.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Whether sound is enabled.
        /// </summary>
        public bool Sound { get; set; }

        /// <summary>
        /// List of active sound layers.
        /// </summary>
        public string[] Layers
        {
            get => _layers.Keys.ToArray();
        }

        private ConcurrentDictionary<string, SoundLayer> _layers;
        private IntPtr _context;
        private IntPtr _device;
        private bool _loopRunning;
        private Task _soundThread;
        private ConcurrentQueue<SoundFile> _buffersToDestroy;
        private AwAction _oneLoopToken;

        /// <summary>
        /// Create a new sound manager.
        /// </summary>
        /// <param name="initialSound">The initial sound enable.</param>
        /// <param name="initialVolume">The initial volume.</param>
        internal SoundManager(bool initialSound = false, float initialVolume = 100)
        {
            Volume = initialVolume;
            Sound = initialSound;

            _layers = new ConcurrentDictionary<string, SoundLayer>();
            _buffersToDestroy = new ConcurrentQueue<SoundFile>();

            // Load the native library functions.
            Al.Init(Bootstrapper.LoadedLibraries["openal"]);
            Alc.Init(Bootstrapper.LoadedLibraries["openal"]);
        }

        /// <summary>
        /// Start running the SoundManager's loop.
        /// </summary>
        public void Run()
        {
            _soundThread = new Task(SoundThreadLoop, TaskCreationOptions.LongRunning);
            _soundThread.Start();

            Engine.Log.Trace("Sound thread started.", MessageSource.SoundManager);
        }

        private void SoundThreadLoop()
        {
            try
            {
                // Create audio context.
                _device = Alc.OpenDevice(null);
                string deviceName = Alc.GetString(_device, Alc.DeviceSpecifier);
                Engine.Log.Info($"Bound audio device on [{deviceName}].", MessageSource.SoundManager);

                int[] attr = new int[0];
                _context = Alc.CreateContext(_device, attr);
                if (_context == IntPtr.Zero) Engine.Log.Error("Couldn't create OpenAL context.", MessageSource.SoundManager);
                bool success = Alc.MakeContextCurrent(_context);
                if (!success) Engine.Log.Error("Couldn't make OpenAL context current.", MessageSource.SoundManager);
                Engine.Log.Info("Created audio context.", MessageSource.SoundManager);

                // Bind thread manager.
                ALThread.BindThread();
                Engine.Log.Trace("Bound AL thread.", MessageSource.SoundManager);

                // Setup listener.
                Al.Listener3f(Al.Position, 0, 0, 0);
                Al.Listener3f(Al.Velocity, 0, 0, 0);

                _loopRunning = true;
                while (_loopRunning)
                {
                    // Update running layers.
                    foreach (KeyValuePair<string, SoundLayer> layer in _layers)
                    {
                        layer.Value.Update();
                    }

                    // Run ALThread and cleanup only if focused and the engine is running.
                    if (!Engine.IsUnfocused && Engine.IsRunning)
                    {
                        // Run queued actions.
                        ALThread.Run();

                        // Check for errors.
                        ALThread.CheckError("loop end");

                        // Check if any of the buffers waiting to be disposed are ready to be.
                        // Maximum of one buffer will be cleaned per loop.
                        bool took = _buffersToDestroy.TryDequeue(out SoundFile soundFile);
                        if (took)
                            foreach (KeyValuePair<string, SoundLayer> layer in _layers)
                            {
                                // If it is within the playlist, it is in use.
                                SoundFile foundFile = layer.Value.PlayList.FirstOrDefault(x => x.ALBuffer == soundFile.ALBuffer);
                                if (foundFile == null)
                                {
                                    // Check if already destroyed.
                                    if (soundFile.ALBuffer == 0) continue;

                                    // If not in use, delete it.
                                    Al.DeleteBuffer(soundFile.ALBuffer);

                                    try
                                    {
                                        ALThread.CheckError($"destroying buffer {soundFile.ALBuffer} - {soundFile.Name}");
                                        soundFile.ALBuffer = 0;
                                    }
                                    catch (Exception)
                                    {
                                        // Wasn't destroyed due to unknown reasons. Add back.
                                        _buffersToDestroy.Enqueue(soundFile);
                                    }
                                }
                                else
                                {
                                    // Add back if still in use.
                                    _buffersToDestroy.Enqueue(soundFile);
                                }
                            }
                    }

                    // Run the one loop token if any.
                    _oneLoopToken?.Run();
                    _oneLoopToken = null;

                    // Update in interval specified in flag.
                    Task.Delay(Engine.Flags.SoundThreadFrequency).Wait();
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ThreadAbortException)) ErrorHandler.SubmitError(new Exception("Error in OpenAL loop.", ex));
            }

            // Cleanup.
            Alc.MakeContextCurrent(IntPtr.Zero);
            Alc.DestroyContext(_context);
            Alc.CloseDevice(_device);
        }

        #region API

        /// <summary>
        /// Plays the specified file on the specified layer.
        /// If the layer doesn't exist it is created.
        /// If sometimes else is playing on that layer it will be forcefully stopped.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <param name="layer">The layer to play on.</param>
        /// <returns>The layer the sound is playing on.</returns>
        [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
        public AwAction Play(SoundFile file, string layer)
        {
            // Check whether the layer exists, and create it if it doesn't.
            SoundLayer playBackLayer = GetLayer(layer) ?? CreateLayer(layer);

            return playBackLayer.Play(file);
        }

        /// <summary>
        /// Plays the specified file on the specified layer.
        /// If the layer doesn't exist it is created.
        /// If sometimes else is playing on that layer this file will be appended to it.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <param name="layer">The layer to play on.</param>
        /// <returns>The layer the sound is playing on.</returns>
        [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
        public AwAction QueuePlay(SoundFile file, string layer)
        {
            // Check whether the layer exists, and create it if it doesn't.
            SoundLayer playBackLayer = GetLayer(layer) ?? CreateLayer(layer);

            return playBackLayer.QueuePlay(file);
        }

        /// <summary>
        /// Returns the sound layer object of the requested name or create one if it doesn't.
        /// </summary>
        /// <param name="layer">The name of the layer to look for.</param>
        /// <returns>The layer with the specified name.</returns>
        public SoundLayer GetLayer(string layer)
        {
            _layers.TryGetValue(layer, out SoundLayer playBackLayer);
            return playBackLayer ?? CreateLayer(layer);
        }

        /// <summary>
        /// Remove a layer by its name.
        /// </summary>
        /// <param name="layer">The name of the layer to remove.</param>
        public void RemoveLayer(string layer)
        {
            _layers.TryRemove(layer, out SoundLayer playBackLayer);
            playBackLayer?.Dispose();
        }

        #endregion

        #region Buffer API

        /// <summary>
        /// Create a sound buffer.
        /// </summary>
        /// <param name="channels">The channels of the sound.</param>
        /// <param name="bitsPerSample">The number of bits per sample.</param>
        /// <param name="soundData">The sound itself as a byte array.</param>
        /// <param name="sampleRate">The rate of samples</param>
        /// <returns>A pointer to the internal buffer.</returns>
        internal unsafe uint CreateBuffer(int channels, int bitsPerSample, byte[] soundData, int sampleRate)
        {
            uint pointer = 0;

            ALThread.ExecuteALThread(() =>
            {
                Al.GenBuffer(out pointer);

                fixed (byte* dataBuffer = &soundData[0])
                {
                    Al.BufferData(pointer, GetSoundFormat(channels, bitsPerSample), dataBuffer, soundData.Length, sampleRate);
                }
            }).Wait();

            return pointer;
        }

        /// <summary>
        /// Add a buffer to be destroyed when it is no longer played.
        /// </summary>
        /// <param name="file">Pointer to the sound file holding the buffer to destroy.</param>
        internal void DestroyBuffer(SoundFile file)
        {
            _buffersToDestroy.Enqueue(file);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns a token which allows you to wait for a single loop of the sound thread.
        /// </summary>
        /// <returns>A task token for chaining tasks to sound thread loops.</returns>
        public AwAction GetOneLoopToken()
        {
            if (_oneLoopToken != null) return _oneLoopToken;

            _oneLoopToken = new AwAction();
            return _oneLoopToken;
        }

        /// <summary>
        /// Returns the type of sound format based on channels and bits.
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        /// <param name="bits">The number of bits.</param>
        /// <returns>
        /// The OpenAL sound format depending ont he channels and bits. Supported formats are 8bit mono and stereo, and 16
        /// bit mono and stereo.
        /// </returns>
        private static int GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? Al.FormatMono8 : Al.FormatMono16;
                case 2: return bits == 8 ? Al.FormatStereo8 : Al.FormatStereo16;
                default: throw new Exception("Unknown format.");
            }
        }

        /// <summary>
        /// Creates the sound layer.
        /// </summary>
        /// <param name="layer">The name of the layer to create.</param>
        /// <returns>The created layer.</returns>
        private SoundLayer CreateLayer(string layer)
        {
            SoundLayer playBackLayer = new SoundLayer(layer);
            bool added = _layers.TryAdd(layer, playBackLayer);
            if (!added) Engine.Log.Error($"Couldn't add sound layer {layer}", MessageSource.SoundManager);
            return playBackLayer;
        }

        #endregion

        /// <summary>
        /// Destroy the audio context.
        /// </summary>
        public void Dispose()
        {
            if (!_loopRunning) return;

            _loopRunning = false;
            while (!_soundThread.IsCompleted)
            {
            }

            _soundThread.Dispose();
        }
    }
}
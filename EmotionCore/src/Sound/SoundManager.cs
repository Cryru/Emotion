// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    /// <summary>
    /// Manages audio and interop with OpenAL.
    /// </summary>
    public sealed class SoundManager
    {
        /// <summary>
        /// List of active sound layers.
        /// </summary>
        public string[] Layers
        {
            get => _layers.Keys.ToArray();
        }

        private ConcurrentDictionary<string, SoundLayer> _layers;
        private ContextHandle _context;
        private IntPtr _device;
        private bool _loopRunning;
        private Task _soundThread;
        private ConcurrentBag<SoundFile> _buffersToDestroy;
        private Task _oneLoopToken;

        /// <summary>
        /// Create a new sound manager.
        /// </summary>
        internal SoundManager()
        {
            _layers = new ConcurrentDictionary<string, SoundLayer>();
            _buffersToDestroy = new ConcurrentBag<SoundFile>();

            _soundThread = new Task(SoundThreadLoop, TaskCreationOptions.LongRunning);
            _soundThread.Start();
        }

        private void SoundThreadLoop()
        {
            try
            {
                // Create audio context.
                Context.Log.Info($"Creating audio device on [{AudioContext.DefaultDevice}].", MessageSource.SoundManager);
                _device = Alc.OpenDevice(null);
                int[] attr = new int[0];
                _context = Alc.CreateContext(_device, attr);
                if (_context.Handle == IntPtr.Zero) Context.Log.Error("Couldn't create OpenAL context.", MessageSource.SoundManager);

                bool success = Alc.MakeContextCurrent(_context);
                if (!success) Context.Log.Error("Couldn't make OpenAL context current.", MessageSource.SoundManager);
                Context.Log.Info("Created audio device.", MessageSource.SoundManager);

                // Bind thread manager.
                ALThread.BindThread();

                // Setup listener.
                AL.Listener(ALListener3f.Position, 0, 0, 0);
                AL.Listener(ALListener3f.Velocity, 0, 0, 0);

                _loopRunning = true;
                while (_loopRunning)
                {
                    // Update running playbacks.
                    lock (_layers)
                    {
                        foreach (KeyValuePair<string, SoundLayer> layer in _layers)
                        {
                            layer.Value.Update();
                        }
                    }

                    // Run ALThread and cleanup only if focused.
                    if (Context.Host.Focused)
                    {
                        // Run queued actions.
                        ALThread.Run();

                        // Check for errors.
                        ALThread.CheckError("loop end");

                        // Check if any of the buffers waiting to be disposed are ready to be.
                        // Maximum of one buffer will be cleaned per loop.
                        bool took = _buffersToDestroy.TryTake(out SoundFile soundFile);
                        if (took)
                            foreach (KeyValuePair<string, SoundLayer> layer in _layers)
                            {
                                // If it is within the playlist, it is in use.
                                SoundFile foundFile = layer.Value.PlayList.FirstOrDefault(x => x.ALBuffer == soundFile.ALBuffer);
                                if (foundFile == null)
                                {
                                    // If not in use, delete it.
                                    AL.DeleteBuffer(soundFile.ALBuffer);
                                    soundFile.ALBuffer = -1;
                                }
                                else
                                {
                                    // Add back if still in use.
                                    _buffersToDestroy.Add(soundFile);
                                }
                            }
                    }

                    // Run the one loop token if any.
                    _oneLoopToken?.RunSynchronously();
                    _oneLoopToken = null;

                    // Update in interval specified in flag.
                    Task.Delay(Context.Flags.SoundThreadFrequency).Wait();
                }

                // Cleanup.
                ContextHandle unbound = new ContextHandle(IntPtr.Zero);
                Alc.MakeContextCurrent(unbound);
                Alc.DestroyContext(_context);
                Alc.CloseDevice(_device);
            }
            catch (Exception ex)
            {
                if (_loopRunning && !(ex is ThreadAbortException)) Context.Log.Error("Error in AL loop.", new Exception(ex.Message, ex), MessageSource.SoundManager);
            }
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
        public Task Play(SoundFile file, string layer)
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
        public Task QueuePlay(SoundFile file, string layer)
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
        internal int CreateBuffer(int channels, int bitsPerSample, byte[] soundData, int sampleRate)
        {
            int pointer = -1;

            ALThread.ExecuteALThread(() =>
            {
                pointer = AL.GenBuffer();
                AL.BufferData(pointer, GetSoundFormat(channels, bitsPerSample), soundData, soundData.Length, sampleRate);
            }).Wait();

            return pointer;
        }

        /// <summary>
        /// Add a buffer to be destroyed when it is no longer played.
        /// </summary>
        /// <param name="file">Pointer to the sound file holding the buffer to destroy.</param>
        internal void DestroyBuffer(SoundFile file)
        {
            _buffersToDestroy.Add(file);
        }

        #endregion

        #region Helpers

        public Task GetOneLoopToken()
        {
            _oneLoopToken = new Task(() => {});
            return _oneLoopToken;
        }

        /// <summary>
        /// Returns the type of sound format based on channels and bits.
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        /// <param name="bits">The number of bits.</param>
        /// <returns>The OpenAL </returns>
        private static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new Exception("Unknown format.");
            }
        }

        #endregion

        /// <summary>
        /// Creates the sound layer.
        /// </summary>
        /// <param name="layer">The name of the layer to create.</param>
        /// <returns>The created layer.</returns>
        private SoundLayer CreateLayer(string layer)
        {
            SoundLayer playBackLayer = new SoundLayer(layer);
            bool added = _layers.TryAdd(layer, playBackLayer);
            if (!added) Context.Log.Error($"Couldn't add sound layer {layer}", MessageSource.SoundManager);
            return playBackLayer;
        }

        /// <summary>
        /// Destroy the audio context.
        /// </summary>
        public void Dispose()
        {
            _loopRunning = false;
            while (!_soundThread.IsCompleted)
            {
            }

            _soundThread.Dispose();
        }
    }
}
#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.Common.Threading;
using Adfectus.Logging;
using Adfectus.OpenAL;
using Adfectus.Platform.DesktopGL.Assets;
using Adfectus.Sound;

#endregion

namespace Adfectus.Platform.DesktopGL.Sound
{
    /// <summary>
    /// Manages audio and interop with OpenAl.
    /// </summary>
    public sealed class ALSoundManager : SoundManager
    {
        /// <summary>
        /// List of active sound layers.
        /// </summary>
        public override string[] Layers
        {
            get => _layers.Keys.ToArray();
        }

        private ConcurrentDictionary<string, SoundLayer> _layers = new ConcurrentDictionary<string, SoundLayer>();
        private IntPtr _context;
        private IntPtr _device;
        private bool _loopRunning;
        private Task _soundThread;
        private ConcurrentQueue<ALSoundFile> _buffersToDestroy = new ConcurrentQueue<ALSoundFile>();

        public ALSoundManager(EngineBuilder builder) : base(builder)
        {
            _soundThread = new Task(SoundThreadLoop, TaskCreationOptions.LongRunning);
            _soundThread.Start();

            Engine.Log.Info($"Sound thread started on Task {_soundThread.Id}.", MessageSource.SoundManager);
        }

        private void SoundThreadLoop()
        {
            try
            {
                // Create audio context.
                _device = Alc.OpenDevice(null);
                int[] attr = new int[0];
                _context = Alc.CreateContext(_device, attr);
                if (_context == IntPtr.Zero) ErrorHandler.SubmitError(new Exception("Couldn't create OpenAL context."));
                bool success = Alc.MakeContextCurrent(_context);
                if (!success) ErrorHandler.SubmitError(new Exception("Couldn't make OpenAL context current."));
                Engine.Log.Info("OpenAL context created!", MessageSource.SoundManager);

                // Bind thread manager.
                ALThread.BindThread();

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
                        bool took = _buffersToDestroy.TryDequeue(out ALSoundFile soundFile);
                        if (took)
                            foreach (KeyValuePair<string, SoundLayer> layer in _layers)
                            {
                                // If it is within the playlist, it is in use.
                                ALSoundFile foundFile = layer.Value.PlayList.FirstOrDefault(x => x.ALBuffer == soundFile.ALBuffer);
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

        public override Task PlayOnLayer(string layerName, SoundFile file)
        {
            if (string.IsNullOrEmpty(layerName)) return Task.CompletedTask;
            SoundLayer layer = EnsureLayer(layerName);
            AwAction act = layer.Play(file);

            // Todo: Change with tasks all the way.
            Task t = new Task(() => { });
            act.ContinueWith(() => t.Start());
            return t;
        }

        public override Task QueueOnLayer(string layerName, SoundFile file)
        {
            if (string.IsNullOrEmpty(layerName)) return Task.CompletedTask;
            SoundLayer layer = EnsureLayer(layerName);
            AwAction act = layer.QueuePlay(file);

            // Todo: Change with tasks all the way.
            Task t = new Task(() => { });
            act.ContinueWith(() => t.Start());
            return t;
        }

        public override Task StopLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName)) return Task.CompletedTask;
            SoundLayer layer = EnsureLayer(layerName);
            AwAction act = layer.StopPlayingAll();

            // Todo: Change with tasks all the way.
            Task t = new Task(() => { });
            act.ContinueWith(() => t.Start());
            return t;
        }

        #region Helpers

        /// <summary>
        /// Ensure the layer exists. If it doesn't - create it.
        /// </summary>
        /// <param name="layerName">The layer to ensure the existence of.</param>
        /// <returns>The layer.</returns>
        private SoundLayer EnsureLayer(string layerName)
        {
            string nameNormalized = layerName.ToLower();
            bool layerExists = _layers.TryGetValue(nameNormalized, out SoundLayer layer);
            if (layerExists) return layer;

            layer = new SoundLayer(nameNormalized);
            bool added = _layers.TryAdd(nameNormalized, layer);
            if (!added) Engine.Log.Error($"Couldn't add layer {layerName}", MessageSource.SoundManager);

            return layer;
        }

        /// <summary>
        /// Returns a token which allows you to wait for a single loop of the sound thread.
        /// </summary>
        /// <returns>A task token for chaining tasks to sound thread loops.</returns>
        public AwAction GetOneLoopToken()
        {
            return ALThread.ExecuteALThread(() => { });
        }

        #endregion

        #region Buffer API

        /// <summary>
        /// Add a buffer to be destroyed when it is no longer played.
        /// </summary>
        /// <param name="file">Pointer to the sound file holding the buffer to destroy.</param>
        internal void DestroyBuffer(ALSoundFile file)
        {
            _buffersToDestroy.Enqueue(file);
        }

        #endregion

        /// <summary>
        /// Destroy the audio context.
        /// </summary>
        public override void Dispose()
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
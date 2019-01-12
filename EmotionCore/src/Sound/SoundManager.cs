// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    public class SoundManager
    {
        private Dictionary<string, SoundLayer> _layers;
        private ContextHandle _context;
        private IntPtr _device;
        private bool _loopRunning = false;
        private Thread _soundThread;

        /// <summary>
        /// Create a new sound manager.
        /// </summary>
        internal SoundManager()
        {
            _layers = new Dictionary<string, SoundLayer>();

            _soundThread = new Thread(SoundThreadLoop);
            _soundThread.Start();
            while (!_soundThread.IsAlive)
            {
            }

            InitDebug();
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
                if (_context.Handle == IntPtr.Zero)
                {
                    Context.Log.Error("Couldn't create OpenAL context.", MessageSource.SoundManager);
                }

                bool success = Alc.MakeContextCurrent(_context);
                if (!success)
                {
                    Context.Log.Error("Couldn't make OpenAL context current.", MessageSource.SoundManager);
                }
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

                    // Run queued actions.
                    ALThread.Run();

                    ALThread.CheckError("loop end");

                    Task.Delay((int) Context.RawFrameTime).Wait();
                }

                // Cleanup.
                ContextHandle unbound = new ContextHandle(IntPtr.Zero);
                Alc.MakeContextCurrent(unbound);
                Alc.DestroyContext(_context);
                Alc.CloseDevice(_device);
            }
            catch (Exception ex)
            {
                if(_loopRunning) Context.Log.Error("Error in AL loop.", ex, MessageSource.SoundManager);
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
        public SoundLayer Play(SoundFile file, string layer)
        {
            // Check whether the layer exists, and create it if it doesn't.
            SoundLayer playBackLayer = GetLayer(layer) ?? CreateLayer(layer);

            playBackLayer.Play(file);

            return playBackLayer;
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
        public SoundLayer PlayQueue(SoundFile file, string layer)
        {
            // Check whether the layer exists, and create it if it doesn't.
            SoundLayer playBackLayer = GetLayer(layer) ?? CreateLayer(layer);

            playBackLayer.QueuePlay(file);

            return playBackLayer;
        }

        /// <summary>
        /// Returns the sound layer object of the requested name or create one if it doesn't.
        /// </summary>
        /// <param name="layer">The name of the layer to look for.</param>
        /// <returns>The layer with the specified name.</returns>
        public SoundLayer GetLayer(string layer)
        {
            SoundLayer playBackLayer;

            lock (_layers)
            {
                _layers.TryGetValue(layer, out playBackLayer);
            }

            return playBackLayer ?? CreateLayer(layer);
        }

        /// <summary>
        /// Remove a layer by its name.
        /// </summary>
        /// <param name="layer">The name of the layer to remove.</param>
        public void RemoveLayer(string layer)
        {
            lock (_layers)
            {
                _layers.TryGetValue(layer, out SoundLayer playBackLayer);
                _layers.Remove(layer);
                playBackLayer?.Dispose();
            }
        }

        #endregion

        #region Debugging

        [Conditional("DEBUG")]
        private void InitDebug()
        {
            Context.ScriptingEngine.Expose("debugAudio", (Action) (() =>
            {
                lock (_layers)
                {
                    foreach (KeyValuePair<string, SoundLayer> layer in _layers)
                    {
                        Context.Log.Info(layer.ToString(), MessageSource.SoundManager);
                    }
                }
            }), "Dumps the status of the sound manager.");
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

            lock (_layers)
            {
                _layers.Add(layer, playBackLayer);
            }

            return playBackLayer;
        }

        /// <summary>
        /// Destroy the audio context.
        /// </summary>
        public void Dispose()
        {
            _loopRunning = false;
            while (_soundThread.IsAlive)
            {
            }
        }
    }
}
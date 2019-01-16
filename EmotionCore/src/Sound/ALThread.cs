// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Libraries;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    /// <summary>
    /// The thread the AL context was created on.
    /// </summary>
    public static class ALThread
    {
        private static ThreadManager _threadManager;

        static ALThread()
        {
            _threadManager = new ThreadManager("AL Thread") {BlockOnExecution = false};
        }

        /// <summary>
        /// Binds the current thread as the AL thread.
        /// </summary>
        internal static void BindThread()
        {
            _threadManager.BindThread();
        }

        /// <summary>
        /// Performs queued tasks on the AL thread.
        /// </summary>
        internal static void Run()
        {
            _threadManager.Run();
        }

        #region API

        /// <summary>
        /// Returns whether the executing thread is the AL thread.
        /// </summary>
        /// <returns>True if the thread on which this is called is the AL thread, false otherwise.</returns>
        public static bool IsALThread()
        {
            return _threadManager.IsManagedThread();
        }

        /// <summary>
        /// Execute the action on the AL thread. Will block the current thread until ready.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public static Task ExecuteALThread(Action action)
        {
            return _threadManager.ExecuteOnThread(action);
        }

        /// <summary>
        /// Check whether the executing thread is the AL thread. If it's not an exception is thrown.
        /// </summary>
        public static void ForceALThread()
        {
            _threadManager.ForceThread();
        }

        #endregion

        /// <summary>
        /// Check for an OpenAL error. Must be called on the ALThread.
        /// </summary>
        /// <param name="location">Where the error check is.</param>
        public static void CheckError(string location)
        {
            // Check if on AL thread.
            if (!IsALThread()) Context.Log.Error($"Error checking location {location} was executed outside of the AL thread.", MessageSource.SoundManager);

            // Get the error.
            ALError errorCheck = AL.GetError();

            // Check if anything.
            if (errorCheck == ALError.NoError) return;
            Context.Log.Error($"OpenAL error at {location}:\n{errorCheck}", MessageSource.SoundManager);

            // For some reason Mac reports errors left and right. As this happens only on that platform error checks are disabled there.
            if (CurrentPlatform.OS != PlatformName.Mac) throw new Exception("OpenAL error at " + location + ":\n" + errorCheck);
        }
    }
}
#region Using

using System;
using System.Diagnostics;

#endregion

namespace FreeImageAPI
{
    /// <summary>
    /// Class handling non-bitmap related functions.
    /// </summary>
    public static class FreeImageEngine
    {
        // TODO: ideally FreeImage would provide this... either way, it should probably be cleared before any call to the API...
        [ThreadStatic] public static string LastErrorMessage;

        #region Callback

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly object outputMessageFunctionLock;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static OutputMessageFunction outputMessageFunction;

        private static event OutputMessageFunction message;

        static FreeImageEngine()
        {
            outputMessageFunctionLock = new object();

            try
            {
                InitializeMessage();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Internal errors in FreeImage generate a logstring that can be
        /// captured by this event.
        /// </summary>
        public static event OutputMessageFunction Message
        {
            add
            {
                InitializeMessage();
                message += value;
            }
            remove
            {
                InitializeMessage();
                message -= value;
            }
        }

        private static void InitializeMessage()
        {
            if (null == outputMessageFunction)
                lock (outputMessageFunctionLock)
                {
                    if (null == outputMessageFunction)
                    {
                        FreeImage.ValidateAvailability();

                        try
                        {
                            // Create a delegate (function pointer) to 'OnMessage'
                            outputMessageFunction =
                                delegate(FREE_IMAGE_FORMAT fif, string message)
                                {
                                    LastErrorMessage = message;

                                    // Get a local copy of the multicast-delegate
                                    OutputMessageFunction m = FreeImageEngine.message;

                                    // Check the local copy instead of the static instance
                                    // to prevent a second thread from setting the delegate
                                    // to null, which would cause a nullreference exception
                                    if (m != null) m.Invoke(fif, message);
                                };

                            // Set the callback
                            FreeImage.SetOutputMessage(outputMessageFunction);
                        }
                        catch
                        {
                            outputMessageFunction = null;
                            throw;
                        }
                    }
                }
        }

        #endregion

        /// <summary>
        /// Gets a string containing the current version of the library.
        /// </summary>
        public static string Version
        {
            get => FreeImage.GetVersion();
        }

        /// <summary>
        /// Gets a string containing a standard copyright message.
        /// </summary>
        public static string CopyrightMessage
        {
            get => FreeImage.GetCopyrightMessage();
        }

        /// <summary>
        /// Gets whether the platform is using Little Endian.
        /// </summary>
        public static bool IsLittleEndian
        {
            get => FreeImage.IsLittleEndian();
        }
    }
}
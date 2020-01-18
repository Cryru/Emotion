#region Using

using System;

#endregion

namespace Khronos
{
    /// <summary>
    /// Basic exception thrown by KhronosApi classes.
    /// </summary>
    public abstract class KhronosException : InvalidOperationException
    {
        #region Constructors

        /// <summary>
        /// Construct a KhronosException.
        /// </summary>
        /// <param name="errorCode">
        /// A <see cref="Int32" /> that specifies the error code.
        /// </param>
        protected KhronosException(int errorCode)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Construct a KhronosException.
        /// </summary>
        /// <param name="errorCode">
        /// A <see cref="Int32" /> that specifies the error code.
        /// </param>
        /// <param name="message">
        /// A <see cref="String" /> that specifies the exception message.
        /// </param>
        protected KhronosException(int errorCode, string message) :
            base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Construct a KhronosException.
        /// </summary>
        /// <param name="errorCode">
        /// A <see cref="Int32" /> that specifies the error code.
        /// </param>
        /// <param name="message">
        /// A <see cref="String" /> that specifies the exception message.
        /// </param>
        /// <param name="innerException">
        /// The <see cref="Exception" /> wrapped by this Exception.
        /// </param>
        protected KhronosException(int errorCode, string message, Exception innerException) :
            base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        #endregion

        #region Error Code

        /// <summary>
        /// Khronos error code.
        /// </summary>
        public readonly int ErrorCode;

        #endregion
    }
}
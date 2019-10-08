#region Using

using Khronos;

#endregion

// ReSharper disable InheritdocConsiderUsage
// ReSharper disable CheckNamespace
namespace OpenGL
{
    /// <summary>
    /// Exception thrown by Gl class.
    /// </summary>
    public sealed class GlException : KhronosException
    {
        #region Constructors

        /// <summary>
        /// Construct a GlException.
        /// </summary>
        /// <param name="errorCode">
        /// A <see cref="ErrorCode" /> that specifies the error code.
        /// </param>
        internal GlException(ErrorCode errorCode) :
            base((int) errorCode, GetErrorMessage(errorCode))
        {
        }

        #endregion

        #region Error Messages

        /// <summary>
        /// Returns a description of the error code.
        /// </summary>
        /// <param name="errorCode">
        /// A <see cref="ErrorCode" /> that specifies the error code.
        /// </param>
        /// <returns>
        /// It returns a description of <paramref name="errorCode" />, asssuming that is a value returned
        /// by <see cref="Gl.GetError" />.
        /// </returns>
        private static string GetErrorMessage(ErrorCode errorCode)
        {
            switch (errorCode)
            {
                default:
                    return $"unknown error code {errorCode}";
                case OpenGL.ErrorCode.NoError:
                    return "no error";
                case OpenGL.ErrorCode.InvalidEnum:
                    return "invalid enumeration";
                case OpenGL.ErrorCode.InvalidFramebufferOperation:
                    return "invalid framebuffer operation";
                case OpenGL.ErrorCode.InvalidOperation:
                    return "invalid operation";
                case OpenGL.ErrorCode.InvalidValue:
                    return "invalid value";
                case OpenGL.ErrorCode.OutOfMemory:
                    return "out of memory";
                case OpenGL.ErrorCode.StackOverflow:
                    return "stack overflow";
                case OpenGL.ErrorCode.StackUnderflow:
                    return "stack underflow";
            }
        }

        #endregion
    }
}
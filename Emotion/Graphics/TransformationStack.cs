#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// A stack of 4x4 matrices. Each matrix pushed is multiplied by the last one in the stack.
    /// </summary>
    public sealed class TransformationStack
    {
        /// <summary>
        /// Returns the top matrix.
        /// </summary>
        public Matrix4x4 CurrentMatrix
        {
            get => _stack.Peek();
        }

        private Stack<Matrix4x4> _stack;

        public TransformationStack()
        {
            _stack = new Stack<Matrix4x4>();
            Push(Matrix4x4.Identity, false);
        }

        #region API

        /// <summary>
        /// Push a matrix to the top of the stack.
        /// </summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="multiply">Whether to multiply the new matrix by the previous matrix.</param>
        public void Push(Matrix4x4 matrix, bool multiply = true)
        {
            if (multiply)
                _stack.Push(CurrentMatrix * matrix);
            else
                _stack.Push(matrix);
        }

        /// <summary>
        /// Remove the top matrix.
        /// </summary>
        public void Pop()
        {
            if (_stack.Count <= 1)
            {
                Engine.Log.Warning("Tried to pop the first identity matrix out of the stack.", MessageSource.Renderer);
                return;
            }

            _stack.Pop();
        }

        #endregion
    }
}
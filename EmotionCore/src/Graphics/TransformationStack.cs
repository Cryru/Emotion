// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    public sealed class TransformationStack
    {
        /// <summary>
        /// Returns the top matrix.
        /// </summary>
        public Matrix4 CurrentMatrix
        {
            get => _stack.Peek();
        }

        private Stack<Matrix4> _stack;

        internal TransformationStack()
        {
            _stack = new Stack<Matrix4>();
            Push(Matrix4.Identity, false);
        }

        #region API

        /// <summary>
        /// Push a matrix to the top of the stack.
        /// </summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="multiply">Whether to multiply the new matrix by the previous matrix.</param>
        public void Push(Matrix4 matrix, bool multiply = true)
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
            if (_stack.Count <= 1) throw new Exception("Tried to pop the first identity matrix out of the stack.");

            _stack.Pop();
        }

        #endregion
    }
}
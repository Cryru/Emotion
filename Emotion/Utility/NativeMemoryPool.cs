#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Utility
{
    public sealed class NativeMemoryPool : IDisposable
    {
        /// <summary>
        /// The amount of memory in use.
        /// </summary>
        public int MemoryFootprint
        {
            get => _pages.Count * _pageSize;
        }

        private List<IntPtr> _pages = new List<IntPtr>();
        private int _currentBuffer;
        private int _memoryUsed;
        private int _pageSize;

        public NativeMemoryPool(int pageSize)
        {
            _pageSize = pageSize;
        }

        /// <summary>
        /// Get a chunk of native memory.
        /// </summary>
        /// <param name="minSize">The min size of the buffer needed.</param>
        /// <returns>A pointer to the native memory requested.</returns>
        public unsafe IntPtr GetMemory(int minSize)
        {
            Debug.Assert(minSize <= _pageSize);

            // Check if there's a current buffer.
            if (_currentBuffer >= _pages.Count) CreatePage();

            // Check if there is enough space in the current buffer.
            if (_pageSize - _memoryUsed < minSize)
            {
                CreatePage();
                _currentBuffer++;
            }

            IntPtr page = _pages[_currentBuffer];

            // ReSharper disable once PossibleNullReferenceException
            return new IntPtr(&((byte*) page)[_memoryUsed]);
        }

        /// <summary>
        /// Mark an amount of memory in the last page as used.
        /// </summary>
        /// <param name="amount">The amount used.</param>
        /// <returns>How much memory is left in that page.</returns>
        public int MarkUsed(int amount)
        {
            Debug.Assert(_memoryUsed + amount <= _pageSize);

            _memoryUsed += amount;
            return _pageSize - _memoryUsed;
        }

        /// <summary>
        /// Reset memory trackers.
        /// </summary>
        public void Reset()
        {
            _memoryUsed = 0;
            _currentBuffer = 0;
        }

        /// <summary>
        /// Clear native memory.
        /// </summary>
        public void Dispose()
        {
            Reset();
            foreach (IntPtr p in _pages)
            {
                Marshal.FreeHGlobal(p);
            }

            _pages.Clear();
        }

        #region Internal

        private void CreatePage()
        {
            _pages.Add(Marshal.AllocHGlobal(_pageSize));
            _memoryUsed = 0;
        }

        #endregion
    }
}
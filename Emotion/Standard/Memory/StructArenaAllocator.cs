#nullable enable

#region Using


#endregion

namespace Emotion.Standard.Memory
{
    public class StructArenaAllocator<T> where T : struct
    {
        private T[] _pool;
        private int _pointer;

        private int _growthSize;

        public StructArenaAllocator(int growthSize)
        {
            _growthSize = growthSize;
            _pool = new T[growthSize];
        }

        public ref T Allocate(out int index)
        {
            if (_pointer >= _pool.Length)
                Array.Resize(ref _pool, _pool.Length + _growthSize);

            index = _pointer;
            ref T obj = ref _pool[_pointer];
            _pointer++;

            return ref obj;
        }

        public void Reset()
        {
            _pointer = 0;
        }

        public int Length => _pointer;

        public ref T this[int index]
        {
            get => ref _pool[index];
        }

        public Span<T> GetActiveSlice()
        {
            return new Span<T>(_pool, 0, _pointer);
        }
    }
}
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using NanoUI.Utils;

namespace NanoUI.Common
{
    // this is only used in drawing "layer"

    /// <summary>
    /// UnsafeBuffer<T>.
    /// </summary>
    internal unsafe struct UnsafeBuffer<T> where T : unmanaged
    {
        const int DefaultCapacity = 4;

        T* _items;
        int _count;
        int _capacity;

        // Initializes a new instance of the <see cref="UnsafeBuffer{T}"/> struct with the specified capacity.
        public UnsafeBuffer(int capacity)
        {
            Capacity = capacity;
        }

        // Gets the number of elements in the buffer.
        public readonly int Count => _count;

        // Gets the pointer to the underlying data array.
        public readonly T* Data => _items;

        // Gets a value indicating whether the buffer is empty.
        public readonly bool IsEmpty => _count == 0;

        // Gets a pointer to the first element in the buffer.        
        public readonly ref T First => ref _items[0];

        // Gets a pointer to the last element in the buffer.
        public readonly ref T Last => ref _items[_count - 1];

        // Gets or sets the capacity of the buffer.        
        public int Capacity
        {
            readonly get => _capacity;
            set
            {
                if (_items == null)
                {
                    // allocates zeroed memory with capasity (count)
                    _items = MemoryUtils.Alloc<T>(value, true);
                    _capacity = value;
                    return;
                }

                _items = MemoryUtils.ReAlloc(_items, value);
                _capacity = value;
                _count = _capacity < _count ? _capacity : _count;
            }
        }

        // Gets the element at the specified index.
        public ref T this[int index]
        {
            // todo: validate index
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _items[index];
        }

        // Increases buffer to fit addition.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Add()
        {
            if ((uint)_count < (uint)_capacity)
            {
                _count++;
            }
            else
            {
                Grow(_count++);
            }
            return ref Last;
        }

        // Appends a range of elements to the end of the vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> values)
        {
            Reserve(_count + values.Length);

            fixed (T* src = values)
            {
                MemoryUtils.MemoryCopy(src, &_items[_count], _capacity * sizeof(T), values.Length * sizeof(T));
            }
            _count += values.Length;
        }

        // Inserts placeholder at the specified index in the buffer.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Insert(int index)
        {
            Reserve(_count + 1);

            var size = (_count - index) * sizeof(T);

            Buffer.MemoryCopy(&_items[index], &_items[index + 1], size, size);

            _count++;

            return ref this[index];
        }

        // Removes the item at the specified index from the buffer.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index == _count - 1)
            {
                _count--;
                return;
            }

            var size = (_count - index) * sizeof(T);
            Buffer.MemoryCopy(&_items[index + 1], &_items[index], size, size);
            _count--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveLast()
        {
            RemoveAt(_count - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Copy(int sourceIndex, int targetIndex)
        {
            MemoryUtils.MemoryCopy(_items + sourceIndex, _items + targetIndex, 1);
        }

        // Searches for the first occurrence of an element in the collection that matches the specified condition and returns its index.
        public int FirstIndexOf(Func<T, bool> comparer)
        {
            for (int i = 0; i < _count; i++)
            {
                if (comparer(_items[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        // Resizes the vector to the specified count. If the new count is larger than the current capacity, the _capacity will be increased accordingly.
        public void Resize(int count)
        {
            if (_count == count)
            {
                return;
            }

            Reserve(count);

            _count = count;
        }

        // Ensures that the buffer has the specified _capacity.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reserve(int capacity)
        {
            if (_capacity < capacity || _items == null)
            {
                Grow(capacity);
            }
        }

        // Reverses the order of the elements in the buffer.        
        public readonly void Reverse()
        {
            new Span<T>(_items, _count).Reverse();
        }

        // Reduces the vector's capacity to match its count.
        public void Compact()
        {
            Capacity = _count;
        }

        // Sets all elements in the vector to their default values and resets the count to 0.
        public readonly void ZeroMemory()
        {
            MemoryUtils.ZeroMemory(_items, _capacity);
        }

        // Clears the buffer (not zeroing values).

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => new(_items, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int offset, int count) => AsReadOnlySpan().Slice(offset, count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => new(_items, _count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int offset, int count) => AsSpan().Slice(offset, count);

        public override readonly int GetHashCode()
        {
            return HashCode.Combine((nint)_items);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_items != null)
            {
                MemoryUtils.Free(_items);
                this = default;
            }
        }

        #region Private

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Grow(int capacity)
        {
            int newCapacity = _count == 0 ? DefaultCapacity : 2 * _count;

            if (newCapacity < capacity)
            {
                newCapacity = capacity;
            }

            Capacity = newCapacity;
        }


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("DEBUG")]
        void ValidateIndex(int index)
        {
            //ArgumentOutOfRangeException.ThrowIfNegative(index);
            //ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _count);
            Debug.Assert(index >= 0 && index < _count, "Index out of range.");
        }

        #endregion
    }
}

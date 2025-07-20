using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NanoUI.Common
{
    // todo : add methods from C# ArrayList (not generics)
    public class ArrayBuffer<T>
    {
        T[] _items;
        int _count;

        const int DefaultCapacity = 4;
        const float GrowthFactor = 2f;

        public ArrayBuffer() : this(DefaultCapacity) { }

        public ArrayBuffer(int capacity)
        {
            Debug.Assert(capacity > 0);
            _items = new T[capacity];
        }

        public int Count => _count;
        
        public ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(_items, 0, _count);
        public ReadOnlySpan<T> AsReadOnlySpan(int offset, int count)
        {
            return new ReadOnlySpan<T>(_items, offset, count);
        }

        public Span<T> AsSpan() => new Span<T>(_items, 0, _count);
        public Span<T> AsSpan(int offset, int count)
        {
            return new Span<T>(_items, offset, count);
        }
        
        public ref T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return ref _items[index];
            }
        }
        public ref T Last
        {
            get
            {
                ValidateIndex(_count -1);
                return ref _items[_count - 1];
            }
        }

        public bool Contains(T item)
        {
            //return GetIndex(item, out _);
            var index = Array.IndexOf(_items, item);
            return index >= 0 && index < _count;
        }

        public virtual void Add(ref T item)
        {
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, (int)(_items.Length * GrowthFactor));
            }

            _items[_count] = item;
            _count += 1;
        }

        public virtual void Add(T item)
        {
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, (int)(_items.Length * GrowthFactor));
            }

            _items[_count] = item;
            _count += 1;
        }

        public void AddRange(T[] items)
        {
            Debug.Assert(items != null);

            int requiredSize = _count + items.Length;
            if (requiredSize > _items.Length)
            {
                Array.Resize(ref _items, (int)(requiredSize * GrowthFactor));
            }

            Array.Copy(items, 0, _items, _count, items.Length);
            _count += items.Length;
        }

        public void AddRange(ReadOnlySpan<T> items)
        {
            Debug.Assert(items != null);

            int requiredSize = _count + items.Length;
            if (requiredSize > _items.Length)
            {
                Array.Resize(ref _items, (int)(requiredSize * GrowthFactor));
            }

            items.CopyTo(new Span<T>(_items, _count, items.Length));

            _count += items.Length;
        }

        public void Insert(int index, T item)
        {
            if (_count == _items.Length)
                Array.Resize(ref _items, _count + 1);

            if (index < _count)
            {
                Array.Copy(_items, index, _items, index + 1, _count - index);
            }
            
            _items[index] = item;
            _count++;
        }

        public void InsertRange(int index, T[] items)
        {
            Debug.Assert(items != null);

            int requiredSize = _count + items.Length;

            if (requiredSize > _items.Length)
            {
                Array.Resize(ref _items, (int)(requiredSize * GrowthFactor));
            }

            // copy items from area to end
            Array.Copy(_items, index, _items, index + items.Length, _count - index);
            // copy new array
            Array.Copy(items, 0, _items, index, items.Length);

            _count += items.Length;
        }

        public void Replace(int index, ref T item)
        {
            ValidateIndex(index);
            _items[index] = item;
        }

        // if array has grown too big, it can be shrinked to current count.
        // No data loss.
        public bool Optimize()
        {
            // prevent to shrink array to 0 length (can't resize then)
            if(_count > 0 && _items.Length > _count)
            {
                Array.Resize(ref _items, _count);
                
                return true;
            }

            return false;
        }

        // NOTE : this can lead losing data
        // Can't resize to 0 length
        public bool Resize(int count)
        {
            Debug.Assert(count > 0);

            if (count > 0)
            {
                Array.Resize(ref _items, count);
                _count = count;

                return true;
            }
            
            return false;
        }

        public bool Remove(ref T item)
        {
            bool contained = IndexOf(item, out int index);

            if (contained)
            {
                CoreRemoveAt(index);
            }

            return contained;
        }

        public bool Remove(T item)
        {
            bool contained = IndexOf(item, out int index);
            if (contained)
            {
                CoreRemoveAt(index);
            }

            return contained;
        }

        public bool RemoveLast()
        { 
            if(_count > 0)
            {
                _count--;
                return true;
            }

            return false;
        }

        public bool RemoveAt(int index)
        {
            if(index >= _count)
            {
                return false;
            }
            ValidateIndex(index);
            CoreRemoveAt(index);

            return true;
        }

        public bool RemoveAt(int index, int count)
        {
            if (_count < index + count)
            {
                return false;
            }

            ValidateIndex(index);

            // copy items from end to area
            Array.Copy(_items, index + count, _items, index, _count - (index + count));
            // resize count
            _count -= count;

            return true;
        }

        public virtual void FindAll(Predicate<T> filter, List<T> result, bool recursive = true)
        {
            for(int i = 0; i < _count; i++)
            {
                ref T item = ref _items[i];

                if(filter.Invoke(item))
                    result.Add(item);
            }
        }

        public void Clear()
        {
            //Array.Clear(_items, 0, _items.Length);
            _count = 0;
        }

        // release reference for unused classes (outside count)
        public void ReleaseUnused()
        {
            for (int i = _count; i < _items.Length; i++)
            {
                if (_items[i] == null)
                    break;

                _items[i] = default;
            }
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item);
        }

        public bool IndexOf(T item, out int index)
        {
            int signedIndex = Array.IndexOf(_items, item);
            index = signedIndex;

            return signedIndex != -1 && signedIndex < _count;
        }

        public void Sort(IComparer<T> comparer)
        {
            Debug.Assert(comparer != null);
            // not whole array
            //Array.Sort(_items, comparer);
            new Span<T>(_items, 0, _count).Sort(comparer);
        }

        public void TransformAll(Func<T, T> transformation)
        {
            Debug.Assert(transformation != null);

            for (int i = 0; i < _count; i++)
            {
                _items[i] = transformation(_items[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CoreRemoveAt(int index)
        {
            _count -= 1;
            Array.Copy(_items, index + 1, _items, index, _count - index);
            //_items[_count] = default(T);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ValidateIndex(int index)
        {
            Debug.Assert(index >= 0 && index < _count);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NanoUI.Common
{
    // todo : add methods from C# ArrayList (not generics)

    /// <summary>
    /// ArrayBuffer<T>.
    /// </summary>
    public class ArrayBuffer<T>
    {
        T[] _items;
        int _count;

        const int DefaultCapacity = 4;
        const float GrowthFactor = 2f;

        /// <summary>
        /// Create ArrayBuffer with default capacity (= 4).
        /// </summary>
        public ArrayBuffer() : this(DefaultCapacity) { }

        /// <summary>
        /// Create ArrayBuffer with capacity.
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public ArrayBuffer(int capacity)
        {
            Debug.Assert(capacity > 0);
            _items = new T[capacity];
        }

        /// <summary>
        /// Items count.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Get items.
        /// </summary>
        /// <returns>ReadOnlySpan<T></returns>
        public ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(_items, 0, _count);

        /// <summary>
        /// Get items.
        /// </summary>
        /// <param name="offset">Start index</param>
        /// <param name="count">Count</param>
        /// <returns>ReadOnlySpan<T></returns>
        public ReadOnlySpan<T> AsReadOnlySpan(int offset, int count)
        {
            return new ReadOnlySpan<T>(_items, offset, count);
        }

        /// <summary>
        /// Get items.
        /// </summary>
        /// <returns>Span<T></returns>
        public Span<T> AsSpan() => new Span<T>(_items, 0, _count);

        /// <summary>
        /// Get items.
        /// </summary>
        /// <param name="offset">Start index</param>
        /// <param name="count">Count</param>
        /// <returns>Span<T></returns>
        public Span<T> AsSpan(int offset, int count)
        {
            return new Span<T>(_items, offset, count);
        }

        /// <summary>
        /// Get Item at index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Item</returns>
        public ref T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return ref _items[index];
            }
        }

        /// <summary>
        /// Get last item.
        /// </summary>
        public ref T Last
        {
            get
            {
                ValidateIndex(_count -1);
                return ref _items[_count - 1];
            }
        }

        /// <summary>
        /// Buffer contains item?
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        public bool Contains(T item)
        {
            //return GetIndex(item, out _);
            var index = Array.IndexOf(_items, item);
            return index >= 0 && index < _count;
        }

        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="item">Item</param>
        public virtual void Add(ref T item)
        {
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, (int)(_items.Length * GrowthFactor));
            }

            _items[_count] = item;
            _count += 1;
        }

        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="item">Item</param>
        public virtual void Add(T item)
        {
            if (_count == _items.Length)
            {
                Array.Resize(ref _items, (int)(_items.Length * GrowthFactor));
            }

            _items[_count] = item;
            _count += 1;
        }

        /// <summary>
        /// Add items.
        /// </summary>
        /// <param name="items">Array of items</param>
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

        /// <summary>
        /// Add items.
        /// </summary>
        /// <param name="items">Span of items</param>
        public void AddRange(ReadOnlySpan<T> items)
        {
            int requiredSize = _count + items.Length;
            if (requiredSize > _items.Length)
            {
                Array.Resize(ref _items, (int)(requiredSize * GrowthFactor));
            }

            items.CopyTo(new Span<T>(_items, _count, items.Length));

            _count += items.Length;
        }

        /// <summary>
        /// Insert item at index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
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

        /// <summary>
        /// Insert items.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="items">Items</param>
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

        /// <summary>
        /// Replace item at index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        public void Replace(int index, ref T item)
        {
            ValidateIndex(index);
            _items[index] = item;
        }

        /// <summary>
        /// Optimize.
        /// </summary>
        /// <returns>Success</returns>
        /// <remarks>
        /// If array has grown too big, it can be shrinked to current count.
        /// No data loss.
        /// </remarks>
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

        /// <summary>
        /// Resize.
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Success</returns>
        /// <remarks>
        /// This can lead to losing data. Can't resize to 0 length
        /// </remarks>
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

        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Success</returns>
        public bool Remove(ref T item)
        {
            bool contained = IndexOf(item, out int index);

            if (contained)
            {
                CoreRemoveAt(index);
            }

            return contained;
        }

        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Success</returns>
        public bool Remove(T item)
        {
            bool contained = IndexOf(item, out int index);
            if (contained)
            {
                CoreRemoveAt(index);
            }

            return contained;
        }

        /// <summary>
        /// Remove last item.
        /// </summary>
        /// <returns>Success</returns>
        public bool RemoveLast()
        { 
            if(_count > 0)
            {
                _count--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove item at index.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Success</returns>
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

        /// <summary>
        /// Remove item(s).
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <returns>Success</returns>
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

        /// <summary>
        /// Find all with filter.
        /// </summary>
        /// <param name="filter">Predicate</param>
        /// <param name="result">Result</param>
        public virtual void FindAll(Predicate<T> filter, List<T> result)
        {
            for(int i = 0; i < _count; i++)
            {
                ref T item = ref _items[i];

                if(filter.Invoke(item))
                    result.Add(item);
            }
        }

        /// <summary>
        /// Clear.
        /// </summary>
        public void Clear()
        {
            //Array.Clear(_items, 0, _items.Length);
            _count = 0;
        }

        /// <summary>
        /// Release reference for unused classes (outside count).
        /// </summary>
        /// <remarks>Not implemented</remarks>
        public void ReleaseUnused()
        {
            // todo
            return;
        }

        /// <summary>
        /// Find index of item.
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item);
        }

        /// <summary>
        /// Find index of item.
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <returns>Success</returns>
        public bool IndexOf(T item, out int index)
        {
            int signedIndex = Array.IndexOf(_items, item);
            index = signedIndex;

            return signedIndex != -1 && signedIndex < _count;
        }

        /// <summary>
        /// Sort with comparer.
        /// </summary>
        /// <param name="comparer">Comparer</param>
        public void Sort(IComparer<T> comparer)
        {
            Debug.Assert(comparer != null);
            // not whole array
            //Array.Sort(_items, comparer);
            new Span<T>(_items, 0, _count).Sort(comparer);
        }

        /// <summary>
        /// Transform all items.
        /// </summary>
        /// <param name="transformation">Transformation</param>
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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoUI.Utils
{
    /// <summary>
    /// MemoryUtils.
    /// </summary>
    public unsafe static class MemoryUtils
    {
        /// <summary>Tries to allocate a chunk of unmanaged memory.</summary>
        /// <param name="count">The count of elements contained in the allocation.</param>
        /// <param name="zero"><c>true</c> if the allocated memory should be zeroed; otherwise, <c>false</c>.</param>
        /// <returns>The address to an allocated chunk of memory that is bytes in length or <c>null</c> if the allocation failed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Alloc<T>(int count, bool zero = false) where T : unmanaged
        {
            void* result = zero ? NativeMemory.AllocZeroed((nuint)count, (nuint)sizeof(T)) :
                NativeMemory.Alloc((nuint)count, (nuint)sizeof(T));

            if (result == null)
            {
                throw new OutOfMemoryException($"The allocation of '{count}x{sizeof(T)}' bytes failed");
            }

            return (T*)result;
        }

        /// <summary>
        /// Reallocates a block of memory to be the specified size in bytes (count * sizeof(T)).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pointer"></param>
        /// <param name="count"></param>
        /// <returns>The address to an reallocated chunk of memory that is bytes in length or <c>null</c> if the allocation failed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* ReAlloc<T>(T* pointer, int count) where T : unmanaged
        {
            void* result = NativeMemory.Realloc(pointer, (nuint)(count * sizeof(T)));

            if (result == null)
            {
                throw new OutOfMemoryException($"The reallocation of '{count}x{sizeof(T)}' bytes failed");
            }

            return (T*)result;
        }

        /// <summary>Frees an allocated chunk of unmanaged memory.</summary>
        /// <param name="pointer">The address to an allocated chunk of memory to free</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* pointer) => NativeMemory.Free(pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemoryCopy<T>(T* src, T* dst, int count) where T : unmanaged
        {
            Buffer.MemoryCopy(src, dst, count * sizeof(T), count * sizeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemoryCopy(void* src, void* dst, long dstLength, long srcLength)
        {
            Buffer.MemoryCopy(src, dst, dstLength, srcLength);
        }

        public static unsafe void SwapT<T>(T* a, T* b) where T : unmanaged
        {
            (*b, *a) = (*a, *b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMemory<T>(T* pointer, int length) where T : unmanaged
        {
            Unsafe.InitBlockUnaligned(pointer, 0, (uint)(sizeof(T) * length));
        }
    }
}

using NanoUI.Common;
using System;

namespace NanoUI.Rendering
{
    // _indexes creation (CounterClockwise)
    public static partial class DrawCache
    {
        static ushort[] _fanIndices = Array.Empty<ushort>();
        static ushort[] _stripIndices = Array.Empty<ushort>();
        // this is for quad indexing (Text)
        static ushort[] _quadIndices = Array.Empty<ushort>();

        static UnsafeBuffer<ushort> _indexes = new(1024);

        /// <summary>
        /// Collected indices since last BeginFrame() command.
        /// </summary>
        public static ReadOnlySpan<ushort> Indexes => _indexes.AsReadOnlySpan();

        // TriangleFan
        static void AddTriangleFanIndexes(int vertexCount)
        {
            int indexCount = (vertexCount - 2) * 3;

            if (_fanIndices.Length < indexCount)
            {
                Array.Resize(ref _fanIndices, indexCount);

                int index = 0;

                for (uint i = 2; i < vertexCount; ++i)
                {
                    _fanIndices[index] = 0;
                    _fanIndices[index + 1] = (ushort)(i - 1);
                    _fanIndices[index + 2] = (ushort)i;

                    index += 3;
                }
            }

            _indexes.AddRange(new ReadOnlySpan<ushort>(_fanIndices, 0, indexCount));
        }
        
        // TriangleStrip
        static void AddTriangleStripIndexes(int vertexCount)
        {
            int indexCount = (vertexCount - 2) * 3;

            if (_stripIndices.Length < indexCount)
            {
                Array.Resize(ref _stripIndices, indexCount);

                int index = 0;

                for (uint i = 0; i < vertexCount - 2; i++)
                {
                    if (i % 2 == 0)
                    {
                        _stripIndices[index] = (ushort)i;
                        _stripIndices[index + 1] = (ushort)(i + 1);
                        _stripIndices[index + 2] = (ushort)(i + 2);
                    }
                    else
                    {
                        _stripIndices[index] = (ushort)i;
                        _stripIndices[index + 1] = (ushort)(i + 2);
                        _stripIndices[index + 2] = (ushort)(i + 1);
                    }
                    index += 3;
                }
            }

            _indexes.AddRange(new ReadOnlySpan<ushort>(_stripIndices, 0, indexCount));
        }
        
        // Quad
        static void AddQuadIndexes(int vertexCount)
        {
            int indexCount = vertexCount * 6 / 4;

            if (_quadIndices.Length < indexCount)
            {
                Array.Resize(ref _quadIndices, indexCount);

                uint counter = 0;
                int index = 0;

                while (counter + 4 <= vertexCount)
                {
                    _quadIndices[index] = (ushort)counter;
                    _quadIndices[index + 1] = (ushort)(counter + 2);
                    _quadIndices[index + 2] = (ushort)(counter + 1);

                    _quadIndices[index + 3] = (ushort)counter;
                    _quadIndices[index + 4] = (ushort)(counter + 3);
                    _quadIndices[index + 5] = (ushort)(counter + 2);

                    index += 6;
                    counter += 4;
                }
            }

            _indexes.AddRange(new ReadOnlySpan<ushort>(_quadIndices, 0, indexCount));
        }
    }
}

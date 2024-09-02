using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ToCBS.Wof
{
    public class Wof
    {
        private static readonly uint WofCompressionChunkSize = 8192;

        public static void WofDecompress(Stream input, Stream output, uint uncompressedSize)
        {
            if (!NativeMethods.CreateDecompressor((uint)(COMPRESS_ALGORITHM.COMPRESS_ALGORITHM_XPRESS_HUFF | COMPRESS_ALGORITHM.COMPRESS_RAW), IntPtr.Zero, out IntPtr decompressorHandle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                uint chunkCountForDecompressedSize = GetChunkCountForDecompressedSize(uncompressedSize);

                byte[] chunkOffsetTable = new byte[4 * (chunkCountForDecompressedSize - 1)];
                byte[] uncompressedBuffer = new byte[WofCompressionChunkSize];
                byte[] compressedData = new byte[WofCompressionChunkSize];

                input.Read(chunkOffsetTable, 0, chunkOffsetTable.Length);

                int num = (int)input.Length - chunkOffsetTable.Length;
                uint readOffset = 0;
                int currentChunk = 0;

                while (currentChunk < chunkCountForDecompressedSize)
                {
                    uint newReadOffset = (currentChunk < (chunkCountForDecompressedSize - 1)) ? BitConverter.ToUInt32(chunkOffsetTable, 4 * currentChunk) : (uint)num;
                    uint compressedDataSize = newReadOffset - readOffset;

                    input.Read(compressedData, 0, (int)compressedDataSize);

                    readOffset = newReadOffset;

                    if (compressedDataSize == WofCompressionChunkSize)
                    {
                        output.Write(compressedData, 0, (int)compressedDataSize);
                    }
                    else
                    {
                        ulong uncompressedBufferSize = (ulong)uncompressedBuffer.Length;

                        if (currentChunk == chunkCountForDecompressedSize - 1)
                        {
                            uncompressedBufferSize = uncompressedSize - (chunkCountForDecompressedSize - 1) * WofCompressionChunkSize;
                        }

                        if (!NativeMethods.Decompress(decompressorHandle, compressedData, compressedDataSize, uncompressedBuffer, uncompressedBufferSize, IntPtr.Zero))
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        output.Write(uncompressedBuffer, 0, (int)uncompressedBufferSize);
                    }
                    currentChunk++;
                }
            }
            finally
            {
                NativeMethods.CloseDecompressor(decompressorHandle);
            }
        }

        private static bool WriteDetourOk(long inputPosition, int inputCount, long position, int count)
        {
            long begin = Math.Max(inputPosition, position);
            long end = Math.Min(inputPosition + inputCount, position + count);

            return end > begin;
        }

        private static void WriteDetour(long inputPosition, byte[] inputBuffer, int inputCount, long position, byte[] buffer, int offset, int count)
        {
            long begin = Math.Max(inputPosition, position);
            long end = Math.Min(inputPosition + inputCount, position + count);

            if (end > begin)
            {
                int index = (int)(begin - position) + offset;
                int inputIndex = (int)(begin - inputPosition);

                using var strm = new MemoryStream(buffer);
                strm.Seek(index, SeekOrigin.Begin);
                strm.Write(inputBuffer, inputIndex, (int)(end - begin));
            }
        }

        public static void WofDecompress(Stream input, uint uncompressedSize, long position, byte[] buffer, int offset, int count)
        {
            long inputPosition = 0;
            long originalInputPosition = input.Position;

            if (!NativeMethods.CreateDecompressor((uint)(COMPRESS_ALGORITHM.COMPRESS_ALGORITHM_XPRESS_HUFF | COMPRESS_ALGORITHM.COMPRESS_RAW), IntPtr.Zero, out IntPtr decompressorHandle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                uint chunkCountForDecompressedSize = GetChunkCountForDecompressedSize(uncompressedSize);

                byte[] chunkOffsetTable = new byte[4 * (chunkCountForDecompressedSize - 1)];
                byte[] uncompressedBuffer = new byte[WofCompressionChunkSize];
                byte[] compressedData = new byte[WofCompressionChunkSize];

                input.Read(chunkOffsetTable, 0, chunkOffsetTable.Length);

                int num = (int)input.Length - chunkOffsetTable.Length;
                uint readOffset = 0;
                int currentChunk = 0;

                while (currentChunk < chunkCountForDecompressedSize)
                {
                    uint newReadOffset = (currentChunk < (chunkCountForDecompressedSize - 1)) ? BitConverter.ToUInt32(chunkOffsetTable, 4 * currentChunk) : (uint)num;
                    uint compressedDataSize = newReadOffset - readOffset;

                    input.Read(compressedData, 0, (int)compressedDataSize);

                    readOffset = newReadOffset;

                    if (compressedDataSize == WofCompressionChunkSize)
                    {
                        WriteDetour(inputPosition, compressedData, (int)compressedDataSize, position, buffer, offset, count);
                        inputPosition += (int)compressedDataSize;
                    }
                    else
                    {
                        ulong uncompressedBufferSize = (ulong)uncompressedBuffer.Length;

                        if (currentChunk == chunkCountForDecompressedSize - 1)
                        {
                            uncompressedBufferSize = uncompressedSize - (chunkCountForDecompressedSize - 1) * WofCompressionChunkSize;
                        }

                        if (WriteDetourOk(inputPosition, (int)uncompressedBufferSize, position, count))
                        {
                            if (!NativeMethods.Decompress(decompressorHandle, compressedData, compressedDataSize, uncompressedBuffer, uncompressedBufferSize, IntPtr.Zero))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }

                            WriteDetour(inputPosition, uncompressedBuffer, (int)uncompressedBufferSize, position, buffer, offset, count);
                        }

                        inputPosition += (int)uncompressedBufferSize;
                    }
                    currentChunk++;
                }
            }
            finally
            {
                NativeMethods.CloseDecompressor(decompressorHandle);
            }

            if (input.CanSeek)
            {
                input.Position = originalInputPosition;
            }
        }

        private static uint GetChunkCountForDecompressedSize(uint uncompressedSize)
        {
            return uncompressedSize / WofCompressionChunkSize + ((uncompressedSize % WofCompressionChunkSize != 0) ? 1U : 0U);
        }
    }
}
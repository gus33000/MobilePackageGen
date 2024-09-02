using System.Runtime.InteropServices;

namespace ToCBS.Wof
{
    internal static class NativeMethods
    {

        [DllImport("cabinet.dll", SetLastError = true)]
        internal static extern bool CreateDecompressor(uint algorithm, IntPtr allocationRoutines, out IntPtr decompressorHandle);

        [DllImport("cabinet.dll", SetLastError = true)]
        internal static extern bool Decompress(IntPtr decompressorHandle, byte[] compressedData, ulong compressedDataSize, byte[] uncompressedBuffer, ulong uncompressedBufferSize, IntPtr uncompressedDataSize);

        [DllImport("cabinet.dll", SetLastError = true)]
        internal static extern bool CloseDecompressor(IntPtr decompressorHandle);
    }
}

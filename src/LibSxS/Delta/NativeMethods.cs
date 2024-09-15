using System.Runtime.InteropServices;

namespace LibSxS.Delta
{
    internal static class NativeMethods
    {
        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool ApplyDeltaB(
           DeltaInputFlags ApplyFlags,
           DeltaInput Source,
           DeltaInput Delta,
           out DeltaOutput lpTarget);

        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool ApplyDeltaW(
           DeltaInputFlags ApplyFlags,
           string lpSourceName,
           string lpDeltaName,
           string lpTargetName);

        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetDeltaInfoB(
           DeltaInput Delta,
           out DeltaHeaderInfo lpHeaderInfo);

        [DllImport("msdelta.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetDeltaInfoW(
           string lpDeltaName,
           out DeltaHeaderInfo lpHeaderInfo);

        [DllImport("msdelta.dll", SetLastError = true)]
        internal static extern bool DeltaFree(IntPtr lpMemory);

        [DllImport("kernel32.dll")]
        internal static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, int NumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr lpOverlapped);
    }
}

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LibSxS.Delta
{
    public static class DeltaAPI
    {
        public static unsafe DeltaHeaderInfo GetDeltaFileInformation(string path)
        {
            byte[] delta;
            DeltaHeaderInfo info;

            using (FileStream fStr = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new((int)fStr.Length))
            {
                fStr.Position = 4;
                fStr.CopyTo(mStr);
                delta = mStr.ToArray();
            }

            fixed (byte* deltaPtr = delta)
            {
                DeltaInput deltaData = new()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = delta.Length,
                    Editable = false
                };

                bool success = NativeMethods.GetDeltaInfoB(deltaData, out info);

                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            return info;
        }

        public static unsafe Stream LoadManifest(Stream compressedManifest)
        {
            byte[] source, delta;
            bool success = false;

            using MemoryStream wcpBaseFileStream = new(Constants.wcpBaseBuffer);
            using MemoryStream wcpBaseStream = new((int)wcpBaseFileStream.Length);

            wcpBaseFileStream.CopyTo(wcpBaseStream);
            source = wcpBaseStream.ToArray();

            MemoryStream mStr = new((int)compressedManifest.Length);

            byte[] compTest = new byte[4];
            compressedManifest.Read(compTest, 0, 4);
            uint headerInt = BitConverter.ToUInt32(compTest, 0);

            if (headerInt == 0x3CBFBBEF || headerInt == 0x6D783F3C) // Decompressed XML starts
            {
                compressedManifest.Position = headerInt == 0x3CBFBBEF ? 3 : 0;
                compressedManifest.CopyTo(mStr);
                mStr.Position = 0x69;
                mStr.WriteByte(0x33);
                mStr.Position = 0;
                return mStr;
            }

            compressedManifest.CopyTo(mStr);
            delta = mStr.ToArray();

            mStr.Dispose();

            fixed (byte* sourcePtr = source)
            fixed (byte* deltaPtr = delta)
            {
                DeltaInput sourceData = new()
                {
                    lpStart = new IntPtr(sourcePtr),
                    uSize = source.Length,
                    Editable = false
                };

                DeltaInput deltaData = new()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = delta.Length,
                    Editable = false
                };

                success = NativeMethods.ApplyDeltaB(DeltaInputFlags.DELTA_FLAG_NONE, sourceData, deltaData, out DeltaOutput outData);

                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                byte[] output = new byte[outData.cbBuf.ToInt32()];
                Marshal.Copy(outData.pBuf, output, 0, output.Length);

                success = NativeMethods.DeltaFree(outData.pBuf);

                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                return new MemoryStream(output);
            }
        }

        public static unsafe void ApplyDelta(string basisPath, string patchPath, long patchOffset, int patchSize, string outputPath, bool allowPA19 = false)
        {
            byte[] source, delta;
            bool success = false;

            using (FileStream fStr = new(basisPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new((int)fStr.Length))
            {
                fStr.CopyTo(mStr);
                source = mStr.ToArray();
            }

            using (FileStream fStr = new(patchPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fStr.Position = patchOffset;
                delta = new byte[patchSize];
                fStr.Read(delta, 0, patchSize);
            }

            fixed (byte* sourcePtr = source)
            fixed (byte* deltaPtr = delta)
            {
                DeltaInput sourceData = new()
                {
                    lpStart = new IntPtr(sourcePtr),
                    uSize = source.Length,
                    Editable = false
                };

                DeltaInput deltaData = new()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = delta.Length,
                    Editable = false
                };

                success = NativeMethods.ApplyDeltaB(allowPA19 ? DeltaInputFlags.DELTA_APPLY_FLAG_ALLOW_PA19 : DeltaInputFlags.DELTA_FLAG_NONE,
                                      sourceData,
                                      deltaData,
                                      out DeltaOutput outData);

                if (!success)
                {
                    sourceData = new DeltaInput()
                    {
                        lpStart = IntPtr.Zero,
                        uSize = IntPtr.Zero,
                        Editable = false
                    };

                    success = NativeMethods.ApplyDeltaB(allowPA19 ? DeltaInputFlags.DELTA_APPLY_FLAG_ALLOW_PA19 : DeltaInputFlags.DELTA_FLAG_NONE,
                                          sourceData,
                                          deltaData,
                                          out outData);

                    if (!success)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }

                using FileStream fs = new(outputPath, FileMode.Create, FileAccess.Write);

                if (!NativeMethods.WriteFile(fs.SafeFileHandle.DangerousGetHandle(), outData.pBuf, outData.cbBuf.ToInt32(), out int written, IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                success = NativeMethods.DeltaFree(outData.pBuf);

                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        public static unsafe DeltaHeaderInfo GetDeltaFilePartInformation(string path, long patchOffset, int patchSize)
        {
            byte[] delta;
            DeltaHeaderInfo info;

            using (FileStream fStr = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream mStr = new((int)fStr.Length))
            {
                fStr.Position = patchOffset;
                delta = new byte[patchSize];
                fStr.Read(delta, 0, patchSize);
            }

            fixed (byte* deltaPtr = delta)
            {
                DeltaInput deltaData = new()
                {
                    lpStart = new IntPtr(deltaPtr),
                    uSize = delta.Length,
                    Editable = false
                };

                bool success = NativeMethods.GetDeltaInfoB(deltaData, out info);

                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            return info;
        }
    }
}

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Playground
{
    internal class Test
    {
        [DllImport("Wintrust.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CryptCATOpen(string pwszFileName, int fdwOpenFlags, IntPtr hProv, int dwPublicVersion, int dwEncodingType);

        [DllImport("Wintrust.dll", SetLastError = true)]
        public static extern bool CryptCATClose(IntPtr hCatalog);

        [DllImport("Wintrust.dll", SetLastError = true)]
        public static extern IntPtr CryptCATEnumerateMember(IntPtr hCatalog, IntPtr pPrevMember);

        [DllImport("Wintrust.dll", SetLastError = true)]
        public static extern IntPtr CryptCATEnumerateAttr(IntPtr hCatalog, IntPtr pCatMember, IntPtr pPrevAttr);

        public const int INVALID_HANDLE_VALUE = -1;

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPTCATMEMBER
        {
            public int cbStruct;           // = sizeof(CRYPTCATMEMBER)
            public IntPtr pwszReferenceTag;
            public IntPtr pwszFileName;       // used only by the CDF APIs
            public Guid gSubjectType;       // may be zeros -- see sEncodedMemberInfo
            public int fdwMemberFlags;
            public IntPtr pIndirectData;     // may be null -- see sEncodedIndirectData
            public int dwCertVersion;      // may be zero -- see sEncodedMemberInfo
            public int dwReserved;         // used by enum -- DO NOT USE!
            public IntPtr hReserved;          // pStack(attrs) (null if init) INTERNAL!

            public CRYPT_ATTR_BLOB sEncodedIndirectData;   // lazy decode
            public CRYPT_ATTR_BLOB sEncodedMemberInfo;     // lazy decode
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_ATTR_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIP_INDIRECT_DATA
        {
            public CRYPT_ATTRIBUTE_TYPE_VALUE Data;            // Encoded attribute
            public CRYPT_ALGORITHM_IDENTIFIER DigestAlgorithm; // Digest algorithm used to hash
            public CRYPT_HASH_BLOB Digest;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_ALGORITHM_IDENTIFIER
        {
            public IntPtr pszObjId;
            public CRYPT_OBJID_BLOB Parameters;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_ATTRIBUTE_TYPE_VALUE
        {
            public IntPtr pszObjId;
            public CRYPT_OBJID_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_OBJID_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPT_HASH_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPTCATATTRIBUTE
        {
            public int cbStruct;           // = sizeof(CRYPTCATATTRIBUTE)
            public IntPtr pwszReferenceTag;
            public int dwAttrTypeAndAction;
            public int cbValue;
            public IntPtr pbValue;           // encoded CAT_NAMEVALUE struct
            public int dwReserved;         // used by enum -- DO NOT USE!
        }


        internal static void Load(string catalog)
        {
            IntPtr hCatalog = CryptCATOpen(catalog, 0, IntPtr.Zero, 0, 0);

            if (hCatalog != INVALID_HANDLE_VALUE)
            {
                IntPtr pMember = IntPtr.Zero;

                while ((pMember = CryptCATEnumerateMember(hCatalog, pMember)) != IntPtr.Zero)
                {
                    CRYPTCATMEMBER pCRYPTCATMEMBER = (CRYPTCATMEMBER)Marshal.PtrToStructure(pMember, typeof(CRYPTCATMEMBER))!;
                    SIP_INDIRECT_DATA pSIP_INDIRECT_DATA = (SIP_INDIRECT_DATA)Marshal.PtrToStructure(pCRYPTCATMEMBER.pIndirectData, typeof(SIP_INDIRECT_DATA))!;

                    StringBuilder sbData = new();
                    IntPtr pData = pSIP_INDIRECT_DATA.Digest.pbData;

                    for (int i = 0; i < pSIP_INDIRECT_DATA.Digest.cbData; i++)
                    {
                        byte byteValue = Marshal.ReadByte(pData);
                        sbData.Append($"{byteValue:X}");
                        pData += 1;
                    }

                    Console.WriteLine($"Data = {sbData}");

                    IntPtr pAttr = IntPtr.Zero;

                    while ((pAttr = CryptCATEnumerateAttr(hCatalog, pMember, pAttr)) != IntPtr.Zero)
                    {
                        CRYPTCATATTRIBUTE pCRYPTCATATTRIBUTE = (CRYPTCATATTRIBUTE)Marshal.PtrToStructure(pAttr, typeof(CRYPTCATATTRIBUTE))!;
                        string sReferenceTag = Marshal.PtrToStringUni(pCRYPTCATATTRIBUTE.pwszReferenceTag)!;

                        Console.WriteLine($"\tReferenceTag = {sReferenceTag}");

                        StringBuilder sbValue = new();
                        IntPtr pValue = pCRYPTCATATTRIBUTE.pbValue;

                        for (int i = 0; i < pCRYPTCATATTRIBUTE.cbValue; i++)
                        {
                            byte byteValue = Marshal.ReadByte(pValue);
                            sbValue.Append(string.Format("{0:X}", byteValue));
                            pValue += 1;
                        }

                        Console.WriteLine($"\tValue = {sbValue}");
                    }
                }

                CryptCATClose(hCatalog);
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
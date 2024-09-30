using DiscUtils;
using System.IO.Compression;

namespace MobilePackageGen.GZip
{
    internal static class GZipDecompressor
    {
        public static Stream OpenFileAndDecompressAsGZip(this IFileSystem mainfileSystem, string vhdFileName)
        {
            Stream fd = new MemoryStream();

            using Stream input = mainfileSystem.OpenFile(vhdFileName, FileMode.Open, FileAccess.Read);
            using GZipStream csStream = new(input, CompressionMode.Decompress);

            byte[] buffer = new byte[4096];
            int nRead;
            while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fd.Write(buffer, 0, nRead);
            }

            fd.Seek(0, SeekOrigin.Begin);

            return fd;
        }
    }
}

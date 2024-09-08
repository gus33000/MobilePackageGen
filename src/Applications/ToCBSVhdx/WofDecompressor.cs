using DiscUtils.Ntfs;
using DiscUtils;

namespace ToCBS.Wof
{
    internal static class WofDecompressor
    {
        public static Stream OpenFileAndDecompressIfNeeded(this IFileSystem mainfileSystem, string vhdFileName)
        {
            if (mainfileSystem is NtfsFileSystem ntfsFileSystem)
            {
                DiscFileInfo fileInfo = mainfileSystem.GetFileInfo(vhdFileName);

                if ((fileInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    if (!ntfsFileSystem.GetAlternateDataStreams(vhdFileName).Contains("WofCompressedData"))
                    {
                        throw new Exception("No compressed data found for file marked as reparse point");
                    }

                    string wofStreamName = GetWofStreamName(vhdFileName);
                    Stream input = ntfsFileSystem.OpenFile(wofStreamName, FileMode.Open, FileAccess.Read);
                    return new WofDecompressorStream(input, (uint)fileInfo.Length);
                }
            }

            return mainfileSystem.OpenFile(vhdFileName, FileMode.Open, FileAccess.Read);
        }

        private static string GetWofStreamName(string vhdFileName)
        {
            return $"{vhdFileName}:WofCompressedData";
        }
    }
}

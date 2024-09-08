using Archives.DiscUtils;
using DiscUtils;
using SevenZipExtractor;
using System;
using System.IO;

namespace ToSPKG
{
    public class Partition
    {
        public string Name { get; }
        public Guid Type { get; }
        public Guid ID { get; }
        public long Size { get; }
        public IFileSystem? FileSystem { get; }
        public Stream Stream { get; }

        public Partition(Stream Stream, string Name, Guid Type, Guid ID, long Size)
        {
            this.Stream = Stream;
            this.Size = Stream.Length;
            this.Name = Name;
            this.Type = Type;
            this.ID = ID;
            this.FileSystem = TryCreateFileSystem(Stream);
        }

        public Partition(Stream Stream, IFileSystem FileSystem, string Name, Guid Type, Guid ID, long Size)
        {
            this.Stream = Stream;
            this.Size = Stream.Length;
            this.Name = Name;
            this.Type = Type;
            this.ID = ID;
            this.FileSystem = FileSystem;
        }

        private static IFileSystem? TryCreateFileSystem(Stream partitionStream)
        {
            try
            {
                partitionStream.Seek(0, SeekOrigin.Begin);
                if (DiscUtils.Ntfs.NtfsFileSystem.Detect(partitionStream))
                {
                    partitionStream.Seek(0, SeekOrigin.Begin);
                    return new DiscUtils.Ntfs.NtfsFileSystem(partitionStream);
                }
            }
            catch
            {
            }

            try
            {
                // DiscUtils fat implementation is a bit broken, on top of not supporting lfn (which we know how to fix)
                // It also fails to find the last chunk on some known partitions (like PLAT)
                // As a result, we use a bridge file system making use of 7z, it's slower, but it's the best we can do for now

                partitionStream.Seek(0x36, SeekOrigin.Begin);
                byte[] buf = new byte[3];
                partitionStream.Read(buf, 0, 3);
                if (buf[0] == 0x46 && buf[1] == 0x41 && buf[2] == 0x54)
                {
                    partitionStream.Seek(0, SeekOrigin.Begin);
                    return new ArchiveBridge(partitionStream, SevenZipFormat.Fat);
                }
            }
            catch
            {

            }

            return null;
        }
    }
}

using Archives.DiscUtils;
using DiscUtils;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using SevenZipExtractor;
using StorageSpace;

namespace MobilePackageGen.Adapters
{
    public class FileSystemPartition : IPartition
    {
        public string Name
        {
            get;
        }

        public Guid Type
        {
            get;
        }

        public Guid ID
        {
            get;
        }

        public long Size
        {
            get;
        }

        public IFileSystem? FileSystem
        {
            get;
        }

        public Stream Stream
        {
            get;
        }

        public FileSystemPartition(Stream Stream, string Name, Guid Type, Guid ID)
        {
            this.Stream = Stream;
            Size = Stream.Length;
            this.Name = Name;
            this.Type = Type;
            this.ID = ID;
            FileSystem = TryCreateFileSystem(Stream);
        }

        public FileSystemPartition(Stream Stream, IFileSystem FileSystem, string Name, Guid Type, Guid ID)
        {
            this.Stream = Stream;
            Size = Stream.Length;
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

        public static List<PartitionInfo> GetPartitions(VirtualDisk virtualDisk)
        {
            List<PartitionInfo> partitions = [];

            PartitionTable partitionTable = virtualDisk.Partitions;

            if (partitionTable != null)
            {
                foreach (PartitionInfo partitionInfo in partitionTable.Partitions)
                {
                    partitions.Add(partitionInfo);

                    if (partitionInfo.GuidType == new Guid("E75CAF8F-F680-4CEE-AFA3-B001E56EFC2D"))
                    {
                        Stream storageSpacePartitionStream = partitionInfo.Open();

                        StorageSpace.StorageSpace storageSpace = new(storageSpacePartitionStream);

                        Dictionary<int, string> disks = storageSpace.GetDisks();

                        foreach (KeyValuePair<int, string> disk in disks)
                        {
                            Space space = storageSpace.OpenDisk(disk.Key);

                            DiscUtils.Raw.Disk duVirtualDisk = new(space, Ownership.None, Geometry.FromCapacity(space.Length, 4096));
                            PartitionTable msPartitionTable = duVirtualDisk.Partitions;

                            if (msPartitionTable != null)
                            {
                                foreach (PartitionInfo storageSpacePartition in msPartitionTable.Partitions)
                                {
                                    partitions.Add(storageSpacePartition);
                                }
                            }
                        }
                    }
                }
            }

            return partitions;
        }
    }
}

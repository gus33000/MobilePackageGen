using DiscUtils.Partitions;
using DiscUtils.Streams;
using DiscUtils;
using Img2Ffu.Reader;
using StorageSpace;

namespace MobilePackageGen.Adapters.FullFlashUpdate
{
    public class Disk : IDisk
    {
        public IEnumerable<IPartition> Partitions
        {
            get;
        }

        public Disk(string ffuPath)
        {
            List<PartitionInfo> partitionInfos = GetPartitions(ffuPath);
            Partitions = GetPartitionStructures(partitionInfos);
        }

        public Disk(List<IPartition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<IPartition> GetPartitionStructures(List<PartitionInfo> partitionInfos)
        {
            List<IPartition> partitions = [];

            foreach (PartitionInfo partitionInfo in partitionInfos)
            {
                SparseStream partitionStream = partitionInfo.Open();
                IPartition partition = new FileSystemPartition(partitionStream, ((GuidPartitionInfo)partitionInfo).Name, ((GuidPartitionInfo)partitionInfo).GuidType, ((GuidPartitionInfo)partitionInfo).Identity);
                partitions.Add(partition);
            }

            return partitions;
        }

        public static List<IDisk> GetUpdateOSDisks(List<IDisk> disks)
        {
            List<IDisk> updateOSDisks = [];

            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    IDisk? updateOSDisk = GetUpdateOSDisk(partition);
                    if (updateOSDisk != null)
                    {
                        updateOSDisks.Add(updateOSDisk);
                    }
                }
            }

            return updateOSDisks;
        }

        public static IDisk? GetUpdateOSDisk(IPartition partition)
        {
            if (partition.FileSystem != null)
            {
                IFileSystem fileSystem = partition.FileSystem;
                try
                {
                    // Handle UpdateOS as well if found
                    if (fileSystem.FileExists("PROGRAMS\\UpdateOS\\UpdateOS.wim"))
                    {
                        List<IPartition> partitions = [];

                        Stream wimStream = fileSystem.OpenFile("PROGRAMS\\UpdateOS\\UpdateOS.wim", FileMode.Open, FileAccess.Read);
                        DiscUtils.Wim.WimFile wimFile = new(wimStream);

                        for (int i = 0; i < wimFile.ImageCount; i++)
                        {
                            IFileSystem wimFileSystem = wimFile.GetImage(i);
                            IPartition wimPartition = new FileSystemPartition(wimStream, wimFileSystem, $"{partition.Name}-UpdateOS-{i}", Guid.Empty, Guid.Empty);
                            partitions.Add(wimPartition);
                        }

                        Disk updateOSDisk = new(partitions);
                        return updateOSDisk;
                    }
                }
                catch
                {

                }
            }

            return null;
        }

        private static List<PartitionInfo> GetPartitions(string ffuPath)
        {
            List<PartitionInfo> partitions = [];
            for (int i = 0; i < FullFlashUpdateReaderStream.GetStoreCount(ffuPath); i++)
            {
                FullFlashUpdateReaderStream store = new(ffuPath, (ulong)i);

                long diskCapacity = store.Length;
                VirtualDisk virtualDisk = new DiscUtils.Raw.Disk(store, Ownership.None, Geometry.FromCapacity(diskCapacity, store.SectorSize));

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
                                OSPoolStream space = storageSpace.OpenDisk(disk.Key);

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
            }

            return partitions;
        }
    }
}

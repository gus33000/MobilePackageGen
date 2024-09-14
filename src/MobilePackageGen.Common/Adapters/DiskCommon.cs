using DiscUtils.Partitions;
using DiscUtils.Streams;
using DiscUtils;
using StorageSpace;
using DiscUtils.Wim;

namespace MobilePackageGen.Adapters
{
    public static class DiskCommon
    {
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
                        Console.WriteLine();

                        Console.WriteLine($"UpdateOS.wim {fileSystem.GetFileLength("PROGRAMS\\UpdateOS\\UpdateOS.wim")} WindowsImageFile");

                        List<IPartition> partitions = [];

                        Stream wimStream = fileSystem.OpenFile("PROGRAMS\\UpdateOS\\UpdateOS.wim", FileMode.Open, FileAccess.Read);
                        WimFile wimFile = new(wimStream);

                        for (int i = 0; i < wimFile.ImageCount; i++)
                        {
                            IFileSystem wimFileSystem = wimFile.GetImage(i);

                            Console.WriteLine($"- {i} WindowsImageIndex");

                            IPartition wimPartition = new FileSystemPartition(wimStream, wimFileSystem, $"{partition.Name}-UpdateOS-{i}", Guid.Empty, Guid.Empty);
                            partitions.Add(wimPartition);
                        }

                        Console.WriteLine();

                        Wim.Disk updateOSDisk = new(partitions);
                        return updateOSDisk;
                    }
                }
                catch
                {

                }
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
                        Console.WriteLine();

                        Console.WriteLine($"{((GuidPartitionInfo)partitionInfo).Name} {((GuidPartitionInfo)partitionInfo).Identity} {((GuidPartitionInfo)partitionInfo).GuidType} {((GuidPartitionInfo)partitionInfo).SectorCount * virtualDisk.Geometry!.Value.BytesPerSector} StoragePool");

                        Stream storageSpacePartitionStream = partitionInfo.Open();

                        Pool pool = new(storageSpacePartitionStream);

                        Dictionary<int, string> disks = pool.GetDisks();

                        foreach (KeyValuePair<int, string> disk in disks.OrderBy(x => x.Key))
                        {
                            Console.WriteLine($"- {disk.Key}: {disk.Value} StorageSpace");
                        }

                        Console.WriteLine();

                        foreach (KeyValuePair<int, string> disk in disks)
                        {
                            Space space = pool.OpenDisk(disk.Key);

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

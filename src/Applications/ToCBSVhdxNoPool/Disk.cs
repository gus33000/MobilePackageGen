using DiscUtils.Partitions;
using DiscUtils.Streams;
using DiscUtils;
using ToCBS.Wof;

namespace ToCBS
{
    public class Disk
    {
        public List<Partition> Partitions { get; }

        public Disk(string vhdx, uint SectorSize)
        {
            List<PartitionInfo> partitionInfos = GetPartitions(vhdx);
            Partitions = GetPartitionStructures(partitionInfos, SectorSize);
        }

        public Disk(List<Partition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<Partition> GetPartitionStructures(List<PartitionInfo> partitionInfos, uint SectorSize)
        {
            List<Partition> partitions = new();

            foreach (PartitionInfo partitionInfo in partitionInfos)
            {
                SparseStream partitionStream = partitionInfo.Open();
                Partition partition = new(partitionStream, ((GuidPartitionInfo)partitionInfo).Name, ((GuidPartitionInfo)partitionInfo).GuidType, ((GuidPartitionInfo)partitionInfo).Identity, ((GuidPartitionInfo)partitionInfo).SectorCount * SectorSize);
                partitions.Add(partition);
            }

            return partitions;
        }

        public static List<Disk> GetUpdateOSDisks(List<Disk> disks)
        {
            List<Disk> updateOSDisks = new();

            foreach (Disk disk in disks)
            {
                foreach (Partition partition in disk.Partitions)
                {
                    Disk? updateOSDisk = GetUpdateOSDisk(partition);
                    if (updateOSDisk != null)
                    {
                        updateOSDisks.Add(updateOSDisk);
                    }
                }
            }

            return updateOSDisks;
        }

        public static Disk? GetUpdateOSDisk(Partition partition)
        {
            if (partition.FileSystem != null)
            {
                IFileSystem fileSystem = partition.FileSystem;
                try
                {
                    // Handle UpdateOS as well if found
                    if (fileSystem.FileExists("PROGRAMS\\UpdateOS\\UpdateOS.wim"))
                    {
                        List<Partition> partitions = new();

                        Stream wimStream = fileSystem.OpenFileAndDecompressIfNeeded("PROGRAMS\\UpdateOS\\UpdateOS.wim");
                        DiscUtils.Wim.WimFile wimFile = new(wimStream);

                        for (int i = 0; i < wimFile.ImageCount; i++)
                        {
                            IFileSystem wimFileSystem = wimFile.GetImage(i);
                            Partition wimPartition = new(wimStream, wimFileSystem, $"{partition.Name}-UpdateOS-{i}", Guid.Empty, Guid.Empty, wimStream.Length);
                            partitions.Add(wimPartition);
                        }

                        Disk updateOSDisk = new Disk(partitions);
                        return updateOSDisk;
                    }
                }
                catch
                {

                }
            }

            return null;
        }

        private static List<PartitionInfo> GetPartitions(string vhdx)
        {
            List<PartitionInfo> partitions = new();

            bool hasOsPool = false;

            VirtualDisk virtualDisk = null;
            if (vhdx.EndsWith(".vhd", StringComparison.InvariantCultureIgnoreCase))
            {
                virtualDisk = new DiscUtils.Vhd.Disk(vhdx, FileAccess.Read);
            }
            else
            {
                virtualDisk = new DiscUtils.Vhdx.Disk(vhdx, FileAccess.Read);
            }

            PartitionTable partitionTable = virtualDisk.Partitions;

            if (partitionTable != null)
            {
                foreach (PartitionInfo partitionInfo in partitionTable.Partitions)
                {
                    partitions.Add(partitionInfo);
                    if (partitionInfo.GuidType == new Guid("E75CAF8F-F680-4CEE-AFA3-B001E56EFC2D"))
                    {
                        hasOsPool = true;
                    }
                }
            }

            if (hasOsPool)
            {
                throw new Exception("Image contains an OSPool which is unsupported by this program!");
            }

            return partitions;
        }
    }
}

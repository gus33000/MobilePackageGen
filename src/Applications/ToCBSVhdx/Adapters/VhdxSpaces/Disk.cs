using DiscUtils.Partitions;
using DiscUtils.Streams;
using DiscUtils;
using ToCBS.Wof;
using Microsoft.Spaces.Diskstream;
using System.ComponentModel;

namespace ToCBS.Adapters.VhdxSpaces
{
    public class Disk : IDisk
    {
        public IEnumerable<IPartition> Partitions
        {
            get;
        }

        public Disk(string vhdx, uint SectorSize)
        {
            List<PartitionInfo> partitionInfos = GetPartitions(vhdx);
            Partitions = GetPartitionStructures(partitionInfos, SectorSize);
        }

        public Disk(List<IPartition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<IPartition> GetPartitionStructures(List<PartitionInfo> partitionInfos, uint SectorSize)
        {
            List<IPartition> partitions = [];

            foreach (PartitionInfo partitionInfo in partitionInfos)
            {
                SparseStream partitionStream = partitionInfo.Open();
                Partition partition = new(partitionStream, ((GuidPartitionInfo)partitionInfo).Name, ((GuidPartitionInfo)partitionInfo).GuidType, ((GuidPartitionInfo)partitionInfo).Identity, ((GuidPartitionInfo)partitionInfo).SectorCount * SectorSize);
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
            List<PartitionInfo> partitions = [];

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
                try
                {
                    Microsoft.Spaces.Diskstream.Disk msVirtualDisk = Vhd.Open(vhdx, true, null);
                    Pool pool = Pool.Open(msVirtualDisk);
                    foreach (Space space in pool.Spaces)
                    {
                        DiscUtils.Raw.Disk duVirtualDisk = new(space, Ownership.None, Geometry.FromCapacity(space.Length, space.BytesPerSector));
                        PartitionTable msPartitionTable = duVirtualDisk.Partitions;

                        if (msPartitionTable != null)
                        {
                            foreach (PartitionInfo sspartition in msPartitionTable.Partitions)
                            {
                                partitions.Add(sspartition);
                            }
                        }
                    }
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode != 1168)
                    {
                        throw;
                    }
                }
            }

            return partitions;
        }
    }
}

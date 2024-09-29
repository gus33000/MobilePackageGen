using DiscUtils;
using DiscUtils.Partitions;
using DiscUtils.Streams;

namespace MobilePackageGen.Adapters.Vhdx
{
    public class Disk : IDisk
    {
        public IEnumerable<IPartition> Partitions
        {
            get;
        }

        public Disk(string vhdx)
        {
            Logging.Log();

            Logging.Log($"{Path.GetFileName(vhdx)} {new FileInfo(vhdx).Length} VirtualHardDisk");

            (IEnumerable<PartitionInfo>, int, Stream) partitionInfos = GetPartitions(vhdx);
            Partitions = GetPartitionStructures(partitionInfos.Item1, partitionInfos.Item2, partitionInfos.Item3);

            Logging.Log();
        }

        public Disk(List<IPartition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<IPartition> GetPartitionStructures(IEnumerable<PartitionInfo> partitionInfos, int SectorSize, Stream _diskData)
        {
            List<IPartition> partitions = [];

            foreach (PartitionInfo partitionInfo in partitionInfos)
            {
                SparseStream partitionStream = Open(partitionInfo, SectorSize, _diskData);
                IPartition partition = new FileSystemPartition(partitionStream, ((GuidPartitionInfo)partitionInfo).Name, ((GuidPartitionInfo)partitionInfo).GuidType, ((GuidPartitionInfo)partitionInfo).Identity);
                partitions.Add(partition);
            }

            return partitions;
        }

        private static SparseStream Open(PartitionInfo entry, int SectorSize, Stream _diskData)
        {
            long start = entry.FirstSector * SectorSize;
            long end = (entry.LastSector + 1) * SectorSize;

            if (end >= _diskData.Length)
            {
                end = _diskData.Length;
            }

            return new SubStream(_diskData, start, end - start);
        }

        private static (IEnumerable<PartitionInfo>, int, Stream) GetPartitions(string vhdx)
        {
            VirtualDisk virtualDisk;
            if (vhdx.EndsWith(".vhd", StringComparison.InvariantCultureIgnoreCase))
            {
                virtualDisk = new DiscUtils.Vhd.Disk(vhdx, FileAccess.Read);
            }
            else
            {
                virtualDisk = new DiscUtils.Vhdx.Disk(vhdx, FileAccess.Read);
            }

            return (DiskCommon.GetPartitions(virtualDisk), virtualDisk.Geometry!.Value.BytesPerSector, virtualDisk.Content);
        }
    }
}

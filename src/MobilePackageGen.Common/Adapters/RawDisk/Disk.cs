using DiscUtils.Partitions;
using DiscUtils.Streams;
using DiscUtils;

namespace MobilePackageGen.Adapters.RawDisk
{
    public class Disk : IDisk
    {
        public IEnumerable<IPartition> Partitions
        {
            get;
        }

        public Disk(Stream diskStream)
        {
            Logging.Log();

            List<PartitionInfo> partitionInfos = GetPartitions(diskStream);
            Partitions = GetPartitionStructures(partitionInfos);

            Logging.Log();
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

        private static List<PartitionInfo> GetPartitions(Stream diskStream)
        {
            VirtualDisk virtualDisk = new DiscUtils.Raw.Disk(diskStream, Ownership.None);
            return DiskCommon.GetPartitions(virtualDisk);
        }
    }
}

using DiscUtils.Partitions;
using DiscUtils.Streams;
using DiscUtils;
using Img2Ffu.Reader;
using MobilePackageGen.Common;

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
            Console.WriteLine();

            Console.WriteLine($"{Path.GetFileName(ffuPath)} {new FileInfo(ffuPath).Length} FullFlashUpdate");

            List<PartitionInfo> partitionInfos = GetPartitions(ffuPath);
            Partitions = GetPartitionStructures(partitionInfos);

            Console.WriteLine();
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

        private static List<PartitionInfo> GetPartitions(string ffuPath)
        {
            List<PartitionInfo> partitions = [];
            for (int i = 0; i < FullFlashUpdateReaderStream.GetStoreCount(ffuPath); i++)
            {
                FullFlashUpdateReaderStream store = new(ffuPath, (ulong)i);

                string FriendlyDevicePath = DevicePathUtils.FormatDevicePath(store.DevicePath);

                Console.WriteLine($"- {i}: {FriendlyDevicePath} {store.Length} FullFlashUpdateStore");

                long diskCapacity = store.Length;
                VirtualDisk virtualDisk = new DiscUtils.Raw.Disk(store, Ownership.None, Geometry.FromCapacity(diskCapacity, store.SectorSize));

                partitions.AddRange(DiskCommon.GetPartitions(virtualDisk));
            }

            return partitions;
        }
    }
}
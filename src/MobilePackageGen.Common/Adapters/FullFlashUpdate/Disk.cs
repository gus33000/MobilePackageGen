using DiscUtils.Partitions;
using DiscUtils.Streams;
using DiscUtils;
using Img2Ffu.Reader;

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
            Logging.Log();

            Logging.Log($"{Path.GetFileName(ffuPath)} {new FileInfo(ffuPath).Length} FullFlashUpdate");

            List<(IEnumerable<PartitionInfo>, int, Stream)> partitionInfos = GetPartitions(ffuPath);

            List<IPartition> Partitions = [];

            foreach ((IEnumerable<PartitionInfo>, int, Stream) partitionInfo in partitionInfos)
            {
                Partitions.AddRange(GetPartitionStructures(partitionInfo.Item1, partitionInfo.Item2, partitionInfo.Item3));
            }

            this.Partitions = Partitions;

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

        private static List<(IEnumerable<PartitionInfo>, int, Stream)> GetPartitions(string ffuPath)
        {
            List<(IEnumerable<PartitionInfo>, int, Stream)> partitions = [];
            for (int i = 0; i < FullFlashUpdateReaderStream.GetStoreCount(ffuPath); i++)
            {
                FullFlashUpdateReaderStream store = new(ffuPath, (ulong)i);

                string FriendlyDevicePath = DevicePathUtils.FormatDevicePath(store.DevicePath);

                Logging.Log($"- {i}: {FriendlyDevicePath} {store.Length} FullFlashUpdateStore");

                long diskCapacity = store.Length;
                VirtualDisk virtualDisk = new DiscUtils.Raw.Disk(store, Ownership.None, Geometry.FromCapacity(diskCapacity, store.SectorSize));

                partitions.Add((DiskCommon.GetPartitions(virtualDisk), store.SectorSize, store));
            }

            return partitions;
        }
    }
}
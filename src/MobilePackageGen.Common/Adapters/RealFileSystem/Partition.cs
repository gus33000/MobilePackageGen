using DiscUtils;

namespace MobilePackageGen.Adapters.RealFileSystem
{
    public class Partition : IPartition
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
        public Stream Stream => GetPartitionStream(Name, GetPartitions());

        public Partition(IFileSystem FileSystem, string Name, Guid Type, Guid ID, long Size)
        {
            this.Size = Stream.Length;
            this.Name = Name;
            this.Type = Type;
            this.ID = ID;
            this.FileSystem = FileSystem;
        }

        private static List<PartitionInfo> GetPartitions()
        {
            return DiskPartitionUtils.GetDiskDetail();
        }

        private static Stream GetPartitionStream(string PartitionName, List<PartitionInfo> partitions)
        {
            string file = TempManager.GetTempFile();
            DiskPartitionUtils.ExtractFromDiskAndCopy(partitions, PartitionName, file);
            return File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}

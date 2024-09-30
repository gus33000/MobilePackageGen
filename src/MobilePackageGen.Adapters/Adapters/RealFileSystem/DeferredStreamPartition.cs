using DiscUtils;

namespace MobilePackageGen.Adapters.RealFileSystem
{
    public class DeferredStreamPartition : IPartition
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

        public DeferredStreamPartition(IFileSystem FileSystem, string Name, Guid Type, Guid ID)
        {
            Size = Stream.Length;
            this.Name = Name;
            this.Type = Type;
            this.ID = ID;
            this.FileSystem = FileSystem;
        }

        private static List<PartitionInfo> GetPartitions()
        {
            return DiskPartitionUtils.GetDiskDetail();
        }

        private static FileStream GetPartitionStream(string PartitionName, List<PartitionInfo> partitions)
        {
            string file = TempManager.GetTempFile();
            DiskPartitionUtils.ExtractFromDiskAndCopy(partitions, PartitionName, file);
            return File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}

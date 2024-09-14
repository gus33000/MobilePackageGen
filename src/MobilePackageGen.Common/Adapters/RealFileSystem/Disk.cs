using DiscUtils;

namespace MobilePackageGen.Adapters.RealFileSystem
{
    public class Disk : IDisk
    {
        public IEnumerable<IPartition> Partitions
        {
            get;
        }

        public Disk(string path)
        {
            Partitions = GetPartitionStructures(path);
        }

        public Disk(List<IPartition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<IPartition> GetPartitionStructures(string path)
        {
            List<IPartition> partitions = [];

            DeferredStreamPartition partition = new(new RealFileSystemBridge(path), path.Replace(":", "").Replace(Path.DirectorySeparatorChar, '_'), Guid.Empty, Guid.Empty);
            partitions.Add(partition);

            return partitions;
        }
    }
}

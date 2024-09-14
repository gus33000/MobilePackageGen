using DiscUtils;

namespace MobilePackageGen.Adapters.Wim
{
    public class Disk : IDisk
    {
        public IEnumerable<IPartition> Partitions
        {
            get;
        }

        public Disk(string wimPath)
        {
            Partitions = GetPartitionStructures(wimPath);
        }

        public Disk(List<IPartition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<IPartition> GetPartitionStructures(string path)
        {
            List<IPartition> partitions = [];

            Stream wimStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            DiscUtils.Wim.WimFile wimFile = new(wimStream);

            for (int i = 0; i < wimFile.ImageCount; i++)
            {
                IFileSystem wimFileSystem = wimFile.GetImage(i);
                IPartition wimPartition = new FileSystemPartition(wimStream, wimFileSystem, $"{Path.GetFileNameWithoutExtension(path)}-{i}", Guid.Empty, Guid.Empty);
                partitions.Add(wimPartition);
            }

            return partitions;
        }
    }
}

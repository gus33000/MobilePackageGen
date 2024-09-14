using DiscUtils;
using DiscUtils.Wim;

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
            Console.WriteLine();

            Console.WriteLine($"{Path.GetFileName(wimPath)} {new FileInfo(wimPath).Length} WindowsImageFile");

            Partitions = GetPartitionStructures(wimPath);

            Console.WriteLine();
        }

        public Disk(List<IPartition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<IPartition> GetPartitionStructures(string path)
        {
            List<IPartition> partitions = [];

            Stream wimStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            WimFile wimFile = new(wimStream);

            for (int i = 0; i < wimFile.ImageCount; i++)
            {
                IFileSystem wimFileSystem = wimFile.GetImage(i);

                Console.WriteLine($"- {i} WindowsImageIndex");

                IPartition wimPartition = new FileSystemPartition(wimStream, wimFileSystem, $"{Path.GetFileNameWithoutExtension(path)}-{i}", Guid.Empty, Guid.Empty);
                partitions.Add(wimPartition);
            }

            Console.WriteLine();

            return partitions;
        }
    }
}

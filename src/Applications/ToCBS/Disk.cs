using DiscUtils;
using ToCBS.Wof;

namespace ToCBS
{
    public class Disk
    {
        public List<Partition> Partitions { get; }

        public Disk(string path)
        {
            Partitions = GetPartitionStructures(path);
        }

        public Disk(List<Partition> Partitions)
        {
            this.Partitions = Partitions;
        }

        private static List<Partition> GetPartitionStructures(string path)
        {
            List<Partition> partitions = new();

            Partition partition = new(new RealFileSystemBridge(path), path.Replace(":", "").Replace(Path.DirectorySeparatorChar, '_'), Guid.Empty, Guid.Empty, 0);
            partitions.Add(partition);

            return partitions;
        }

        public static List<Disk> GetUpdateOSDisks(List<Disk> disks)
        {
            List<Disk> updateOSDisks = new();

            foreach (Disk disk in disks)
            {
                foreach (Partition partition in disk.Partitions)
                {
                    Disk? updateOSDisk = GetUpdateOSDisk(partition);
                    if (updateOSDisk != null)
                    {
                        updateOSDisks.Add(updateOSDisk);
                    }
                }
            }

            return updateOSDisks;
        }

        public static Disk? GetUpdateOSDisk(Partition partition)
        {
            if (partition.FileSystem != null)
            {
                IFileSystem fileSystem = partition.FileSystem;
                try
                {
                    // Handle UpdateOS as well if found
                    if (fileSystem.FileExists("PROGRAMS\\UpdateOS\\UpdateOS.wim"))
                    {
                        List<Partition> partitions = new();

                        Stream wimStream = fileSystem.OpenFileAndDecompressIfNeeded("PROGRAMS\\UpdateOS\\UpdateOS.wim");
                        DiscUtils.Wim.WimFile wimFile = new(wimStream);

                        for (int i = 0; i < wimFile.ImageCount; i++)
                        {
                            IFileSystem wimFileSystem = wimFile.GetImage(i);
                            Partition wimPartition = new(wimFileSystem, $"{partition.Name}-UpdateOS-{i}", Guid.Empty, Guid.Empty, wimStream.Length);
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
    }
}

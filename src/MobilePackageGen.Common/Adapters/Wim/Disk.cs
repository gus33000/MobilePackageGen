﻿using DiscUtils;
using MobilePackageGen.Wof;

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

        public static List<IDisk> GetUpdateOSDisks(List<IDisk> disks)
        {
            List<IDisk> updateOSDisks = [];

            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    IDisk? updateOSDisk = GetUpdateOSDisk(partition);
                    if (updateOSDisk != null)
                    {
                        updateOSDisks.Add(updateOSDisk);
                    }
                }
            }

            return updateOSDisks;
        }

        public static IDisk? GetUpdateOSDisk(IPartition partition)
        {
            if (partition.FileSystem != null)
            {
                IFileSystem fileSystem = partition.FileSystem;
                try
                {
                    // Handle UpdateOS as well if found
                    if (fileSystem.FileExists("PROGRAMS\\UpdateOS\\UpdateOS.wim"))
                    {
                        List<IPartition> partitions = [];

                        Stream wimStream = fileSystem.OpenFileAndDecompressIfNeeded("PROGRAMS\\UpdateOS\\UpdateOS.wim");
                        DiscUtils.Wim.WimFile wimFile = new(wimStream);

                        for (int i = 0; i < wimFile.ImageCount; i++)
                        {
                            IFileSystem wimFileSystem = wimFile.GetImage(i);
                            IPartition wimPartition = new FileSystemPartition(wimStream, wimFileSystem, $"{partition.Name}-UpdateOS-{i}", Guid.Empty, Guid.Empty);
                            partitions.Add(wimPartition);
                        }

                        IDisk updateOSDisk = new Disk(partitions);
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

using DiscUtils;
using DiscUtils.Internal;
using DiscUtils.Ntfs;
using DiscUtils.Ntfs.Internals;

namespace MobilePackageGen
{
    public static class IFileSystemExtensions
    {
        public static IEnumerable<string> GetFilesWithNtfsIssueWorkaround(this IFileSystem fileSystem, string path, string searchPattern, SearchOption searchOption)
        {
            if (fileSystem is not NtfsFileSystem ntfsFileSystem)
            {
                return fileSystem.GetFiles(path, searchPattern, searchOption);
            }

            List<string> files = [];
            HashSet<string> fileMap = [];

            // Build the whole file map
            MasterFileTable table = ntfsFileSystem.GetMasterFileTable();
            ClusterMap clusterMap = table.GetClusterMap();

            IEnumerable<MasterFileTableEntry> fileEntries = table.GetEntries(EntryStates.InUse);

            foreach (MasterFileTableEntry fileEntry in fileEntries)
            {
                if (fileEntry.Flags.HasFlag(MasterFileTableEntryFlags.IsDirectory))
                {
                    continue;
                }

                long recordIndex = fileEntry.Index;
                IList<string> filePaths = clusterMap.FileIdToPaths(recordIndex);
                foreach (string filePath in filePaths)
                {
                    fileMap.Add(filePath);
                }
            }

            Func<string, bool>? re = null;

            if (!string.IsNullOrEmpty(searchPattern))
            {
                re = Utilities.ConvertWildcardsToRegEx(searchPattern, true);
            }

            IEnumerable<string> prereq = fileMap
                .Where(x => x.StartsWith(path, StringComparison.InvariantCultureIgnoreCase) && (re == null || re(x)));

            switch (searchOption)
            {
                case SearchOption.AllDirectories:
                    {
                        return prereq.ToArray();
                    }
                case SearchOption.TopDirectoryOnly:
                    {
                        int expectedCount = path.Count(x => x == '\\');
                        if (!path.EndsWith('\\'))
                        {
                            expectedCount++;
                        }

                        return prereq.Where(x => x.Count(x => x == '\\') == expectedCount).ToArray();
                    }
            }

            return [];
        }
    }
}

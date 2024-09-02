using DiscUtils.Partitions;
using DiscUtils;
using Microsoft.Deployment.Compression.Cab;
using Microsoft.Deployment.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DiscUtils.Streams;
using ToCBS.Wof;
using SevenZipExtractor;
using Archives.DiscUtils;
using FfuStream;

namespace ToCBS
{
    internal class CBSBuilder
    {
        private static List<CabinetFileInfo> GetCabinetFileInfoForCbsPackage(XmlMum.Assembly cbs, IFileSystem fileSystem, List<PartitionInfo> partitions)
        {
            List<CabinetFileInfo> fileMappings = new();

            string packages_path = @"Windows\servicing\Packages";
            string winsxs_manifests_path = @"Windows\WinSxS\Manifests";

            string packageName = $"{cbs.AssemblyIdentity.Name}~{cbs.AssemblyIdentity.PublicKeyToken}~{cbs.AssemblyIdentity.ProcessorArchitecture}~{(cbs.AssemblyIdentity.Language == "neutral" ? "" : cbs.AssemblyIdentity.Language)}~{cbs.AssemblyIdentity.Version}";

            string componentStatus = $"Processing {packageName} - Analyzing Package Files";
            Console.WriteLine(componentStatus);

            int i = 0;

            int oldPercentage = -1;

            foreach (XmlMum.File packageFile in cbs.Package.CustomInformation.File)
            {
                int percentage = i++ * 100 / cbs.Package.CustomInformation.File.Count;
                if (percentage != oldPercentage)
                {
                    oldPercentage = percentage;
                    string progressBarString = GetDismLikeProgBar(percentage);
                    Console.Write($"\r{progressBarString}");
                }

                // File names in cab files are all lower case
                string fileName = packageFile.Name.ToLower();

                // Prevent getting files from root of this program
                if (fileName.StartsWith("\\"))
                {
                    fileName = fileName[1..];
                }

                // If a manifest file is without any path, it must be retrieved from the manifest directory
                if (!fileName.Contains('\\') && fileName.EndsWith(".manifest"))
                {
                    fileName = Path.Combine(winsxs_manifests_path, fileName);
                }

                // We now replace macros with known values
                string normalized = fileName.Replace("$(runtime.bootdrive)", "")
                                                    .Replace("$(runtime.systemroot)", "windows")
                                                    .Replace("$(runtime.fonts)", @"windows\fonts")
                                                    .Replace("$(runtime.inf)", @"windows\inf")
                                                    .Replace("$(runtime.system)", @"windows\system")
                                                    .Replace("$(runtime.system32)", @"windows\system32")
                                                    .Replace("$(runtime.wbem)", @"windows\system32\wbem")
                                                    .Replace("$(runtime.drivers)", @"windows\system32\drivers");

                // Prevent getting files from root of this program
                if (normalized.StartsWith("\\"))
                {
                    normalized = normalized[1..];
                }

                // The package name is renamed to "update" in cab files, fix this
                if (normalized.EndsWith("update.mum"))
                {
                    normalized = normalized.Replace("update.mum", $"{packageName}.mum");

                    if (!normalized.Contains('\\'))
                    {
                        normalized = Path.Combine(packages_path, normalized);
                    }
                }

                // Same here for the catalog
                if (normalized.EndsWith("update.cat"))
                {
                    normalized = normalized.Replace("update.cat", $"{packageName}.cat");

                    if (!normalized.Contains('\\'))
                    {
                        normalized = Path.Combine(packages_path, normalized);
                    }
                }

                // For specific wow sub architecures, we want to fetch the files from the right place on the file system
                string architecture = cbs.Package.Update?.Component?.AssemblyIdentity?.ProcessorArchitecture;

                if (normalized.StartsWith(@"windows\system32") && architecture?.Contains("arm64.arm") == true)
                {
                    string newpath = normalized.Replace(@"windows\system32", @"windows\sysarm32");
                    if (fileSystem.FileExists(newpath))
                    {
                        normalized = newpath;
                    }
                }

                // Prevent getting files from root of this program
                if (normalized.StartsWith("\\"))
                {
                    normalized = normalized[1..];
                }

                if (!fileSystem.Exists(normalized) && normalized.EndsWith(".manifest"))
                {
                    normalized = Path.Combine(winsxs_manifests_path, normalized.Split("\\")[^1]);
                }

                CabinetFileInfo cabinetFileInfo = null;

                // If we end in bin, and the package is marked binary partition, this is a partition on one of the device disks, retrieve it
                if (normalized.EndsWith(".bin") && cbs.Package.BinaryPartition.ToLower() == "true")
                {
                    foreach (PartitionInfo element in partitions)
                    {
                        if (element.VolumeType == PhysicalVolumeType.GptPartition)
                        {
                            GuidPartitionInfo guidPartitionInfo = (GuidPartitionInfo)element;
                            if (guidPartitionInfo.Name.Equals(cbs.Package.TargetPartition, StringComparison.InvariantCultureIgnoreCase))
                            {
                                cabinetFileInfo = new CabinetFileInfo()
                                {
                                    FileName = packageFile.Cabpath,
                                    FileStream = new Substream(element.Open(), long.Parse(packageFile.Size)),
                                    Attributes = FileAttributes.Normal,
                                    DateTime = DateTime.Now
                                };
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (!fileSystem.FileExists(normalized))
                    {
                        if (normalized.StartsWith("data\\", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (PartitionInfo partition in partitions)
                            {
                                if (partition.VolumeType == PhysicalVolumeType.GptPartition)
                                {
                                    GuidPartitionInfo guidPartitionInfo = (GuidPartitionInfo)partition;

                                    if (guidPartitionInfo.Name.Equals("data", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        SparseStream partitionStream = partition.Open();

                                        IFileSystem? fileSystemData = TryCreateFileSystem(partitionStream);

                                        if (fileSystemData == null)
                                        {
                                            partitionStream.Dispose();
                                            break;
                                        }

                                        cabinetFileInfo = new CabinetFileInfo()
                                        {
                                            FileName = packageFile.Cabpath,
                                            FileStream = fileSystemData.OpenFileAndDecompressIfNeeded(normalized[5..]),
                                            Attributes = fileSystemData.GetAttributes(normalized[5..]) & ~FileAttributes.ReparsePoint,
                                            DateTime = fileSystemData.GetLastWriteTime(normalized[5..])
                                        };

                                        break;
                                    }
                                }
                            }
                        }

                        if (normalized.StartsWith("efiesp\\", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (PartitionInfo partition in partitions)
                            {
                                if (partition.VolumeType == PhysicalVolumeType.GptPartition)
                                {
                                    GuidPartitionInfo guidPartitionInfo = (GuidPartitionInfo)partition;

                                    if (guidPartitionInfo.Name.Equals("efiesp", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        SparseStream partitionStream = partition.Open();

                                        IFileSystem? fileSystemData = TryCreateFileSystem(partitionStream);

                                        if (fileSystemData == null)
                                        {
                                            partitionStream.Dispose();
                                            break;
                                        }

                                        cabinetFileInfo = new CabinetFileInfo()
                                        {
                                            FileName = packageFile.Cabpath,
                                            FileStream = fileSystemData.OpenFileAndDecompressIfNeeded(normalized[5..]),
                                            Attributes = fileSystemData.GetAttributes(normalized[5..]) & ~FileAttributes.ReparsePoint,
                                            DateTime = fileSystemData.GetLastWriteTime(normalized[5..])
                                        };

                                        break;
                                    }
                                }
                            }
                        }

                        if (normalized.StartsWith("osdata\\", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (PartitionInfo partition in partitions)
                            {
                                if (partition.VolumeType == PhysicalVolumeType.GptPartition)
                                {
                                    GuidPartitionInfo guidPartitionInfo = (GuidPartitionInfo)partition;

                                    if (guidPartitionInfo.Name.Equals("osdata", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        SparseStream partitionStream = partition.Open();

                                        IFileSystem? fileSystemData = TryCreateFileSystem(partitionStream);

                                        if (fileSystemData == null)
                                        {
                                            partitionStream.Dispose();
                                            break;
                                        }

                                        cabinetFileInfo = new CabinetFileInfo()
                                        {
                                            FileName = packageFile.Cabpath,
                                            FileStream = fileSystemData.OpenFileAndDecompressIfNeeded(normalized[5..]),
                                            Attributes = fileSystemData.GetAttributes(normalized[5..]) & ~FileAttributes.ReparsePoint,
                                            DateTime = fileSystemData.GetLastWriteTime(normalized[5..])
                                        };

                                        break;
                                    }
                                }
                            }
                        }

                        if (normalized.StartsWith("dpp\\", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (PartitionInfo partition in partitions)
                            {
                                if (partition.VolumeType == PhysicalVolumeType.GptPartition)
                                {
                                    GuidPartitionInfo guidPartitionInfo = (GuidPartitionInfo)partition;

                                    if (guidPartitionInfo.Name.Equals("dpp", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        SparseStream partitionStream = partition.Open();

                                        IFileSystem? fileSystemData = TryCreateFileSystem(partitionStream);

                                        if (fileSystemData == null)
                                        {
                                            partitionStream.Dispose();
                                            break;
                                        }

                                        cabinetFileInfo = new CabinetFileInfo()
                                        {
                                            FileName = packageFile.Cabpath,
                                            FileStream = fileSystemData.OpenFileAndDecompressIfNeeded(normalized[5..]),
                                            Attributes = fileSystemData.GetAttributes(normalized[5..]) & ~FileAttributes.ReparsePoint,
                                            DateTime = fileSystemData.GetLastWriteTime(normalized[5..])
                                        };

                                        break;
                                    }
                                }
                            }
                        }

                        if (normalized.StartsWith("mmos\\", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (PartitionInfo partition in partitions)
                            {
                                if (partition.VolumeType == PhysicalVolumeType.GptPartition)
                                {
                                    GuidPartitionInfo guidPartitionInfo = (GuidPartitionInfo)partition;

                                    if (guidPartitionInfo.Name.Equals("mmos", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        SparseStream partitionStream = partition.Open();

                                        IFileSystem? fileSystemData = TryCreateFileSystem(partitionStream);

                                        if (fileSystemData == null)
                                        {
                                            partitionStream.Dispose();
                                            break;
                                        }

                                        cabinetFileInfo = new CabinetFileInfo()
                                        {
                                            FileName = packageFile.Cabpath,
                                            FileStream = fileSystemData.OpenFileAndDecompressIfNeeded(normalized[5..]),
                                            Attributes = fileSystemData.GetAttributes(normalized[5..]) & ~FileAttributes.ReparsePoint,
                                            DateTime = fileSystemData.GetLastWriteTime(normalized[5..])
                                        };

                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        cabinetFileInfo = new CabinetFileInfo()
                        {
                            FileName = packageFile.Cabpath,
                            FileStream = fileSystem.OpenFileAndDecompressIfNeeded(normalized),
                            Attributes = fileSystem.GetAttributes(normalized) & ~FileAttributes.ReparsePoint,
                            DateTime = fileSystem.GetLastWriteTime(normalized)
                        };
                    }
                }

                if (cabinetFileInfo != null)
                {
                    fileMappings.Add(cabinetFileInfo);
                }
                else
                {
                    //throw new FileNotFoundException(normalized);
                }
            }

            Console.WriteLine($"\r{GetDismLikeProgBar(100)}");

            return fileMappings;
        }

        public static void BuildCBS(string ffuPath, string destination_path)
        {
            Dictionary<string, IList<CabinetFileInfo>> packages = BuildPackageListFromLiveFileSystem(ffuPath);
            BuildCabinets(packages, destination_path);

            TempManager.CleanupTempFiles();
            Console.WriteLine("The operation completed successfully.");
        }

        private static List<PartitionInfo> GetPartitions(string ffuPath)
        {
            List<PartitionInfo> partitions = new();

            FileStream ffuStream = File.OpenRead(ffuPath);
            FfuFile ffuFile = new(ffuStream);

            for (int i = 0; i < ffuFile.StoreCount; i++)
            {
                bool hasOsPool = false;

                long diskCapacity = (long)ffuFile.GetMinSectorCount(i) * ffuFile.GetSectorSize(i);
                VirtualDisk virtualDisk = new DiscUtils.Raw.Disk(ffuFile.OpenStore(i), Ownership.None, Geometry.FromCapacity(diskCapacity, (int)ffuFile.GetSectorSize(i)));
                //VirtualDisk virtualDisk = new DiscUtils.Raw.Disk(vhdx, FileAccess.Read);

                PartitionTable partitionTable = virtualDisk.Partitions;

                if (partitionTable != null)
                {
                    foreach (PartitionInfo sspartition in partitionTable.Partitions)
                    {
                        partitions.Add(sspartition);
                        if (sspartition.GuidType == new Guid("E75CAF8F-F680-4CEE-AFA3-B001E56EFC2D"))
                        {
                            hasOsPool = true;
                        }
                    }
                }

                if (hasOsPool)
                {
                    throw new Exception("Image contains an OSPool which is unsupported by this program!");
                }
            }

            return partitions;
        }

        private static IFileSystem? TryCreateFileSystem(SparseStream partitionStream)
        {
            try
            {
                partitionStream.Seek(0, SeekOrigin.Begin);
                if (DiscUtils.Ntfs.NtfsFileSystem.Detect(partitionStream))
                {
                    partitionStream.Seek(0, SeekOrigin.Begin);
                    return new DiscUtils.Ntfs.NtfsFileSystem(partitionStream);
                }
            }
            catch
            {
            }

            try
            {
                // DiscUtils fat implementation is a bit broken, on top of not supporting lfn (which we know how to fix)
                // It also fails to find the last chunk on some known partitions (like PLAT)
                // As a result, we use a bridge file system making use of 7z, it's slower, but it's the best we can do for now

                partitionStream.Seek(0x36, SeekOrigin.Begin);
                byte[] buf = new byte[3];
                partitionStream.Read(buf, 0, 3);
                if (buf[0] == 0x46 && buf[1] == 0x41 && buf[2] == 0x54)
                {
                    partitionStream.Seek(0, SeekOrigin.Begin);
                    return new ArchiveBridge(partitionStream, SevenZipFormat.Fat);
                }
            }
            catch
            {

            }

            return null;
        }

        private static Dictionary<string, IFileSystem> GetFileSystemsWithServicing(List<PartitionInfo> partitions)
        {
            Dictionary<string, IFileSystem> fileSystemsWithServicing = new();

            int c = 0;
            Console.WriteLine("GetFileSystemsWithServicing: Total: " + partitions.Count);
            foreach (PartitionInfo partitionInfo in partitions)
            {
                Console.WriteLine("GetFileSystemsWithServicing: " + c++ + " " + ((GuidPartitionInfo)partitionInfo).Name);
                SparseStream partitionStream = partitionInfo.Open();

                bool hasOneFileSystemOk = false;

                IFileSystem? fileSystem = TryCreateFileSystem(partitionStream);

                if (fileSystem == null)
                {
                    partitionStream.Dispose();
                    continue;
                }

                try
                {
                    if (fileSystem.DirectoryExists(@"Windows\Servicing\Packages"))
                    {
                        Console.WriteLine("GetFileSystemsWithServicing: Adding " + ((GuidPartitionInfo)partitionInfo).Name + " at " + fileSystemsWithServicing.Count);
                        fileSystemsWithServicing.Add(((GuidPartitionInfo)partitionInfo).Name, fileSystem);
                        hasOneFileSystemOk = true;
                    }

                    // Handle UpdateOS as well if found
                    if (fileSystem.FileExists("PROGRAMS\\UpdateOS\\UpdateOS.wim"))
                    {
                        Stream wimStream = fileSystem.OpenFileAndDecompressIfNeeded("PROGRAMS\\UpdateOS\\UpdateOS.wim");
                        DiscUtils.Wim.WimFile wimFile = new(wimStream);

                        bool hasOneWimFileSystemOk = false;

                        for (int i = 0; i < wimFile.ImageCount; i++)
                        {
                            IFileSystem wimFileSystem = wimFile.GetImage(i);
                            if (wimFileSystem.DirectoryExists(@"Windows\Servicing\Packages"))
                            {
                                Console.WriteLine("GetFileSystemsWithServicing: Adding " + ((GuidPartitionInfo)partitionInfo).Name + "'s UPDATEOS.wim at " + fileSystemsWithServicing.Count);
                                fileSystemsWithServicing.Add(((GuidPartitionInfo)partitionInfo).Name + "-UpdateOS", wimFileSystem);
                                hasOneWimFileSystemOk = true;
                                hasOneFileSystemOk = true;
                            }
                        }

                        if (!hasOneWimFileSystemOk)
                        {
                            wimStream.Dispose();
                        }
                    }
                }
                catch
                {

                }

                if (!hasOneFileSystemOk)
                {
                    partitionStream.Dispose();
                }
            }

            return fileSystemsWithServicing;
        }

        private static Dictionary<string, IList<CabinetFileInfo>> BuildPackageListFromLiveFileSystem(string ffuPath)
        {
            Dictionary<string, IList<CabinetFileInfo>> packages = new();

            Console.WriteLine("GetPartitions: Start");
            List<PartitionInfo> partitions = GetPartitions(ffuPath);
            Console.WriteLine("GetPartitions: End");
            Console.WriteLine("GetFileSystemsWithServicing: Start");
            Dictionary<string, IFileSystem> fileSystemsWithServicing = GetFileSystemsWithServicing(partitions);
            Console.WriteLine("GetFileSystemsWithServicing: End");

            foreach (var dictEl in fileSystemsWithServicing)
            {
                IFileSystem fileSystem = dictEl.Value;
                Console.WriteLine("NEW FILE SYSTEM: " + dictEl.Key);

                IEnumerable<string> manifestFiles = fileSystem.GetFiles(@"Windows\servicing\Packages", "*.mum", SearchOption.TopDirectoryOnly);

                int packagesCount = manifestFiles.Count();
                for (int i = 0; i < packagesCount; i++)
                {
                    try
                    {
                        string manifestFile = manifestFiles.ElementAt(i);
                        Console.WriteLine("Processing: " + manifestFile);
                        Stream stream = fileSystem.OpenFileAndDecompressIfNeeded(manifestFile);
                        XmlSerializer serializer = new(typeof(XmlMum.Assembly));
                        XmlMum.Assembly cbs = (XmlMum.Assembly)serializer.Deserialize(stream);

                        List<CabinetFileInfo> fileMappings = GetCabinetFileInfoForCbsPackage(cbs, fileSystem, partitions);

                        string packageName = $"{cbs.AssemblyIdentity.Name.Replace($"_{cbs.AssemblyIdentity.Language}", "", StringComparison.InvariantCultureIgnoreCase)}";
                        if (!packageName.Contains("InboxCompDB"))
                        {
                            packageName = $"{packageName}~{cbs.AssemblyIdentity.PublicKeyToken.Replace("628844477771337a", "31bf3856ad364e35", StringComparison.InvariantCultureIgnoreCase)}~{cbs.AssemblyIdentity.ProcessorArchitecture}~{(cbs.AssemblyIdentity.Language == "neutral" ? "" : cbs.AssemblyIdentity.Language)}~";
                        }

                        packages.Add(Path.Combine(dictEl.Key, packageName), fileMappings);
                        Console.WriteLine("Processing Complete: " + manifestFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }
                }
            }

            return packages;
        }

        private static void BuildCabinets(Dictionary<string, IList<CabinetFileInfo>> packages, string outputPath)
        {
            int packagesCount = packages.Count();
            for (int i = 0; i < packagesCount; i++)
            {
                KeyValuePair<string, IList<CabinetFileInfo>> package = packages.ElementAt(i);
                string packageName = package.Key;
                IList<CabinetFileInfo> fileMappings = package.Value;

                string cabFile = Path.Combine(outputPath, $"{packageName}.cab");
                if (Path.GetDirectoryName(cabFile) is string directory && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(cabFile))
                {
                    try
                    {
                        string componentStatus = $"Processing {i + 1} of {packagesCount} - Creating package {packageName}";
                        if (componentStatus.Length > Console.BufferWidth - 1)
                        {
                            componentStatus = $"{componentStatus[..(Console.BufferWidth - 4)]}...";
                        }

                        Console.WriteLine(componentStatus);

                        int oldPercentage = -1;
                        CabInfo cab = new(cabFile);
                        cab.PackFiles(null, fileMappings.Select(x => x.GetFileTuple()).ToArray(), fileMappings.Select(x => x.FileName).ToArray(), CompressionLevel.Min, (object sender, ArchiveProgressEventArgs archiveProgressEventArgs) =>
                        {
                            int percentage = archiveProgressEventArgs.CurrentFileNumber * 100 / archiveProgressEventArgs.TotalFiles;
                            if (percentage != oldPercentage)
                            {
                                oldPercentage = percentage;
                                string progressBarString = GetDismLikeProgBar(percentage);
                                Console.Write($"\r{progressBarString}");
                            }
                        });

                        if (i != packagesCount - 1)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Console.WriteLine($"{new string(' ', componentStatus.Length)}\n{GetDismLikeProgBar(100)}");
                            Console.SetCursorPosition(0, Console.CursorTop - 2);
                        }
                        else
                        {
                            Console.WriteLine($"\r{GetDismLikeProgBar(100)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: CAB creation failed! {ex.Message}");
                        //throw;
                    }
                }

                foreach (CabinetFileInfo fileMapping in fileMappings)
                {
                    fileMapping.FileStream.Close();
                }
            }

            foreach (KeyValuePair<string, IList<CabinetFileInfo>> package in packages)
            {
                IList<CabinetFileInfo> fileMappings = package.Value;
                foreach (CabinetFileInfo fileMapping in fileMappings)
                {
                    fileMapping.FileStream.Close();
                }
            }
        }

        private static string GetDismLikeProgBar(int percentage)
        {
            int eqsLength = (int)((double)percentage / 100 * 55);
            string bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
            bases = bases.Insert(28, percentage + "%");
            if (percentage == 100)
            {
                bases = bases[1..];
            }
            else if (percentage < 10)
            {
                bases = bases.Insert(28, " ");
            }

            return $"[{bases}]";
        }
    }
}
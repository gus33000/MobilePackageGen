using System.Xml.Serialization;
using Microsoft.Deployment.Compression.Cab;
using DiscUtils;
using Microsoft.Deployment.Compression;
using MobilePackageGen.GZip;
using System.Runtime.InteropServices;

namespace MobilePackageGen
{
    public class SPKGBuilder
    {
        private static string GetSPKGComponentName(XmlDsm.Package dsm)
        {
            return $"{dsm.Identity.Owner}" +
                $"{(string.IsNullOrEmpty(dsm.Identity.Component) ? "": $".{dsm.Identity.Component}")}" +
                $"{(string.IsNullOrEmpty(dsm.Identity.SubComponent) ? "" : $".{dsm.Identity.SubComponent}")}" +
                $"{(string.IsNullOrEmpty(dsm.Culture) == true ? "" : $"_Lang_{dsm.Culture}")}";
        }

        private static List<CabinetFileInfo> GetCabinetFileInfoForDsmPackage(XmlDsm.Package dsm, IPartition partition, List<IDisk> disks)
        {
            List<CabinetFileInfo> fileMappings = [];

            IFileSystem fileSystem = partition.FileSystem!;

            int i = 0;

            uint oldPercentage = uint.MaxValue;

            foreach (XmlDsm.FileEntry packageFile in dsm.Files.FileEntry)
            {
                uint percentage = (uint)Math.Floor((double)i++ * 50 / dsm.Files.FileEntry.Count);

                if (percentage != oldPercentage)
                {
                    oldPercentage = percentage;
                    string progressBarString = Logging.GetDISMLikeProgressBar(percentage);

                    Logging.Log(progressBarString, returnLine: false);
                }

                string fileName = packageFile.DevicePath;

                string normalized = fileName;

                // Prevent getting files from root of this program
                if (normalized.StartsWith('\\'))
                {
                    normalized = normalized[1..];
                }

                CabinetFileInfo? cabinetFileInfo = null;

                // If we end in bin, and the package is marked binary partition, this is a partition on one of the device disks, retrieve it
                if (normalized.EndsWith(".bin") && packageFile.FileType.Contains("BinaryPartition", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (IDisk disk in disks)
                    {
                        bool done = false;

                        foreach (IPartition diskPartition in disk.Partitions)
                        {
                            if (diskPartition.Name.Split("\0")[0].Equals(dsm.Partition, StringComparison.InvariantCultureIgnoreCase))
                            {
                                done = true;

                                // Some older SPKGs from 2012 / 2013 may lack the packageFile.FileSize property entirely
                                // in their DSM file. In this case, we will use the size of the partition itself.
                                // It may also be possible to infer the correct size from the catalog
                                // if the size is not meant to be the full size of the partition.

                                Stream partitionStream = packageFile.FileSize != null ? new Substream(diskPartition.Stream, long.Parse(packageFile.FileSize)) : diskPartition.Stream;

                                diskPartition.Stream.Seek(0, SeekOrigin.Begin);

                                cabinetFileInfo = new CabinetFileInfo()
                                {
                                    FileName = packageFile.CabPath,
                                    FileStream = partitionStream,
                                    Attributes = FileAttributes.Normal,
                                    DateTime = DateTime.Now
                                };
                                break;
                            }
                        }

                        if (done)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (!fileSystem.FileExists(normalized))
                    {
                        string[] partitionNamesWithLinks = ["data", "efiesp", "osdata", "dpp", "mmos"];

                        foreach (string partitionNameWithLink in partitionNamesWithLinks)
                        {
                            if (normalized.StartsWith($"{partitionNameWithLink}\\", StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (IDisk disk in disks)
                                {
                                    bool done = false;

                                    foreach (IPartition diskPartition in disk.Partitions)
                                    {
                                        if (diskPartition.Name.Split("\0")[0].Equals(partitionNameWithLink, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            done = true;

                                            IFileSystem? fileSystemData = diskPartition.FileSystem;

                                            if (fileSystemData == null)
                                            {
                                                break;
                                            }

                                            string targetFile = normalized[(partitionNameWithLink.Length + 1)..];

                                            if (!fileSystemData.FileExists(targetFile))
                                            {
                                                break;
                                            }

                                            bool needsDecompression = packageFile.FileType.Contains("registry", StringComparison.CurrentCultureIgnoreCase) || packageFile.FileType.Contains("policy", StringComparison.CurrentCultureIgnoreCase) || packageFile.FileType.Contains("manifest", StringComparison.CurrentCultureIgnoreCase);
                                            bool doesNotNeedDecompression = packageFile.FileType.Contains("catalog", StringComparison.CurrentCultureIgnoreCase) || packageFile.FileType.Contains("regular", StringComparison.CurrentCultureIgnoreCase);

                                            if (needsDecompression)
                                            {
                                                Stream? cabFileStream = null;

                                                try
                                                {
                                                    cabFileStream = fileSystemData.OpenFileAndDecompressAsGZip(targetFile);
                                                }
                                                catch (InvalidDataException)
                                                {
                                                    cabFileStream = fileSystemData.OpenFile(targetFile, FileMode.Open, FileAccess.Read);
                                                }

                                                cabinetFileInfo = new CabinetFileInfo()
                                                {
                                                    FileName = packageFile.CabPath,
                                                    FileStream = cabFileStream,
                                                    Attributes = fileSystemData.GetAttributes(targetFile) & ~FileAttributes.ReparsePoint,
                                                    DateTime = fileSystemData.GetLastWriteTime(targetFile)
                                                };
                                            }
                                            else if (doesNotNeedDecompression)
                                            {
                                                cabinetFileInfo = new CabinetFileInfo()
                                                {
                                                    FileName = packageFile.CabPath,
                                                    FileStream = fileSystemData.OpenFile(targetFile, FileMode.Open, FileAccess.Read),
                                                    Attributes = fileSystemData.GetAttributes(targetFile) & ~FileAttributes.ReparsePoint,
                                                    DateTime = fileSystemData.GetLastWriteTime(targetFile)
                                                };
                                            }
                                            else
                                            {
                                                cabinetFileInfo = new CabinetFileInfo()
                                                {
                                                    FileName = packageFile.CabPath,
                                                    FileStream = fileSystemData.OpenFile(targetFile, FileMode.Open, FileAccess.Read),
                                                    Attributes = fileSystemData.GetAttributes(targetFile) & ~FileAttributes.ReparsePoint,
                                                    DateTime = fileSystemData.GetLastWriteTime(targetFile)
                                                };
                                            }

                                            break;
                                        }
                                    }

                                    if (done)
                                    {
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        bool needsDecompression = packageFile.FileType.Contains("registry", StringComparison.CurrentCultureIgnoreCase) || packageFile.FileType.Contains("policy", StringComparison.CurrentCultureIgnoreCase) || packageFile.FileType.Contains("manifest", StringComparison.CurrentCultureIgnoreCase);
                        bool doesNotNeedDecompression = packageFile.FileType.Contains("catalog", StringComparison.CurrentCultureIgnoreCase) || packageFile.FileType.Contains("regular", StringComparison.CurrentCultureIgnoreCase);

                        if (needsDecompression)
                        {
                            Stream? cabFileStream = null;

                            try
                            {
                                cabFileStream = fileSystem.OpenFileAndDecompressAsGZip(normalized);
                            }
                            catch (InvalidDataException)
                            {
                                cabFileStream = fileSystem.OpenFile(normalized, FileMode.Open, FileAccess.Read);
                            }

                            cabinetFileInfo = new CabinetFileInfo()
                            {
                                FileName = packageFile.CabPath,
                                FileStream = cabFileStream,
                                Attributes = fileSystem.GetAttributes(normalized) & ~FileAttributes.ReparsePoint,
                                DateTime = fileSystem.GetLastWriteTime(normalized)
                            };
                        }
                        else if (doesNotNeedDecompression)
                        {
                            cabinetFileInfo = new CabinetFileInfo()
                            {
                                FileName = packageFile.CabPath,
                                FileStream = fileSystem.OpenFile(normalized, FileMode.Open, FileAccess.Read),
                                Attributes = fileSystem.GetAttributes(normalized) & ~FileAttributes.ReparsePoint,
                                DateTime = fileSystem.GetLastWriteTime(normalized)
                            };
                        }
                        else
                        {
                            cabinetFileInfo = new CabinetFileInfo()
                            {
                                FileName = packageFile.CabPath,
                                FileStream = fileSystem.OpenFile(normalized, FileMode.Open, FileAccess.Read),
                                Attributes = fileSystem.GetAttributes(normalized) & ~FileAttributes.ReparsePoint,
                                DateTime = fileSystem.GetLastWriteTime(normalized)
                            };
                        }
                    }
                }

                if (cabinetFileInfo != null)
                {
                    fileMappings.Add(cabinetFileInfo);
                }
                else
                {
                    Logging.Log($"\rError: File not found! {normalized}\n", LoggingLevel.Error);
                    //throw new FileNotFoundException(normalized);
                }
            }

            return fileMappings;
        }

        public static void BuildSPKG(List<IDisk> disks, string destination_path)
        {
            Logging.Log();
            Logging.Log("Building SPKG Cabinet Files...");
            Logging.Log();

            BuildCabinets(disks, destination_path);

            Logging.Log();
            Logging.Log("Cleaning up...");
            Logging.Log();

            TempManager.CleanupTempFiles();
        }

        private static List<IPartition> GetPartitionsWithServicing(List<IDisk> disks)
        {
            List<IPartition> fileSystemsWithServicing = [];

            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    IFileSystem? fileSystem = partition.FileSystem;

                    if (fileSystem != null)
                    {
                        try
                        {
                            if (fileSystem.DirectoryExists(@"Windows\Packages\DsmFiles"))
                            {
                                fileSystemsWithServicing.Add(partition);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.Log($"Error: Looking up file system servicing failed! {ex.Message}", LoggingLevel.Error);
                        }
                    }
                }
            }

            return fileSystemsWithServicing;
        }

        private static int GetPackageCount(List<IDisk> disks)
        {
            int count = 0;

            List<IPartition> partitionsWithCbsServicing = GetPartitionsWithServicing(disks);

            foreach (IPartition partition in partitionsWithCbsServicing)
            {
                IFileSystem fileSystem = partition.FileSystem!;

                IEnumerable<string> manifestFiles = fileSystem.GetFiles(@"Windows\Packages\DsmFiles", "*.xml", SearchOption.TopDirectoryOnly);

                count += manifestFiles.Count();
            }

            return count;
        }

        private static void BuildCabinets(List<IDisk> disks, string outputPath)
        {
            int packagesCount = GetPackageCount(disks);

            List<IPartition> partitionsWithCbsServicing = GetPartitionsWithServicing(disks);

            int i = 0;

            foreach (IPartition partition in partitionsWithCbsServicing)
            {
                IFileSystem fileSystem = partition.FileSystem!;

                IEnumerable<string> manifestFiles = fileSystem.GetFiles(@"Windows\Packages\DsmFiles", "*.xml", SearchOption.TopDirectoryOnly);

                foreach (string manifestFile in manifestFiles)
                {
                    try
                    {
                        XmlDsm.Package? dsm = null;

                        try
                        {
                            using Stream stream = fileSystem.OpenFileAndDecompressAsGZip(manifestFile);
                            XmlSerializer serializer = new(typeof(XmlDsm.Package));
                            dsm = (XmlDsm.Package)serializer.Deserialize(stream)!;
                        }
                        catch (InvalidDataException)
                        {
                            using Stream stream = fileSystem.OpenFile(manifestFile, FileMode.Open, FileAccess.Read);
                            XmlSerializer serializer = new(typeof(XmlDsm.Package));
                            dsm = (XmlDsm.Package)serializer.Deserialize(stream)!;
                        }

                        string packageName = GetSPKGComponentName(dsm);

                        string cabFileName = Path.Combine(partition.Name.Replace("\0", "-"), packageName);

                        string cabFile = Path.Combine(outputPath, $"{cabFileName}.spkg");
                        if (Path.GetDirectoryName(cabFile) is string directory && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        string componentStatus = $"Creating package {i + 1} of {packagesCount} - {cabFileName}";
                        if (componentStatus.Length > Console.BufferWidth - 24 - 1)
                        {
                            componentStatus = $"{componentStatus[..(Console.BufferWidth - 24 - 4)]}...";
                        }

                        Logging.Log(componentStatus);
                        string progressBarString = Logging.GetDISMLikeProgressBar(0);
                        Logging.Log(progressBarString, returnLine: false);

                        string fileStatus = "";

                        if (!File.Exists(cabFile))
                        {
                            List<CabinetFileInfo> fileMappings = GetCabinetFileInfoForDsmPackage(dsm, partition, disks);

                            uint oldPercentage = uint.MaxValue;
                            uint oldFilePercentage = uint.MaxValue;
                            string oldFileName = "";

                            // Cab Creation is only supported on Windows
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                CabInfo cab = new(cabFile);
                                cab.PackFiles(null, fileMappings.Select(x => x.GetFileTuple()).ToArray(), fileMappings.Select(x => x.FileName).ToArray(), CompressionLevel.Min, (object? _, ArchiveProgressEventArgs archiveProgressEventArgs) =>
                                {
                                    string fileNameParsed;
                                    if (string.IsNullOrEmpty(archiveProgressEventArgs.CurrentFileName))
                                    {
                                        fileNameParsed = $"Unknown ({archiveProgressEventArgs.CurrentFileNumber})";
                                    }
                                    else
                                    {
                                        fileNameParsed = archiveProgressEventArgs.CurrentFileName;
                                    }

                                    uint percentage = (uint)Math.Floor((double)archiveProgressEventArgs.CurrentFileNumber * 50 / archiveProgressEventArgs.TotalFiles) + 50;

                                    if (percentage != oldPercentage)
                                    {
                                        oldPercentage = percentage;
                                        string progressBarString = Logging.GetDISMLikeProgressBar(percentage);

                                        Logging.Log(progressBarString, returnLine: false);
                                    }

                                    if (fileNameParsed != oldFileName)
                                    {
                                        Logging.Log();
                                        Logging.Log(new string(' ', fileStatus.Length));
                                        Logging.Log(Logging.GetDISMLikeProgressBar(0), returnLine: false);

                                        Console.SetCursorPosition(0, Console.CursorTop - 2);

                                        oldFileName = fileNameParsed;

                                        oldFilePercentage = uint.MaxValue;

                                        fileStatus = $"Adding file {archiveProgressEventArgs.CurrentFileNumber + 1} of {archiveProgressEventArgs.TotalFiles} - {fileNameParsed}";
                                        if (fileStatus.Length > Console.BufferWidth - 24 - 1)
                                        {
                                            fileStatus = $"{fileStatus[..(Console.BufferWidth - 24 - 4)]}...";
                                        }

                                        Logging.Log();
                                        Logging.Log(fileStatus);
                                        Logging.Log(Logging.GetDISMLikeProgressBar(0), returnLine: false);

                                        Console.SetCursorPosition(0, Console.CursorTop - 2);
                                    }

                                    uint filePercentage = (uint)Math.Floor((double)archiveProgressEventArgs.CurrentFileBytesProcessed * 100 / archiveProgressEventArgs.CurrentFileTotalBytes);

                                    if (filePercentage != oldFilePercentage)
                                    {
                                        oldFilePercentage = filePercentage;
                                        string progressBarString = Logging.GetDISMLikeProgressBar(filePercentage);

                                        Logging.Log();
                                        Logging.Log();
                                        Logging.Log(progressBarString, returnLine: false);

                                        Console.SetCursorPosition(0, Console.CursorTop - 2);
                                    }
                                });
                            }

                            foreach (CabinetFileInfo fileMapping in fileMappings)
                            {
                                fileMapping.FileStream.Close();
                            }
                        }

                        if (i != packagesCount - 1)
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);

                            Logging.Log(new string(' ', componentStatus.Length));
                            Logging.Log(Logging.GetDISMLikeProgressBar(100));

                            if (string.IsNullOrEmpty(fileStatus))
                            {
                                Logging.Log(new string(' ', fileStatus.Length));
                                Logging.Log(new string(' ', 60));
                            }
                            else
                            {
                                Logging.Log(new string(' ', fileStatus.Length));
                                Logging.Log(Logging.GetDISMLikeProgressBar(100));
                            }

                            Console.SetCursorPosition(0, Console.CursorTop - 4);
                        }
                        else
                        {
                            Logging.Log($"\r{Logging.GetDISMLikeProgressBar(100)}");

                            if (string.IsNullOrEmpty(fileStatus))
                            {
                                Logging.Log();
                                Logging.Log(new string(' ', 60));
                            }
                            else
                            {
                                Logging.Log();
                                Logging.Log(Logging.GetDISMLikeProgressBar(100));
                            }
                        }

                        i++;
                    }
                    catch (Exception ex)
                    {
                        Logging.Log($"Error: CAB creation failed! {ex.Message}", LoggingLevel.Error);
                        //throw;
                    }
                }
            }
        }
    }
}
﻿using DiscUtils;
using Microsoft.Deployment.Compression.Cab;
using Microsoft.Deployment.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ToCBS.Wof;

namespace ToCBS
{
    internal class CBSBuilder
    {
        private static List<CabinetFileInfo> GetCabinetFileInfoForCbsPackage(XmlMum.Assembly cbs, Partition partition, List<Disk> disks)
        {
            List<CabinetFileInfo> fileMappings = new();

            IFileSystem fileSystem = partition.FileSystem;

            string packages_path = @"Windows\servicing\Packages";
            string winsxs_manifests_path = @"Windows\WinSxS\Manifests";

            string packageName = $"{cbs.AssemblyIdentity.Name}~{cbs.AssemblyIdentity.PublicKeyToken}~{cbs.AssemblyIdentity.ProcessorArchitecture}~{(cbs.AssemblyIdentity.Language == "neutral" ? "" : cbs.AssemblyIdentity.Language)}~{cbs.AssemblyIdentity.Version}";

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
                    foreach (Disk disk in disks)
                    {
                        bool done = false;

                        foreach (Partition diskPartition in disk.Partitions)
                        {
                            if (diskPartition.Name.Equals(cbs.Package.TargetPartition, StringComparison.InvariantCultureIgnoreCase))
                            {
                                done = true;

                                cabinetFileInfo = new CabinetFileInfo()
                                {
                                    FileName = packageFile.Cabpath,
                                    FileStream = new Substream(diskPartition.Stream, long.Parse(packageFile.Size)),
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
                        string[] partitionNamesWithLinks = new string[] { "data", "efiesp", "osdata", "dpp", "mmos" };

                        foreach (string partitionNameWithLink in partitionNamesWithLinks)
                        {
                            if (normalized.StartsWith(partitionNameWithLink + "\\", StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (Disk disk in disks)
                                {
                                    bool done = false;

                                    foreach (Partition diskPartition in disk.Partitions)
                                    {
                                        if (diskPartition.Name.Equals(partitionNameWithLink, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            done = true;

                                            IFileSystem? fileSystemData = diskPartition.FileSystem;

                                            if (fileSystemData == null)
                                            {
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
                    Console.WriteLine($"Error: File not found! {normalized}");
                    //throw new FileNotFoundException(normalized);
                }
            }

            return fileMappings;
        }

        public static void BuildCBS(List<Disk> disks, string destination_path)
        {
            Console.WriteLine("Getting Update OS Disks...");

            disks.AddRange(Disk.GetUpdateOSDisks(disks));

            Console.WriteLine();
            Console.WriteLine("Found Disks:");
            Console.WriteLine();

            foreach (Disk disk in disks)
            {
                foreach (Partition partition in disk.Partitions)
                {
                    if (partition.FileSystem != null)
                    {
                        Console.WriteLine($"{partition.Name} {partition.ID} {partition.Type} {partition.Size} KnownFS");
                    }
                }
            }

            Console.WriteLine();

            foreach (Disk disk in disks)
            {
                foreach (Partition partition in disk.Partitions)
                {
                    if (partition.FileSystem == null)
                    {
                        Console.WriteLine($"{partition.Name} {partition.ID} {partition.Type} {partition.Size} UnknownFS");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Building CBS Cabinet Files...");
            Console.WriteLine();

            BuildCabinets(disks, destination_path);

            Console.WriteLine();
            Console.WriteLine("Cleaning up...");
            Console.WriteLine();

            TempManager.CleanupTempFiles();

            Console.WriteLine("The operation completed successfully.");
        }

        private static List<Partition> GetPartitionsWithServicing(List<Disk> disks)
        {
            List<Partition> fileSystemsWithServicing = new();

            foreach (Disk disk in disks)
            {
                foreach (Partition partition in disk.Partitions)
                {
                    IFileSystem? fileSystem = partition.FileSystem;

                    if (fileSystem != null)
                    {
                        try
                        {
                            if (fileSystem.DirectoryExists(@"Windows\Servicing\Packages"))
                            {
                                fileSystemsWithServicing.Add(partition);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return fileSystemsWithServicing;
        }

        private static int GetPackageCount(List<Disk> disks)
        {
            int count = 0;

            List<Partition> partitionsWithCbsServicing = GetPartitionsWithServicing(disks);

            foreach (Partition partition in partitionsWithCbsServicing)
            {
                IFileSystem fileSystem = partition.FileSystem;

                IEnumerable<string> manifestFiles = fileSystem.GetFiles(@"Windows\servicing\Packages", "*.mum", SearchOption.TopDirectoryOnly);

                count += manifestFiles.Count();
            }

            return count;
        }

        private static void BuildCabinets(List<Disk> disks, string outputPath)
        {
            int packagesCount = GetPackageCount(disks);

            List<Partition> partitionsWithCbsServicing = GetPartitionsWithServicing(disks);
            int i = 0;

            foreach (Partition partition in partitionsWithCbsServicing)
            {
                IFileSystem fileSystem = partition.FileSystem;

                IEnumerable<string> manifestFiles = fileSystem.GetFiles(@"Windows\servicing\Packages", "*.mum", SearchOption.TopDirectoryOnly);

                foreach (string manifestFile in manifestFiles)
                {
                    try
                    {
                        Stream stream = fileSystem.OpenFileAndDecompressIfNeeded(manifestFile);
                        XmlSerializer serializer = new(typeof(XmlMum.Assembly));
                        XmlMum.Assembly cbs = (XmlMum.Assembly)serializer.Deserialize(stream);

                        string packageName = $"{cbs.AssemblyIdentity.Name.Replace($"_{cbs.AssemblyIdentity.Language}", "", StringComparison.InvariantCultureIgnoreCase)}";

                        if (!packageName.Contains("InboxCompDB"))
                        {
                            packageName = $"{packageName}~{cbs.AssemblyIdentity.PublicKeyToken.Replace("628844477771337a", "31bf3856ad364e35", StringComparison.InvariantCultureIgnoreCase)}~{cbs.AssemblyIdentity.ProcessorArchitecture}~{(cbs.AssemblyIdentity.Language == "neutral" ? "" : cbs.AssemblyIdentity.Language)}~";
                        }

                        string cabFileName = Path.Combine(partition.Name, packageName);

                        string cabFile = Path.Combine(outputPath, $"{cabFileName}.cab");
                        if (Path.GetDirectoryName(cabFile) is string directory && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        string componentStatus = $"Processing {i + 1} of {packagesCount} - Creating package {cabFileName}";
                        if (componentStatus.Length > Console.BufferWidth - 1)
                        {
                            componentStatus = $"{componentStatus[..(Console.BufferWidth - 4)]}...";
                        }

                        Console.WriteLine(componentStatus);

                        if (!File.Exists(cabFile))
                        {
                            List<CabinetFileInfo> fileMappings = GetCabinetFileInfoForCbsPackage(cbs, partition, disks);

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


                            foreach (CabinetFileInfo fileMapping in fileMappings)
                            {
                                fileMapping.FileStream.Close();
                            }
                        }

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

                        i++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: CAB creation failed! {ex.Message}");
                        //throw;
                    }
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
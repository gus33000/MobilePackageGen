using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Deployment.Compression.Cab;
using DiscUtils;
using Microsoft.Deployment.Compression;
using ToSPKG.GZip;
using DiscUtils.Streams;

namespace ToSPKG
{
    public class SPKGBuilder
    {
        public static void BuildSPKG(List<Disk> disks, string destination_path)
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
            Console.WriteLine("Building SPKG Cabinet Files...");
            Console.WriteLine();

            BuildCabinets(disks, destination_path);

            Console.WriteLine();
            Console.WriteLine("Cleaning up...");
            Console.WriteLine();

            TempManager.CleanupTempFiles();

            Console.WriteLine("The operation completed successfully.");
        }

        private static void CreateDirectoryWhileCopying(IFileSystem fileSystem, string source, string dest, bool decompress = false)
        {
            string dirFromDest = Path.GetDirectoryName(dest);
            if (!Directory.Exists(dirFromDest))
            {
                Directory.CreateDirectory(dirFromDest);
            }

            try
            {
                if (!decompress)
                {
                    using Stream stream = fileSystem.OpenFile(source, FileMode.Open, FileAccess.Read);
                    using Stream destStream = File.Open(dest, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                    stream.CopyTo(destStream);
                }
                else
                {
                    using Stream stream = fileSystem.OpenFileAndDecompressAsGZip(source);
                    using Stream destStream = File.Open(dest, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                    stream.CopyTo(destStream);
                }
            }
            catch (Exception ex)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                //Console.WriteLine($"FILED TO COPY {source} --> {ex.Message}");
                //Console.ResetColor();
            }
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
                            if (fileSystem.DirectoryExists(@"Windows\Packages\DsmFiles"))
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

                IEnumerable<string> manifestFiles = fileSystem.GetFiles(@"Windows\Packages\DsmFiles", "*.xml", SearchOption.TopDirectoryOnly);

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

                IEnumerable<string> XMLs = fileSystem.GetFiles(@"Windows\Packages\DsmFiles", "*.xml", SearchOption.TopDirectoryOnly);

                foreach (string xml in XMLs)
                {
                    using Stream stream = fileSystem.OpenFileAndDecompressAsGZip(xml);
                    XmlSerializer serializer = new(typeof(XmlDsm.Package));
                    XmlDsm.Package xmlDSM = (XmlDsm.Package)serializer.Deserialize(stream);

                    string current_folder = Path.Combine(outputPath, partition.Name, xml.Split('\\').Last().Replace(".xml", "").Replace(".dsm", ""));

                    if (!Directory.Exists(current_folder))
                    {
                        Directory.CreateDirectory(current_folder);
                    }

                    if (xmlDSM.Files?.FileEntry?.Count > 0)
                    {
                        //Console.ForegroundColor = ConsoleColor.Green;
                        //Console.WriteLine($"\nPROCESSING: {xml.Split('\\').Last().Replace(".xml", "").Replace(".dsm", "")}");
                        //Console.ResetColor();

                        xmlDSM.Files.FileEntry.ForEach(xmlFile =>
                        {
                            string normalized = xmlFile.DevicePath;

                            if (xmlFile.DevicePath.StartsWith('\\'))
                            {
                                normalized = xmlFile.DevicePath.Substring(1);
                            }

                            //Console.ForegroundColor = ConsoleColor.Cyan;
                            //Console.WriteLine($"Copying: {normalized}");
                            //Console.ResetColor();

                            if (xmlFile.FileType.Contains("registry", StringComparison.CurrentCultureIgnoreCase) || xmlFile.FileType.Contains("policy", StringComparison.CurrentCultureIgnoreCase) || xmlFile.FileType.Contains("manifest", StringComparison.CurrentCultureIgnoreCase))
                            {
                                CreateDirectoryWhileCopying(fileSystem, normalized, Path.Combine(current_folder, xmlFile.CabPath), true);
                            }
                            else if (xmlFile.FileType.Contains("catalog", StringComparison.CurrentCultureIgnoreCase) || xmlFile.FileType.Contains("regular", StringComparison.CurrentCultureIgnoreCase))
                            {
                                CreateDirectoryWhileCopying(fileSystem, normalized, Path.Combine(current_folder, xmlFile.CabPath));
                            }
                            else if (xmlFile.FileType.Contains("binarypartition", StringComparison.CurrentCultureIgnoreCase)) //MEH
                            {
                                Stream FileStream = null;

                                foreach (Disk disk in disks)
                                {
                                    bool done = false;

                                    foreach (Partition diskPartition in disk.Partitions)
                                    {
                                        if (diskPartition.Name.Equals(xmlDSM.Partition, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            done = true;
                                            FileStream = new Substream(diskPartition.Stream, long.Parse(xmlFile.FileSize));
                                            break;
                                        }
                                    }

                                    if (done)
                                    {
                                        break;
                                    }
                                }

                                if (FileStream != null)
                                {
                                    string dest = Path.Combine(current_folder, xmlFile.CabPath);

                                    string dirFromDest = Path.GetDirectoryName(dest);
                                    if (!Directory.Exists(dirFromDest))
                                    {
                                        Directory.CreateDirectory(dirFromDest);
                                    }

                                    using Stream destStream = File.Open(dest, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                                    FileStream.CopyTo(destStream);
                                }
                                else
                                {
                                    throw new Exception($"Cannot find Binary Partition named: {xmlDSM.Partition}");
                                }
                            }
                            else
                            {
                                Debugger.Break();
                            }
                        });
                    }

                    try
                    {
                        string packageName = current_folder.Split('\\').Last();

                        string cabFileName = Path.Combine(partition.Name, packageName);

                        string cabFile = Path.Combine(outputPath, $"{cabFileName}.spkg");
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
                            int oldPercentage = -1;
                            CabInfo cab = new(cabFile);
                            cab.Pack(current_folder, true, CompressionLevel.Max, (object sender, ArchiveProgressEventArgs archiveProgressEventArgs) =>
                            {
                                int percentage = archiveProgressEventArgs.CurrentFileNumber * 100 / archiveProgressEventArgs.TotalFiles;
                                if (percentage != oldPercentage)
                                {
                                    oldPercentage = percentage;
                                    string progressBarString = GetDismLikeProgBar(percentage);
                                    Console.Write($"\r{progressBarString}");
                                }
                            });
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

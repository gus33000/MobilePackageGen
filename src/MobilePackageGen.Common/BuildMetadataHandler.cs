using DiscUtils;

namespace MobilePackageGen
{
    public static class BuildMetadataHandler
    {
        public static UpdateHistory.UpdateHistory? GetUpdateHistory(IEnumerable<IDisk> disks)
        {
            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    IFileSystem? fileSystem = partition.FileSystem;

                    if (fileSystem != null)
                    {
                        if (fileSystem.DirectoryExists(@"Windows\ImageUpdate"))
                        {
                            string[] ImageUpdateFiles = fileSystem.GetFiles(@"Windows\ImageUpdate", "*", SearchOption.AllDirectories).ToArray();
                            foreach (string ImageUpdateFile in ImageUpdateFiles)
                            {
                                if (Path.GetFileName(ImageUpdateFile).Equals("UpdateHistory.xml", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    try
                                    {
                                        using Stream UpdateHistoryStream = fileSystem.OpenFile(ImageUpdateFile, FileMode.Open, FileAccess.Read);
                                        UpdateHistory.UpdateHistory UpdateHistory = UpdateHistoryStream.GetObjectFromXML<UpdateHistory.UpdateHistory>();

                                        return UpdateHistory;
                                    }
                                    catch { }
                                }
                            }
                        }

                        if (fileSystem.DirectoryExists(@"SharedData\DuShared"))
                        {
                            string[] DUSharedFiles = fileSystem.GetFiles(@"SharedData\DuShared", "*", SearchOption.AllDirectories).ToArray();
                            foreach (string DUSharedFile in DUSharedFiles)
                            {
                                if (Path.GetFileName(DUSharedFile).Equals("UpdateHistory.xml", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    using Stream UpdateHistoryStream = fileSystem.OpenFile(DUSharedFile, FileMode.Open, FileAccess.Read);
                                    UpdateHistory.UpdateHistory UpdateHistory = UpdateHistoryStream.GetObjectFromXML<UpdateHistory.UpdateHistory>();

                                    return UpdateHistory;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static void GetOEMInput(IEnumerable<IDisk> disks, string destination_path)
        {
            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    IFileSystem? fileSystem = partition.FileSystem;

                    if (fileSystem != null)
                    {
                        if (fileSystem.FileExists(@"Windows\ImageUpdate\OEMInput.xml"))
                        {
                            if (!Directory.Exists(destination_path))
                            {
                                Directory.CreateDirectory(destination_path);
                            }

                            string destOemInput = Path.Combine(destination_path, "OEMInput.xml");

                            if (!File.Exists(destOemInput))
                            {
                                using Stream OEMInputFileStream = fileSystem.OpenFile(@"Windows\ImageUpdate\OEMInput.xml", FileMode.Open, FileAccess.Read);

                                FileAttributes Attributes = fileSystem.GetAttributes(@"Windows\ImageUpdate\OEMInput.xml") & ~FileAttributes.ReparsePoint;
                                DateTime LastWriteTime = fileSystem.GetLastWriteTime(@"Windows\ImageUpdate\OEMInput.xml");

                                using (Stream outputFile = File.Create(destOemInput))
                                {
                                    OEMInputFileStream.CopyTo(outputFile);
                                }

                                File.SetAttributes(destOemInput, Attributes);
                                File.SetLastWriteTime(destOemInput, LastWriteTime);
                            }
                        }
                    }
                }
            }
        }

        public static void GetAdditionalContent(IEnumerable<IDisk> disks)
        {
            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    IFileSystem? fileSystem = partition.FileSystem;

                    if (fileSystem != null)
                    {
                        if (partition.Name.Equals("BSP", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // BSP Partition
                            // Driver Packages
                        }

                        if (partition.Name.Equals("PreInstalled", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // PreInstalled Partition
                            // Extracted APPX, Licenses
                        }
                    }
                }
            }
        }

        public static (string cabFileName, string cabFile) GetPackageNamingForSPKG(XmlDsm.Package dsm, UpdateHistory.UpdateHistory? updateHistory)
        {
            string cabFileName = "";
            string cabFile = "";

            bool found = false;

            if (updateHistory != null)
            {
                // Go through every update in reverse chronological order
                foreach (UpdateHistory.UpdateEvent UpdateEvent in updateHistory.UpdateEvents.UpdateEvent.Reverse())
                {
                    foreach (UpdateHistory.Package Package in UpdateEvent.UpdateOSOutput.Packages.Package)
                    {
                        bool matches = (dsm.Identity.Owner ?? "").Equals(Package.Identity.Owner ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Identity.Component ?? "").Equals(Package.Identity.Component ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Identity.Version.Major ?? "").Equals(Package.Identity.Version.Major ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Identity.Version.Minor ?? "").Equals(Package.Identity.Version.Minor ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Identity.Version.QFE ?? "").Equals(Package.Identity.Version.QFE ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Identity.Version.Build ?? "").Equals(Package.Identity.Version.Build ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Identity.SubComponent ?? "").Equals(Package.Identity.SubComponent ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.ReleaseType ?? "").Equals(Package.ReleaseType ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.OwnerType ?? "").Equals(Package.OwnerType ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.BuildType ?? "").Equals(Package.BuildType ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.CpuType ?? "").Equals(Package.CpuType ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Partition ?? "").Equals(Package.Partition ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            //(dsm.IsRemoval ?? "").Equals(Package.IsRemoval ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            //(dsm.GroupingKey ?? "").Equals(Package.GroupingKey ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Culture ?? "").Equals(Package.Culture ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            //(dsm.Platform ?? "").Equals(Package.Platform ?? "", StringComparison.InvariantCultureIgnoreCase) &&
                            (dsm.Resolution ?? "").Equals(Package.Resolution ?? "", StringComparison.InvariantCultureIgnoreCase);

                        if (matches)
                        {
                            string DestinationPath = Package.PackageFile;

                            /*if (DestinationPath.StartsWith(@"\\?\"))
                            {
                                int indexOfPackages = DestinationPath.IndexOf("MSPackages");
                                if (indexOfPackages > -1)
                                {
                                    DestinationPath = DestinationPath[indexOfPackages..];
                                }
                            }*/

                            if (DestinationPath.StartsWith(@"\\?\"))
                            {
                                DestinationPath = DestinationPath[4..];
                            }

                            if (DestinationPath[1] == ':')
                            {
                                DestinationPath = Path.Combine($"Drive{DestinationPath[0]}", DestinationPath[3..]);
                            }

                            string DestinationPathExtension = Path.GetExtension(DestinationPath);
                            if (!string.IsNullOrEmpty(DestinationPathExtension))
                            {
                                cabFileName = DestinationPath[..^DestinationPathExtension.Length];
                                cabFile = DestinationPath;
                            }
                            else
                            {
                                cabFileName = DestinationPath;
                                cabFile = cabFileName;
                            }

                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }
            }

            return (cabFileName, cabFile);
        }

        public static (string cabFileName, string cabFile) GetPackageNamingForCBS(XmlMum.Assembly cbs, UpdateHistory.UpdateHistory? updateHistory)
        {
            string cabFileName = "";
            string cabFile = "";

            bool found = false;

            string cbsPackageIdentity = $"{cbs.AssemblyIdentity.Name}~{cbs.AssemblyIdentity.PublicKeyToken}~{cbs.AssemblyIdentity.ProcessorArchitecture}~{(cbs.AssemblyIdentity.Language == "neutral" ? "" : cbs.AssemblyIdentity.Language)}~{cbs.AssemblyIdentity.Version}";

            if (updateHistory != null)
            {
                // Go through every update in reverse chronological order
                foreach (UpdateHistory.UpdateEvent UpdateEvent in updateHistory.UpdateEvents.UpdateEvent.Reverse())
                {
                    foreach (UpdateHistory.Package Package in UpdateEvent.UpdateOSOutput.Packages.Package)
                    {
                        if (Package.PackageFile.EndsWith(".mum", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        bool matches = Package.PackageIdentity.Equals(cbsPackageIdentity, StringComparison.InvariantCultureIgnoreCase);

                        if (matches)
                        {
                            string DestinationPath = Package.PackageFile;

                            /*if (DestinationPath.StartsWith(@"\\?\"))
                            {
                                int indexOfPackages = DestinationPath.IndexOf("MSPackages");
                                if (indexOfPackages > -1)
                                {
                                    DestinationPath = DestinationPath[indexOfPackages..];
                                }
                            }*/

                            if (DestinationPath.StartsWith(@"\\?\"))
                            {
                                DestinationPath = DestinationPath[4..];
                            }

                            if (DestinationPath[1] == ':')
                            {
                                DestinationPath = Path.Combine($"Drive{DestinationPath[0]}", DestinationPath[3..]);
                            }

                            string DestinationPathExtension = Path.GetExtension(DestinationPath);
                            if (!string.IsNullOrEmpty(DestinationPathExtension))
                            {
                                cabFileName = DestinationPath[..^DestinationPathExtension.Length];
                                cabFile = DestinationPath;
                            }
                            else
                            {
                                cabFileName = DestinationPath;
                                cabFile = cabFileName;
                            }

                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }
            }

            return (cabFileName, cabFile);
        }
    }
}
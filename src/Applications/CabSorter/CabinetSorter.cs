using DiscUtils;
using MobilePackageGen;

namespace CabSorter
{
    internal class CabinetSorter
    {
        private static void SortCabinets(UpdateHistory.UpdateHistory UpdateHistory, string destination_path)
        {
            foreach (UpdateHistory.UpdateEvent UpdateEvent in UpdateHistory.UpdateEvents.UpdateEvent)
            {
                foreach (UpdateHistory.Package Package in UpdateEvent.UpdateOSOutput.Packages.Package)
                {
                    if (!string.IsNullOrEmpty(Package.PackageIdentity))
                    {
                        string DestinationPath = Package.PackageFile;

                        if (DestinationPath.StartsWith(@"\\?\"))
                        {
                            int indexOfPackages = DestinationPath.IndexOf("MSPackages");
                            if (indexOfPackages > -1)
                            {
                                DestinationPath = DestinationPath[indexOfPackages..];
                            }
                        }

                        if (DestinationPath.StartsWith(@"\\?\"))
                        {
                            DestinationPath = DestinationPath[4..];
                        }

                        if (DestinationPath[1] == ':')
                        {
                            DestinationPath = Path.Combine($"Drive{DestinationPath[0]}", DestinationPath[3..]);
                        }

                        bool found = false;
                        string foundFilePath = "";

                        string MatchingCabName = $"{string.Join("~", Package.PackageIdentity.Split("~")[..^1])}~.cab";

                        foreach (string file in Directory.GetFiles(destination_path, "*.cab", SearchOption.AllDirectories).ToArray())
                        {
                            if (Path.GetFileName(file).Equals(MatchingCabName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                found = true;
                                foundFilePath = file;
                                break;
                            }
                        }

                        if (!found)
                        {
                            string NewMatchingCabName = MatchingCabName.Replace("628844477771337a", "31bf3856ad364e35");

                            foreach (string file in Directory.GetFiles(destination_path, "*.cab", SearchOption.AllDirectories).ToArray())
                            {
                                if (Path.GetFileName(file).Equals(NewMatchingCabName, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    found = true;
                                    foundFilePath = file;
                                    break;
                                }
                            }
                        }

                        if (!found && !string.IsNullOrEmpty(Package.Culture))
                        {
                            string NewMatchingCabName = MatchingCabName.Replace($"_{Package.Culture}~", "~");

                            foreach (string file in Directory.GetFiles(destination_path, "*.cab", SearchOption.AllDirectories).ToArray())
                            {
                                if (Path.GetFileName(file).Equals(NewMatchingCabName, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    found = true;
                                    foundFilePath = file;
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            Console.WriteLine("PLEASE FILE A BUG REPORT NOW");
                            Console.WriteLine(MatchingCabName);
                        }
                        else
                        {
                            //Console.WriteLine("File: " + foundFilePath);
                            //Console.WriteLine("New Name: " + Path.Combine(destination_path, DestinationPath));

                            string newFile = Path.Combine(destination_path, DestinationPath);

                            if (newFile.Equals(foundFilePath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Already fine, abort
                                break;
                            }

                            if (!Directory.Exists(Path.GetDirectoryName(newFile)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(newFile));
                            }

                            File.Move(foundFilePath, newFile);
                        }
                    }
                    else
                    {
                        string DestinationPath = Package.PackageFile;

                        if (DestinationPath.StartsWith(@"\\?\"))
                        {
                            int indexOfPackages = DestinationPath.IndexOf("MSPackages");
                            if (indexOfPackages > -1)
                            {
                                DestinationPath = DestinationPath[indexOfPackages..];
                            }
                        }

                        if (DestinationPath.StartsWith(@"\\?\"))
                        {
                            DestinationPath = DestinationPath[4..];
                        }

                        if (DestinationPath[1] == ':')
                        {
                            DestinationPath = Path.Combine($"Drive{DestinationPath[0]}", DestinationPath[3..]);
                        }

                        bool found = false;
                        string foundFilePath = "";

                        string MatchingCabName = $"{Package.Identity.Owner}" +
                        $"{(string.IsNullOrEmpty(Package.Identity.Component) ? "" : $".{Package.Identity.Component}")}" +
                        $"{(string.IsNullOrEmpty(Package.Identity.SubComponent) ? "" : $".{Package.Identity.SubComponent}")}" +
                        $"{(string.IsNullOrEmpty(Package.Culture) == true ? "" : $"_Lang_{Package.Culture}")}.spkg";

                        foreach (string file in Directory.GetFiles(destination_path, "*.spkg", SearchOption.AllDirectories).ToArray())
                        {
                            if (Path.GetFileName(file).Equals(MatchingCabName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                found = true;
                                foundFilePath = file;
                                break;
                            }
                        }

                        if (!found)
                        {
                            Console.WriteLine("PLEASE FILE A BUG REPORT NOW");
                            Console.WriteLine(MatchingCabName);
                        }
                        else
                        {
                            //Console.WriteLine("File: " + foundFilePath);
                            //Console.WriteLine("New Name: " + Path.Combine(destination_path, DestinationPath));

                            string newFile = Path.Combine(destination_path, DestinationPath);

                            if (newFile.Equals(foundFilePath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Already fine, abort
                                break;
                            }

                            if (!Directory.Exists(Path.GetDirectoryName(newFile)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(newFile));
                            }

                            File.Move(foundFilePath, newFile);
                        }
                    }
                }
            }
        }

        public static void SortCabinets(IEnumerable<IDisk> disks, string destination_path)
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

                        if (fileSystem.DirectoryExists(@"Windows\ImageUpdate"))
                        {
                            string[] ImageUpdateFiles = fileSystem.GetFiles(@"Windows\ImageUpdate", "*", SearchOption.AllDirectories).ToArray();
                            foreach (string ImageUpdateFile in ImageUpdateFiles)
                            {
                                Console.WriteLine(ImageUpdateFile);

                                if (Path.GetFileName(ImageUpdateFile).Equals("UpdateHistory.xml", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    using Stream UpdateHistoryStream = fileSystem.OpenFile(ImageUpdateFile, FileMode.Open, FileAccess.Read);
                                    UpdateHistory.UpdateHistory UpdateHistory = UpdateHistoryStream.GetObjectFromXML<UpdateHistory.UpdateHistory>();

                                    SortCabinets(UpdateHistory, destination_path);
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

                                    SortCabinets(UpdateHistory, destination_path);
                                }
                            }
                        }

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

            Logging.Log();
            Logging.Log("Cleaning up...");
            Logging.Log();

            TempManager.CleanupTempFiles();
        }
    }
}

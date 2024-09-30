using DiscUtils;
using MobilePackageGen;
using MobilePackageGen.GZip;
using System.Text;
using System.Xml.Serialization;

namespace WPBuildInfo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"
Windows Phone Build Info Tool
Version: 1.0.6.0
");

            if (args.Length < 1)
            {
                PrintHelp();
                return;
            }

            IEnumerable<IDisk> disks = DiskLoader.LoadDisks(args);

            if (!disks.Any())
            {
                PrintHelp();
                return;
            }

            Logging.Log();

            FindBuildInfo(disks);
            Logging.Log();

            FindKernel(disks);
            Logging.Log();

            FindPackages(disks);
            Logging.Log();

            Console.WriteLine("The operation completed successfully.");
        }

        private static void FindBuildInfo(IEnumerable<IDisk> disks)
        {
            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    if (partition.FileSystem is IFileSystem fileSystem)
                    {
                        try
                        {
                            if (fileSystem.FileExists(@"Windows\System32\buildinfo.xml"))
                            {
                                using DiscUtils.Streams.SparseStream xmlstrm = fileSystem.OpenFile(@"Windows\System32\buildinfo.xml", FileMode.Open, FileAccess.Read);

                                BuildInfo.Buildinformation target = GetBuildInfoXml(xmlstrm);

                                string buildString = $"{target.Majorversion}.{target.Minorversion}.{target.Qfelevel}.XXX.{target.Releaselabel}({target.Builder}).{target.Buildtime}";

                                Logging.Log($"BuildInfo: WP ({partition.Name}): {buildString}");

                                if (!string.IsNullOrEmpty(target.Ntrazzlebuildnumber))
                                {
                                    string ntVersionString = $"{target.Ntrazzlemajorversion}.{target.Ntrazzleminorversion}.{target.Ntrazzlebuildnumber}.{target.Ntrazzlerevisionnumber} ({target.Releaselabel.ToLower()}.{target.Buildtime[2..]})";

                                    Logging.Log($"BuildInfo: NT ({partition.Name}): {ntVersionString}");
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private static void FindKernel(IEnumerable<IDisk> disks)
        {
            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    if (partition.FileSystem is IFileSystem fileSystem)
                    {
                        try
                        {
                            if (fileSystem.FileExists(@"Windows\System32\ntoskrnl.exe"))
                            {
                                using DiscUtils.Streams.SparseStream ntosstrm = fileSystem.OpenFile(@"Windows\System32\ntoskrnl.exe", FileMode.Open, FileAccess.Read);

                                byte[] buffer = new byte[ntosstrm.Length];
                                ntosstrm.Read(buffer, 0, buffer.Length);

                                Logging.Log($"NTOS: NT ({partition.Name}): {GetBuildNumberFromPE(buffer)}");
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private static void FindPackages(IEnumerable<IDisk> disks)
        {
            foreach (IDisk disk in disks)
            {
                foreach (IPartition partition in disk.Partitions)
                {
                    if (partition.FileSystem is IFileSystem fileSystem)
                    {
                        try
                        {
                            if (fileSystem.DirectoryExists(@"Windows\Packages\DsmFiles"))
                            {
                                IEnumerable<string> manifestFiles = fileSystem.GetFilesWithNtfsIssueWorkaround(@"Windows\Packages\DsmFiles", "*.xml", SearchOption.TopDirectoryOnly);

                                foreach (string manifestFile in manifestFiles)
                                {
                                    try
                                    {
                                        MobilePackageGen.XmlDsm.Package? dsm = null;

                                        try
                                        {
                                            using Stream stream = fileSystem.OpenFileAndDecompressAsGZip(manifestFile);
                                            XmlSerializer serializer = new(typeof(MobilePackageGen.XmlDsm.Package));
                                            dsm = (MobilePackageGen.XmlDsm.Package)serializer.Deserialize(stream)!;
                                        }
                                        catch (InvalidDataException)
                                        {
                                            using Stream stream = fileSystem.OpenFile(manifestFile, FileMode.Open, FileAccess.Read);
                                            XmlSerializer serializer = new(typeof(MobilePackageGen.XmlDsm.Package));
                                            dsm = (MobilePackageGen.XmlDsm.Package)serializer.Deserialize(stream)!;
                                        }

                                        Logging.Log($"{partition.Name},{GetSPKGComponentName(dsm)},{dsm.Identity.Version.Major}.{dsm.Identity.Version.Minor}.{dsm.Identity.Version.QFE}.{dsm.Identity.Version.Build}");
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                        catch
                        {

                        }

                        try
                        {
                            if (fileSystem.DirectoryExists(@"Windows\servicing\Packages"))
                            {
                                IEnumerable<string> manifestFiles = fileSystem.GetFilesWithNtfsIssueWorkaround(@"Windows\servicing\Packages", "*.mum", SearchOption.TopDirectoryOnly);

                                foreach (string manifestFile in manifestFiles)
                                {
                                    try
                                    {
                                        Stream stream = fileSystem.OpenFile(manifestFile, FileMode.Open, FileAccess.Read);
                                        XmlSerializer serializer = new(typeof(MobilePackageGen.XmlMum.Assembly));
                                        MobilePackageGen.XmlMum.Assembly cbs = (MobilePackageGen.XmlMum.Assembly)serializer.Deserialize(stream)!;

                                        string packageName = GetCBSComponentName(cbs);

                                        Logging.Log($"{partition.Name},{packageName},{cbs.AssemblyIdentity.Version}");
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        private static string GetCBSComponentName(MobilePackageGen.XmlMum.Assembly cbs)
        {
            return $"{cbs.AssemblyIdentity.Name}~{cbs.AssemblyIdentity.PublicKeyToken}~{cbs.AssemblyIdentity.ProcessorArchitecture}~{(cbs.AssemblyIdentity.Language == "neutral" ? "" : cbs.AssemblyIdentity.Language)}~{cbs.AssemblyIdentity.Version}";
        }

        private static string GetSPKGComponentName(MobilePackageGen.XmlDsm.Package dsm)
        {
            return $"{dsm.Identity.Owner}" +
                $"{(string.IsNullOrEmpty(dsm.Identity.Component) ? "" : $".{dsm.Identity.Component}")}" +
                $"{(string.IsNullOrEmpty(dsm.Identity.SubComponent) ? "" : $".{dsm.Identity.SubComponent}")}" +
                $"{(string.IsNullOrEmpty(dsm.Culture) == true ? "" : $"_Lang_{dsm.Culture}")}" +
                $"{(string.IsNullOrEmpty(dsm.Resolution) == true ? "" : $"_Res_{dsm.Resolution}")}";
        }

        public static string GetBuildNumberFromPE(byte[] peFile)
        {
            byte[] sign = new byte[] {
                0x46, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x65, 0x00, 0x56, 0x00, 0x65, 0x00, 0x72,
                0x00, 0x73, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            int fIndex = IndexOf(peFile, sign) + sign.Length;
            int lIndex = IndexOf(peFile, new byte[] { 0x00, 0x00, 0x00 }, fIndex) + 1;

            byte[] sliced = SliceByteArray(peFile, lIndex - fIndex, fIndex);

            return Encoding.Unicode.GetString(sliced);
        }

        private static byte[] SliceByteArray(byte[] source, int length, int offset)
        {
            byte[] destfoo = new byte[length];
            Array.Copy(source, offset, destfoo, 0, length);
            return destfoo;
        }

        private static int IndexOf(byte[] searchIn, byte[] searchFor, int offset = 0)
        {
            if ((searchIn != null) && (searchIn != null))
            {
                if (searchFor.Length > searchIn.Length)
                {
                    return 0;
                }

                for (int i = offset; i < searchIn.Length; i++)
                {
                    int startIndex = i;
                    bool match = true;
                    for (int j = 0; j < searchFor.Length; j++)
                    {
                        if (searchIn[startIndex] != searchFor[j])
                        {
                            match = false;
                            break;
                        }
                        else if (startIndex < searchIn.Length)
                        {
                            startIndex++;
                        }
                    }
                    if (match)
                    {
                        return startIndex - searchFor.Length;
                    }
                }
            }
            return -1;
        }

        public static BuildInfo.Buildinformation GetBuildInfoXml(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            XmlSerializer serializer = new(typeof(BuildInfo.Buildinformation));

            BuildInfo.Buildinformation package = (BuildInfo.Buildinformation)serializer.Deserialize(stream)!;

            return package;
        }

        private static void PrintHelp()
        {
            Logging.Log("Remember to run the tool as Trusted Installer (TI).");
            Logging.Log();
            Logging.Log("You need to pass at least 2 parameters:");
            Logging.Log(@"	<Path to MainOS/Data/EFIESP> <Output folder CBSs>");
            Logging.Log("Examples:");
            Logging.Log(@"	 D: C:\OutputCabs");
            Logging.Log(@"	 D: E: F: C:\OutputCabs");
            Logging.Log();
            Logging.Log("You need to pass 2 parameters:");
            Logging.Log(@"	<Path to FFU File> <Output folder CBSs>");
            Logging.Log("Example:");
            Logging.Log(@"	 D:\Flash.ffu C:\OutputCabs");
            Logging.Log();
            Logging.Log("You need to pass at least 2 parameters:");
            Logging.Log(@"	<Path to VHDx> <Output folder CBSs>");
            Logging.Log("Examples:");
            Logging.Log(@"	 D:\LUN0.vhdx C:\OutputCabs");
            Logging.Log(@"	 D:\LUN0.vhdx D:\LUN1.vhdx D:\LUN2.vhdx C:\OutputCabs");
            Logging.Log();
            Logging.Log("You need to pass 2 parameters:");
            Logging.Log(@"	<Path to WIM File> <Output folder CBSs>");
            Logging.Log("Example:");
            Logging.Log(@"	 D:\Flash.wim C:\OutputCabs");
        }
    }
}

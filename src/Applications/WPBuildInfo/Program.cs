using DiscUtils;
using MobilePackageGen;
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

                                Logging.Log($"WP ({partition.Name}): {buildString}");

                                if (!string.IsNullOrEmpty(target.Ntrazzlebuildnumber))
                                {
                                    string ntVersionString = $"{target.Ntrazzlemajorversion}.{target.Ntrazzleminorversion}.{target.Ntrazzlebuildnumber}.{target.Ntrazzlerevisionnumber} ({target.Releaselabel.ToLower()}.{target.Buildtime[2..]})";

                                    Logging.Log($"NT ({partition.Name}): {ntVersionString}");
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            Console.WriteLine("The operation completed successfully.");
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
            Console.WriteLine("Remember to run the tool as Trusted Installer (TI).");
            Console.WriteLine();
            Console.WriteLine("You need to pass at least 2 parameters:");
            Console.WriteLine(@"	<Path to MainOS/Data/EFIESP> <Output folder CBSs>");
            Console.WriteLine("Examples:");
            Console.WriteLine(@"	 D: C:\OutputCabs");
            Console.WriteLine(@"	 D: E: F: C:\OutputCabs");
            Console.WriteLine();
            Console.WriteLine("You need to pass 2 parameters:");
            Console.WriteLine(@"	<Path to FFU File> <Output folder CBSs>");
            Console.WriteLine("Example:");
            Console.WriteLine(@"	 D:\Flash.ffu C:\OutputCabs");
            Console.WriteLine();
            Console.WriteLine("You need to pass at least 2 parameters:");
            Console.WriteLine(@"	<Path to VHDx> <Output folder CBSs>");
            Console.WriteLine("Examples:");
            Console.WriteLine(@"	 D:\LUN0.vhdx C:\OutputCabs");
            Console.WriteLine(@"	 D:\LUN0.vhdx D:\LUN1.vhdx D:\LUN2.vhdx C:\OutputCabs");
            Console.WriteLine();
            Console.WriteLine("You need to pass 2 parameters:");
            Console.WriteLine(@"	<Path to WIM File> <Output folder CBSs>");
            Console.WriteLine("Example:");
            Console.WriteLine(@"	 D:\Flash.wim C:\OutputCabs");
        }
    }
}

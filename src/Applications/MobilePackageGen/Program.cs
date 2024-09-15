using MobilePackageGen.Adapters;

namespace MobilePackageGen
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(@"
Mobile Package Generator Tool
Version: 1.0.5.0
");

            if (args.Length < 2)
            {
                PrintHelp();
                return;
            }

            string[] inputArgs = args[..^1];
            string outputFolder = args[^1];

            Console.WriteLine("Getting Disks...");

            List<IDisk> disks = [];

            foreach (string inputArg in inputArgs)
            {
                if (Directory.Exists(inputArg))
                {
                    if (Directory.Exists(Path.Combine(inputArg, @"Windows\Servicing\Packages")) || Directory.Exists(Path.Combine(inputArg, @"Windows\Packages\DsmFiles")))
                    {
                        disks.Add(new Adapters.RealFileSystem.Disk(inputArg));
                    }
                    else
                    {
                        string[] files = Directory.EnumerateFiles(inputArg).ToArray();

                        foreach (string file in files)
                        {
                            IDisk? disk = LoadFile(file);
                            if (disk != null)
                            {
                                disks.Add(disk);
                            }
                        }
                    }
                }
                else if (File.Exists(inputArg))
                {
                    IDisk? disk = LoadFile(inputArg);
                    if (disk != null)
                    {
                        disks.Add(disk);
                    }
                }
            }

            if (disks.Count == 0)
            {
                PrintHelp();
                return;
            }

            Console.WriteLine("Getting Update OS Disks...");

            disks.AddRange(DiskCommon.GetUpdateOSDisks(disks));

            DiskCommon.PrintDiskInfo(disks);

            CBSBuilder.BuildCBS(disks, outputFolder);

            SPKGBuilder.BuildSPKG(disks, outputFolder);

            Console.WriteLine("The operation completed successfully.");
        }

        private static IDisk? LoadFile(string file)
        {
            string extension = Path.GetExtension(file);
            switch (extension.ToLowerInvariant())
            {
                case ".wim":
                    {
                        return new Adapters.Wim.Disk(file);
                    }
                case ".ffu":
                    {
                        return new Adapters.FullFlashUpdate.Disk(file);
                    }
                case ".vhd":
                    {
                        return new Adapters.Vhdx.Disk(file);
                    }
                case ".vhdx":
                    {
                        return new Adapters.Vhdx.Disk(file);
                    }
                case ".img":
                    {
                        return new Adapters.RawDisk.Disk(File.OpenRead(file));
                    }
                case ".bin":
                    {
                        return new Adapters.RawDisk.Disk(File.OpenRead(file));
                    }
                default:
                    {
                        return null;
                    }
            }
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
using MobilePackageGen;

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

            if (args.Length < 2)
            {
                PrintHelp();
                return;
            }

            string[] inputArgs = args[..^1];
            string outputFolder = args[^1];

            IEnumerable<IDisk> disks = DiskLoader.LoadDisks(inputArgs);

            if (!disks.Any())
            {
                PrintHelp();
                return;
            }



            Console.WriteLine("The operation completed successfully.");
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

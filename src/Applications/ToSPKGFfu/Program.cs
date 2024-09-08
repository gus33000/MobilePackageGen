using MobilePackageGen;
using MobilePackageGen.Adapters.FullFlashUpdate;

namespace ToSPKG
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(@"
Image To Software Package Cabinets tool
Version: 1.0.1.0
");

            if (args.Length < 2)
            {
                Console.WriteLine("Remember to run the tool as Trusted Installer (TI).");
                Console.WriteLine("You need to pass 2 parameters:");
                Console.WriteLine(@"	<Path to FFU File> <Output folder CBSs>");
                Console.WriteLine("Example:");
                Console.WriteLine(@"	 ""D:\Flash.ffu"" ""C:\OutputCabs\""");
                return;
            }

            Console.WriteLine("Getting Disks...");

            List<IDisk> disks = GetDisks(args[0]);

            Console.WriteLine("Getting Update OS Disks...");

            disks.AddRange(Disk.GetUpdateOSDisks(disks));

            SPKGBuilder.BuildSPKG(disks, args[1]);
        }

        private static List<IDisk> GetDisks(string ffuPath)
        {
            List<IDisk> disks =
            [
                new Disk(ffuPath)
            ];

            return disks;
        }
    }
}

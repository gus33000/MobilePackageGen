using ToCBS.Adapters.VhdxSpaces;

namespace ToCBS
{
    internal class Program
    {
        // Hardcoded, todo
        public const uint SectorSize = 512;

        private static void Main(string[] args)
        {
            Console.WriteLine(@"
Image To Component Based Servicing Cabinets tool
Version: 1.0.0.0
");

            if (args.Length < 2)
            {
                Console.WriteLine("Remember to run the tool as Trusted Installer (TI).");
                Console.WriteLine("You need to pass at least 2 parameters:");
                Console.WriteLine(@"	<Path to MainOS/Data/EFIESP> <Output folder CBSs>");
                Console.WriteLine("Examples:");
                Console.WriteLine(@"	 ""D:\LUN0.vhdx"" ""C:\OutputCabs\""");
                Console.WriteLine(@"	 ""D:\LUN0.vhdx"" ""D:\LUN1.vhdx"" ""D:\LUN2.vhdx"" ""C:\OutputCabs\""");
                return;
            }

            string[] vhds = args[..^1];
            if (vhds.Length == 1 && Directory.Exists(vhds[0]))
            {
                vhds = Directory.EnumerateFiles(vhds[0], "*.vhdx", SearchOption.TopDirectoryOnly).ToArray();
            }

            Console.WriteLine("Getting Disks...");

            List<IDisk> disks = GetDisks(vhds);

            Console.WriteLine("Getting Update OS Disks...");

            disks.AddRange(Disk.GetUpdateOSDisks(disks));

            CBSBuilder.BuildCBS(disks, args[^1]);
        }

        private static List<IDisk> GetDisks(string[] vhdxs)
        {
            List<IDisk> disks = [];

            foreach (string vhdx in vhdxs)
            {
                disks.Add(new Disk(vhdx, SectorSize)); // Hardcoded, todo
            }

            return disks;
        }
    }
}

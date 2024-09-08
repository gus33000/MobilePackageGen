namespace ToCBS
{
    internal class Program
    {
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
                Console.WriteLine(@"	 ""D:\"" ""C:\OutputCabs\""");
                Console.WriteLine(@"	 ""D:\"" ""E:\"" ""F:\"" ""C:\OutputCabs\""");
                return;
            }

            Console.WriteLine("Getting Disks...");

            List<Disk> disks = GetDisks(args[..^1]);

            CBSBuilder.BuildCBS(disks, args[^1]);
        }

        private static List<Disk> GetDisks(string[] paths)
        {
            List<Disk> disks = [];

            foreach (string path in paths)
            {
                disks.Add(new Disk(path)); // Hardcoded, todo
            }

            return disks;
        }
    }
}

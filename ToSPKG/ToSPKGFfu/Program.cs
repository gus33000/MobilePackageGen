using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToSPKG
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(@"
Image To Software Package Cabinets tool
Version: 1.0.0.0
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

            List<Disk> disks = GetDisks(args[0]);

            SPKGBuilder.BuildSPKG(disks, args[1]);
        }

        private static List<Disk> GetDisks(string ffuPath)
        {
            List<Disk> disks =
            [
                new Disk(ffuPath, 512) // Hardcoded, todo
            ];

            return disks;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToCBS
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("\nImage To Component Based Servicing Cabinets tool\nVersion: 1.0.0.0\n");

            if (args.Length < 2)
            {
                Console.WriteLine("Remember to run as TI.");
                Console.WriteLine("You need to pass 2 parameters:");
                Console.WriteLine("\t<Path to MainOS/Data/EFIESP> <Output folder CBSs>");
                Console.WriteLine("Example:");
                Console.WriteLine("\t \"D:\\\" \"C:\\RAMD\\\"");
                return;
            }

            string[] vhds = args[..^1];
            if (vhds.Length == 1 && Directory.Exists(vhds[0]))
            {
                vhds = Directory.EnumerateFiles(vhds[0], "*.vhdx", SearchOption.TopDirectoryOnly).ToArray();
            }

            Console.WriteLine("Getting Disks...");

            List<Disk> disks = GetDisks(vhds);

            CBSBuilder.BuildCBS(disks, args[^1]);
        }

        private static List<Disk> GetDisks(string[] vhdxs)
        {
            List<Disk> disks = new();

            foreach (string vhdx in vhdxs)
            {
                disks.Add(new Disk(vhdx, 4096)); // Hardcoded, todo
            }

            return disks;
        }
    }
}

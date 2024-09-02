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

            Console.WriteLine("Getting Disks...");

            List<Disk> disks = GetDisks(args[0]);

            CBSBuilder.BuildCBS(disks, args[1]);
        }

        private static List<Disk> GetDisks(string ffuPath)
        {
            List<Disk> disks = new()
            {
                new Disk(ffuPath, 4096) // Hardcoded, todo
            };

            return disks;
        }
    }
}

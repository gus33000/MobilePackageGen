using System;

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

            CBSBuilder.BuildCBS(args[0], args[1]);
        }
    }
}

namespace SecWim2Wim
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"
Secure WIM to WIM tool
Version: 1.0.6.0
");

            if (args.Length < 2)
            {
                Console.WriteLine("You need to pass 2 parameters:");
                Console.WriteLine(@"	<Path to Secure WIM> <Output WIM>");
                Console.WriteLine("Example:");
                Console.WriteLine(@"	 ""D:\install.secwim"" ""D:\install.wim""");
                return;
            }

            ConvertSECWIM2WIM(args[0], args[1]);
        }

        public static void ConvertSECWIM2WIM(string wimsec, string wim)
        {
            using FileStream? wimsecstream = File.OpenRead(wimsec);
            using FileStream? wimstream = File.OpenWrite(wim);
            using BinaryReader? wimsecreader = new(wimsecstream);
            using BinaryWriter? wimwriter = new(wimstream);

            byte[]? bytes =
            [
                0x4D, 0x53, 0x57, 0x49, 0x4D
            ];

            Console.WriteLine("(wimsec2wim) Finding Magic Bytes...");

            long start = FindPosition(wimsecstream, bytes);

            Console.WriteLine($"(wimsec2wim) Found Magic Bytes at {start}");

            Console.WriteLine("(wimsec2wim) Finding WIM XML Data...");

            byte[]? endbytes =
            [
                0x3C, 0x00, 0x2F, 0x00, 0x57, 0x00, 0x49, 0x00, 0x4D, 0x00, 0x3E, 0x00
            ];

            wimsecstream.Seek(start + 72, SeekOrigin.Begin);
            byte[] buffer = new byte[24];
            wimsecstream.Read(buffer, 0, 24);
            long may = ToInt64LittleEndian(buffer, 8);
            wimsecstream.Seek(start, SeekOrigin.Begin);

            Console.WriteLine($"(wimsec2wim) Found WIM XML Data at {start}{may}{2}");

            Console.WriteLine($"(wimsec2wim) Writing {may}{2} bytes...");

            wimwriter.Write(wimsecreader.ReadBytes((int)may + 2));

            Console.WriteLine($"(wimsec2wim) Written {may}{2} bytes");

            Console.WriteLine("(wimsec2wim) Writing WIM XML Data...");

            for (long i = wimsecstream.Position; i < wimsecstream.Length - endbytes.Length; i++)
            {
                if (BitConverter.ToString(wimsecreader.ReadBytes(12)) == BitConverter.ToString(endbytes))
                {
                    wimwriter.Write(endbytes);
                    break;
                }
                wimsecstream.Seek(-12, SeekOrigin.Current);
                wimwriter.Write(wimsecreader.ReadBytes(1));
            }

            Console.WriteLine("(wimsec2wim) Written WIM XML Data");
            Console.WriteLine("(wimsec2wim) Done.");
        }

        public static long ToInt64LittleEndian(byte[] buffer, int offset)
        {
            return (long)ToUInt64LittleEndian(buffer, offset);
        }

        public static uint ToUInt32LittleEndian(byte[] buffer, int offset)
        {
            return (uint)(((buffer[offset + 3] << 24) & 0xFF000000U) | ((buffer[offset + 2] << 16) & 0x00FF0000U)
                | ((buffer[offset + 1] << 8) & 0x0000FF00U) | ((buffer[offset + 0] << 0) & 0x000000FFU));
        }

        public static ulong ToUInt64LittleEndian(byte[] buffer, int offset)
        {
            return (((ulong)ToUInt32LittleEndian(buffer, offset + 4)) << 32) | ToUInt32LittleEndian(buffer, offset + 0);
        }

        //
        // https://stackoverflow.com/questions/1471975/best-way-to-find-position-in-the-stream-where-given-byte-sequence-starts
        //
        public static long FindPosition(Stream stream, byte[] byteSequence)
        {
            if (byteSequence.Length > stream.Length)
            {
                return -1;
            }

            byte[] buffer = new byte[byteSequence.Length];

            BufferedStream bufStream = new(stream, byteSequence.Length);
            int i;

            while ((i = bufStream.Read(buffer, 0, byteSequence.Length)) == byteSequence.Length)
            {
                if (byteSequence.SequenceEqual(buffer))
                {
                    return bufStream.Position - byteSequence.Length;
                }
                else
                {
                    bufStream.Position -= byteSequence.Length - PadLeftSequence(buffer, byteSequence);
                }
            }

            return -1;
        }

        private static int PadLeftSequence(byte[] bytes, byte[] seqBytes)
        {
            int i = 1;
            while (i < bytes.Length)
            {
                int n = bytes.Length - i;
                byte[] aux1 = new byte[n];
                byte[] aux2 = new byte[n];
                Array.Copy(bytes, i, aux1, 0, n);
                Array.Copy(seqBytes, aux2, n);
                if (aux1.SequenceEqual(aux2))
                {
                    return i;
                }

                i++;
            }
            return i;
        }
    }
}

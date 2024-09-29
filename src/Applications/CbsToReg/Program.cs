using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace CbsToReg
{
    internal class Program
    {
        private static readonly string SystemKeyName = "RTSYSTEM";
        private static readonly string SoftwareKeyName = "RTSOFTWARE";

        internal static void Main(string[] args)
        {
            Console.WriteLine(@"
CBS to REG tool
Version: 1.0.6.0
");

            if (args.Length < 1)
            {
                Console.WriteLine("You need to pass 1 parameter:");
                Console.WriteLine(@"	<Path to mum file>");
                Console.WriteLine("Example:");
                Console.WriteLine(@"	 ""D:\mymanifest.mum""");
                return;
            }

            HandleCbsFile(args[0]);
        }

        private static void HandleCbsFile(string file)
        {
            Stream? stream = null;
            Mum.Assembly cbs;
            XmlSerializer serializer = new(typeof(Mum.Assembly));

            try
            {
                stream = File.OpenRead(file);
                cbs = (Mum.Assembly)serializer.Deserialize(stream)!;
            }
            catch
            {
                // LibSxS is only supported on Windows
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new NotImplementedException("The compression algorithm this data block uses is not currently implemented.");
                }

                stream = LibSxS.Delta.DeltaAPI.LoadManifest(File.OpenRead(file));

                byte[] manifestBuffer = new byte[stream.Length];
                stream.Read(manifestBuffer, 0, manifestBuffer.Length);
                stream.Seek(0, SeekOrigin.Begin);

                Console.WriteLine($"Exporting to {file}.expanded");
                File.WriteAllBytes($"{file}.expanded", manifestBuffer);

                cbs = (Mum.Assembly)serializer.Deserialize(stream)!;
            }

            if (cbs.RegistryKeys != null && cbs.RegistryKeys.Count > 0)
            {
                CbsToReg regExporter = new();

                foreach (Mum.RegistryKey regKey in cbs.RegistryKeys)
                {
                    List<RegistryValue> keyValues = new(regKey.RegistryValues.Count);
                    foreach (Mum.RegistryValue keyValue in regKey.RegistryValues)
                    {
                        keyValues.Add(new RegistryValue(keyValue.Name, keyValue.Value, keyValue.ValueType,
                                                        keyValue.Mutable, keyValue.OperationHint));
                    }

                    regExporter.Add(new RegistryCollection(regKey.KeyName, keyValues));
                }

                string reg = regExporter.Build(SoftwareKeyName, SystemKeyName);

                Console.WriteLine($"Exporting to {file}.reg");
                File.WriteAllText($"{file}.reg", reg, Encoding.UTF8);
            }

            stream?.Close();
        }
    }
}
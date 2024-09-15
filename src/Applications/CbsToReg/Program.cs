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
            LibSxS.Delta.DeltaAPI.wcpBasePath = Path.Combine(Directory.GetCurrentDirectory(), "manifest.bin");

            Console.WriteLine(@"
CBS to REG tool
Version: 1.0.4.0
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
                string manifestString = LibSxS.Delta.DeltaAPI.GetManifest(file);
                StringReader stringReader = new(manifestString);
                cbs = (Mum.Assembly)serializer.Deserialize(stringReader)!;
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
                File.WriteAllText($"{file}.reg", reg, Encoding.Unicode);
            }

            stream?.Close();
        }
    }
}
﻿using System.Text;
using System.Xml.Serialization;

namespace CbsToReg
{
    internal class Program
    {
        private static readonly string SystemKeyName = "RTSYSTEM";
        private static readonly string SoftwareKeyName = "RTSOFTWARE";

        internal static void Main(string[] args)
        {
            HandleCbsFile(args[0]);
        }

        private static void HandleCbsFile(string file)
        {
            Stream stream = File.OpenRead(file);

            XmlSerializer serializer = new(typeof(Mum.Assembly));
            Mum.Assembly cbs = (Mum.Assembly)serializer.Deserialize(stream);

            if (cbs.RegistryKeys != null && cbs.RegistryKeys.Count > 0)
            {
                CbsToReg regExporter = new();

                foreach (Mum.RegistryKey? regKey in cbs.RegistryKeys)
                {
                    List<RegistryValue>? keyValues = new(regKey.RegistryValues.Count);
                    foreach (Mum.RegistryValue? keyValue in regKey.RegistryValues)
                    {
                        keyValues.Add(new RegistryValue(keyValue.Name, keyValue.Value, keyValue.ValueType,
                                                        keyValue.Mutable, keyValue.OperationHint));
                    }

                    regExporter.Add(new RegistryCollection(regKey.KeyName, keyValues));
                }

                string reg = regExporter.Build(SoftwareKeyName, SystemKeyName);
                File.WriteAllText(file + ".reg", reg, Encoding.Unicode);
            }

            stream.Close();
        }
    }
}
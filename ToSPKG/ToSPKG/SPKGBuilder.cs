using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO.Compression;
using Microsoft.Deployment.Compression.Cab;
using System.Threading.Tasks;

namespace ToSPKG
{
    public class SPKGBuilder
    {
        const string TMP_PATH = @"P:\spkg\tmp";

        public static void BuildSPKG()
        {
            const string PATH = @"X:\"; //TODO: REMOVE
            const string DEST = @"P:\spkg\fold";
            const string DEST_CAB = @"P:\spkg\cabs";

            var packages_path = Path.Combine(PATH, @"Windows\Packages");
            var winsxs_manifests_path = Path.Combine(PATH, @"Windows\WinSxS\Manifests");

            var xmls = Directory.EnumerateFiles(Path.Combine(packages_path, "DsmFiles"), "*.xml");

            foreach (var xml in xmls)
            {
                DecompressTo(xml, Path.Combine(TMP_PATH, xml.Split('\\').Last()));

                Stream stream = File.OpenRead(Path.Combine(TMP_PATH, xml.Split('\\').Last()));
                XmlSerializer serializer = new XmlSerializer(typeof(XmlDsm.Package));
                XmlDsm.Package xmldsm = (XmlDsm.Package)serializer.Deserialize(stream);

                var current_folder = Path.Combine(DEST, xml.Split('\\').Last().Replace(".xml", "").Replace(".dsm", ""));

                if (!Directory.Exists(current_folder))
                    Directory.CreateDirectory(current_folder);

                if (xmldsm.Files?.FileEntry?.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nPROCESSING: " + xml.Split('\\').Last().Replace(".xml", "").Replace(".dsm", ""));
                    Console.ResetColor();

                    xmldsm.Files.FileEntry.ForEach(xmlFile => {
                        var normalized = xmlFile.DevicePath;

                        if (xmlFile.DevicePath.StartsWith("\\"))
                            normalized = xmlFile.DevicePath.Substring(1);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Copying: " + normalized);
                        Console.ResetColor();

                        if (xmlFile.FileType.ToLower().Contains("registry") || xmlFile.FileType.ToLower().Contains("policy") || xmlFile.FileType.ToLower().Contains("manifest"))
                            CreateDirectoryWhileCopying(Path.Combine(PATH, normalized), Path.Combine(current_folder, xmlFile.CabPath), true);
                        else if (xmlFile.FileType.ToLower().Contains("catalog") || xmlFile.FileType.ToLower().Contains("regular"))
                            CreateDirectoryWhileCopying(Path.Combine(PATH, normalized), Path.Combine(current_folder, xmlFile.CabPath));
                        else if (xmlFile.FileType.ToLower().Contains("binarypartition")) //MEH
                            CreateDirectoryWhileCopying(Path.Combine(PATH, normalized), Path.Combine(current_folder, xmlFile.CabPath));
                        else
                            Debugger.Break();
                    });
                }
            }

            var folders = Directory.EnumerateDirectories(DEST, "*", SearchOption.TopDirectoryOnly);

            Parallel.ForEach(folders, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, folder =>
            {
                try
                {
                    Console.WriteLine("BUILDING FOR: " + folder.Split('\\').Last() + ".spkg");
                    var cab = new CabInfo(Path.Combine(DEST_CAB, folder.Split('\\').Last() + ".spkg"));
                    cab.Pack(folder, true, Microsoft.Deployment.Compression.CompressionLevel.Max, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERRORE: CREAZIONE CAB FALLITA: " + ex.Message);

                }
            });
           
        }

        private static void DecompressTo(string sourceFile, string destFile)
        {
            using (Stream fd = File.Create(destFile))
            using (Stream fs = File.OpenRead(sourceFile))
            using (Stream csStream = new GZipStream(fs, CompressionMode.Decompress))
            {
                byte[] buffer = new byte[4096];
                int nRead;
                while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
                    fd.Write(buffer, 0, nRead);
            }
        }

        private static void CreateDirectoryWhileCopying(string source, string dest, bool decompress = false)
        {
            var dirFromDest = Path.GetDirectoryName(dest);
            if (!Directory.Exists(dirFromDest))
                Directory.CreateDirectory(dirFromDest);

            try
            {
                if (!decompress)
                    File.Copy(source, dest, true);
                else
                    DecompressTo(source, dest);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("FILED TO COPY " + source + " --> " + ex.Message);
                Console.ResetColor();
            }
        }
    }
}

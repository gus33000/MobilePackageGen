namespace StorageSpace
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string OSPoolPath = @"C:\Users\gus33\Documents\GitHub\Ffu2Vhdx\publish\artifacts\win-arm64\CLI\2.OSPool.img";

            using FileStream fileStream = File.OpenRead(OSPoolPath);
            using BinaryReader reader = new(fileStream);

            byte[] spaceDbHeader = reader.ReadBytes(8);
            string spaceDbHeaderStr = System.Text.Encoding.ASCII.GetString(spaceDbHeader, 0, spaceDbHeader.Length);

            if (spaceDbHeaderStr != "SPACEDB ")
            {
                throw new Exception("Invalid OSPool!");
            }

            Console.WriteLine($"Postition Before: 0x{fileStream.Position:X}");
            fileStream.Seek(0x18, SeekOrigin.Current);
            Console.WriteLine($"Postition After: 0x{fileStream.Position:X}");

            Guid storagePoolUUID = new(reader.ReadBytes(16));
            Guid physicalDiskUUID = new(reader.ReadBytes(16));

            Console.WriteLine($"Storage Pool UUID: {storagePoolUUID}");
            Console.WriteLine($"Physical Disk UUID: {physicalDiskUUID}");

            int SDBCOffset = 0x1000;

            Console.WriteLine($"Postition Before: 0x{fileStream.Position:X}");
            fileStream.Seek(SDBCOffset, SeekOrigin.Begin);
            Console.WriteLine($"Postition After: 0x{fileStream.Position:X}");

            byte[] sdBcHeader = reader.ReadBytes(8);
            string sdBcHeaderStr = System.Text.Encoding.ASCII.GetString(sdBcHeader, 0, sdBcHeader.Length);

            if (sdBcHeaderStr != "SDBC    ")
            {
                throw new Exception("Invalid SDBC!");
            }

            Console.WriteLine($"Postition Before: 0x{fileStream.Position:X}");
            fileStream.Seek(8, SeekOrigin.Current);
            Console.WriteLine($"Postition After: 0x{fileStream.Position:X}");

            Guid sdbcStoragePoolUUID = new(reader.ReadBytes(16));

            Console.WriteLine($"Storage Pool UUID (SDBC): {sdbcStoragePoolUUID}");

            if (sdbcStoragePoolUUID != storagePoolUUID)
            {
                throw new Exception("Invalid OSPool! SDBC is not for the given SpaceDB!");
            }

            Console.WriteLine($"Postition Before: 0x{fileStream.Position:X}");
            fileStream.Seek(4, SeekOrigin.Current);
            Console.WriteLine($"Postition After: 0x{fileStream.Position:X}");

            uint sdbbEntrySize = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
            uint nextSdbbEntryNumber = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());

            Console.WriteLine($"SDBB Entry Size: 0x{sdbbEntrySize:X}");
            Console.WriteLine($"Next SDBB Entry Number: 0x{nextSdbbEntryNumber:X}");

            Console.WriteLine($"Postition Before: 0x{fileStream.Position:X}");
            fileStream.Seek(0x1C, SeekOrigin.Current);
            Console.WriteLine($"Postition After: 0x{fileStream.Position:X}");

            DateTime sdbbEntryModifiedTime = DateTime.FromFileTime(BitConverter.ToInt64(reader.ReadBytes(8).Reverse().ToArray()));

            Console.WriteLine($"SDBB Entry Modified Time: {sdbbEntryModifiedTime}");

            Console.WriteLine($"Postition Before: 0x{fileStream.Position:X}");
            fileStream.Seek(SDBCOffset + 8 * sdbbEntrySize, SeekOrigin.Begin);
            Console.WriteLine($"Postition After: 0x{fileStream.Position:X}");

            List<byte[]> sdbbEntryType1 = new List<byte[]>();
            List<byte[]> sdbbEntryType2 = new List<byte[]>();
            List<byte[]> sdbbEntryType3 = new List<byte[]>();
            List<byte[]> sdbbEntryType4 = new List<byte[]>();

            Dictionary<int, byte[]> entryDataList = [];

            for (int i = 8; i < nextSdbbEntryNumber; i++)
            {
                fileStream.Seek(8, SeekOrigin.Current);
                int entryIndex = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray());
                fileStream.Seek(2, SeekOrigin.Current);
                short entryDataCount = BitConverter.ToInt16(reader.ReadBytes(2).Reverse().ToArray());

                if (entryDataCount == 0) // Empty Entry
                {
                    throw new Exception("An entry exists which is empty, this is abnormal");
                }

                byte[] entryDataPart = reader.ReadBytes(0x30);

                if (entryDataList.ContainsKey(entryIndex))
                {
                    entryDataList[entryIndex] = entryDataList[entryIndex].Concat(entryDataPart).ToArray();
                }
                else
                {
                    entryDataList[entryIndex] = entryDataPart;
                }
            }

            for (int i = 8; i < nextSdbbEntryNumber; i++)
            {
                if (!entryDataList.ContainsKey(i))
                {
                    continue;
                }

                byte[] entry = entryDataList[i];

                nint entryType = entry[0];
                
                int entryDataLength = BitConverter.ToInt32(entry.Skip(0x04).Take(4).Reverse().ToArray(), 0);

                byte[] entryData = entry.Skip(8).Take(entryDataLength).ToArray();

                switch (entryType)
                {
                    case 1:
                        sdbbEntryType1.Add(entryData);
                        break;
                    case 2:
                        sdbbEntryType2.Add(entryData);
                        break;
                    case 3:
                        sdbbEntryType3.Add(entryData);
                        break;
                    case 4:
                        sdbbEntryType4.Add(entryData);
                        break;
                    default:
                        throw new Exception($"Unknown Entry Type! {entryType}");
                }
            }

            Console.WriteLine($"Type 1 Count: {sdbbEntryType1.Count}"); // Storage Pool Information
            Console.WriteLine($"Type 2 Count: {sdbbEntryType2.Count}"); // Disk Information
            Console.WriteLine($"Type 3 Count: {sdbbEntryType3.Count}"); // Virtual Disks Information
            Console.WriteLine($"Type 4 Count: {sdbbEntryType4.Count}"); // Data Record Information

            for (int k = 0; k < sdbbEntryType1.Count; k++)
            {
                File.WriteAllBytes($"SDBB1_{k}.bin", sdbbEntryType1[k]);
            }

            for (int k = 0; k < sdbbEntryType2.Count; k++)
            {
                File.WriteAllBytes($"SDBB2_{k}.bin", sdbbEntryType2[k]);
            }

            for (int k = 0; k < sdbbEntryType3.Count; k++)
            {
                File.WriteAllBytes($"SDBB3_{k}.bin", sdbbEntryType3[k]);

                using Stream dataStream = new MemoryStream(sdbbEntryType3[k]);
                using BinaryReader dataReader = new BinaryReader(dataStream);

                dataStream.Seek(0x15, SeekOrigin.Begin);
                ushort VirtualDiskNameLength = dataReader.ReadUInt16();

                string VirtualDiskName = System.Text.Encoding.Unicode.GetString(dataReader.ReadBytes(VirtualDiskNameLength * 2));
                Console.WriteLine($"DiskName ({VirtualDiskNameLength}): {VirtualDiskName}");
            }

            Console.WriteLine($"Postition: 0x{fileStream.Position:X}");
        }
    }
}

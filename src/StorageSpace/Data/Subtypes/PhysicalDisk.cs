using System.Text;

namespace StorageSpace.Data.Subtypes
{
    public class PhysicalDisk
    {
        public static void ParseEntryType2(List<byte[]> sdbbEntryType2, Dictionary<int, Disk> parsedDisks)
        {
            foreach (byte[] sdbbEntry in sdbbEntryType2)
            {
                int tempOffset = 0;
                int dataRecordLen = sdbbEntry[tempOffset];
                int physicalDiskId = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());

                tempOffset += sdbbEntry[tempOffset] + 1;
                tempOffset += sdbbEntry[tempOffset] + 1;

                byte[] physicalDiskUuid = sdbbEntry.Skip(tempOffset).Take(0x10).ToArray();
                tempOffset += 0x10;

                int physicalDiskNameLength = BitConverter.ToUInt16(sdbbEntry.Skip(tempOffset).Take(0x02).Reverse().ToArray(), 0);
                tempOffset += 0x02;

                byte[] physicalDiskName = new byte[physicalDiskNameLength * 2];
                byte[] tempPhysicalDiskName = sdbbEntry.Skip(tempOffset).Take(physicalDiskNameLength * 2).ToArray();
                tempOffset += physicalDiskNameLength * 2;
                for (int j = 0; j < physicalDiskNameLength * 2; j += 2)
                {
                    physicalDiskName[j] = tempPhysicalDiskName[j + 1];
                    physicalDiskName[j + 1] = tempPhysicalDiskName[j];
                }
                tempOffset += 6;

                dataRecordLen = sdbbEntry[tempOffset];
                int physicalDiskBlockNumber = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());

                int diskId = physicalDiskId;
                Guid diskUuid = new(physicalDiskUuid);
                string diskName = Encoding.Unicode.GetString(physicalDiskName);
                int diskBlockNumber = physicalDiskBlockNumber;

                if (!parsedDisks.ContainsKey(diskId))
                {
                    parsedDisks.Add(diskId, new Disk());
                }

                parsedDisks[diskId].ID = diskId;
                parsedDisks[diskId].UUID = diskUuid;
                parsedDisks[diskId].Name = diskName;
                parsedDisks[diskId].TotalBlocks = diskBlockNumber;
            }
        }

        private static int BigEndianToInt(byte[] buf)
        {
            int val = 0;
            for (int i = 0; i < buf.Length; i++)
            {
                val *= 0x100;
                val += buf[i];
            }
            return val;
        }
    }
}

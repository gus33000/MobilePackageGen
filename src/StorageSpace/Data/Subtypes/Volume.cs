using System.Text;

namespace StorageSpace.Data.Subtypes
{
    public class Volume
    {
        public static void ParseEntryType3(List<byte[]> sdbbEntryType3, Dictionary<int, Disk> parsedDisks)
        {
            foreach (byte[] SDBBVolume in sdbbEntryType3)
            {
                /*int tempOffset = 0;
                int dataRecordLen = sdbbEntry[tempOffset];
                int virtualDiskId = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += 0x02;

                if (sdbbEntry[tempOffset] == 0x01)
                {
                    tempOffset += sdbbEntry[tempOffset] + 1;
                }

                byte[] virtualDiskUuid = sdbbEntry.Skip(tempOffset).Take(0x10).ToArray();
                tempOffset += 0x10;

                int virtualDiskNameLength = BitConverter.ToUInt16(sdbbEntry.Skip(tempOffset).Take(0x02).Reverse().ToArray(), 0);
                tempOffset += 0x02;*/


                int tempOffset = 0;

                byte DataLength = SDBBVolume[0];
                int Data = BigEndianToInt(SDBBVolume.Skip(1).Take(DataLength).ToArray());

                byte DataLength2 = SDBBVolume[1 + DataLength];

                int CommandSerialNumber = BigEndianToInt(SDBBVolume.Skip(1 + DataLength + 1).Take(DataLength2).ToArray());

                Guid VolumeGUID = new(SDBBVolume.Skip(1 + DataLength + 1 + DataLength2).Take(16).ToArray());

                /*tempOffset += 0x02;

                if (SDBBVolume[tempOffset] == 0x01)
                {
                    tempOffset += SDBBVolume[tempOffset] + 1;
                }

                tempOffset += 0x10;*/

                int VolumeLengthName = BitConverter.ToUInt16(SDBBVolume.Skip(1 + DataLength + 1 + DataLength2 + 16).Take(0x02).Reverse().ToArray(), 0);

                if (VolumeLengthName == 0)
                {
                    continue;
                }

                tempOffset = 1 + DataLength + 1 + DataLength2 + 16 + 0x02;

                byte[] virtualDiskName = new byte[VolumeLengthName * 2];
                byte[] tempVirtualDiskName = SDBBVolume.Skip(tempOffset).Take(VolumeLengthName * 2).ToArray();
                tempOffset += VolumeLengthName * 2;
                for (int j = 0; j < VolumeLengthName * 2; j += 2)
                {
                    virtualDiskName[j] = tempVirtualDiskName[j + 1];
                    virtualDiskName[j + 1] = tempVirtualDiskName[j];
                }

                int diskDescriptionLength = BitConverter.ToUInt16(SDBBVolume.Skip(tempOffset).Take(0x02).Reverse().ToArray(), 0);
                tempOffset += 0x02;

                byte[] diskDescription = new byte[diskDescriptionLength * 2];
                byte[] tempDiskDescription = SDBBVolume.Skip(tempOffset).Take(diskDescriptionLength * 2).ToArray();
                tempOffset += diskDescriptionLength * 2;
                for (int j = 0; j < diskDescriptionLength * 2; j += 2)
                {
                    diskDescription[j] = tempDiskDescription[j + 1];
                    diskDescription[j + 1] = tempDiskDescription[j];
                }

                tempOffset += 3;

                int virtualDiskBlockNumber = 0;

                int dataRecordLen = SDBBVolume[tempOffset];
                dataRecordLen -= 3;
                virtualDiskBlockNumber = BigEndianToInt(SDBBVolume.Skip(tempOffset + 1).Take(dataRecordLen).ToArray()) / 0x10;

                if (virtualDiskBlockNumber == 0)
                {
                    dataRecordLen = SDBBVolume[tempOffset];
                    virtualDiskBlockNumber = BigEndianToInt(SDBBVolume.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                }

                int diskId = Data;
                Guid diskUuid = VolumeGUID;
                string diskName = Encoding.Unicode.GetString(virtualDiskName);
                int diskBlockNumber = virtualDiskBlockNumber;

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

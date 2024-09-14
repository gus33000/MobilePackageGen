namespace StorageSpace.Data.Subtypes
{
    public class SlabAllocation
    {
        public static void ParseEntryType4(List<byte[]> sdbbEntryType4, Dictionary<int, Disk> parsedDisks)
        {
            foreach (byte[] sdbbEntry in sdbbEntryType4)
            {
                int tempOffset = 0;

                tempOffset += sdbbEntry[tempOffset] + 1;
                tempOffset += sdbbEntry[tempOffset] + 1;
                tempOffset += sdbbEntry[tempOffset] + 1;
                tempOffset += sdbbEntry[tempOffset] + 1;
                tempOffset += sdbbEntry[tempOffset] + 1;

                int dataRecordLen = sdbbEntry[tempOffset];
                int virtual_disk_id = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += sdbbEntry[tempOffset] + 1;

                dataRecordLen = sdbbEntry[tempOffset];
                int virtual_disk_block_number = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += sdbbEntry[tempOffset] + 1;

                dataRecordLen = sdbbEntry[tempOffset];
                int parity_sequence_number = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += sdbbEntry[tempOffset] + 1;

                dataRecordLen = sdbbEntry[tempOffset];
                int mirror_sequence_number = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += sdbbEntry[tempOffset] + 1;

                tempOffset += sdbbEntry[tempOffset] + 1;

                dataRecordLen = sdbbEntry[tempOffset];
                int physical_disk_id = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += sdbbEntry[tempOffset] + 1;

                dataRecordLen = sdbbEntry[tempOffset];
                int physical_disk_block_number = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += sdbbEntry[tempOffset] + 1;

                if (!parsedDisks.TryGetValue(virtual_disk_id, out Disk? value))
                {
                    value = new Disk();
                    parsedDisks.Add(virtual_disk_id, value);
                }

                value.sdbbEntryType4.Add(new DataEntry()
                {
                    mirror_sequence_number = mirror_sequence_number,
                    parity_sequence_number = parity_sequence_number,
                    physical_disk_block_number = physical_disk_block_number,
                    physical_disk_id = physical_disk_id,
                    virtual_disk_block_number = virtual_disk_block_number,
                    virtual_disk_id = virtual_disk_id,
                });
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

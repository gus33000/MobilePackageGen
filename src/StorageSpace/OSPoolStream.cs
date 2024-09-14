using System.Text;

namespace StorageSpace
{
    public class OSPoolStream : Stream
    {
        private readonly Stream stream;
        private readonly Dictionary<int, Disk> parsedDisks = [];
        private readonly Disk store;
        private readonly long length;
        private readonly long blockSize = 0x100000;
        private readonly Dictionary<long, int> blockTable;
        private readonly StorageSpace storageSpace;

        private long currentPosition = 0;

        private readonly long ogSeek;

        public OSPoolStream(Stream stream, ulong storeIndex)
        {
            this.stream = stream;

            storageSpace = new(stream);

            ParseEntryType2(storageSpace.SDBBPhysicalDisks);
            ParseEntryType3(storageSpace.SDBBVolumes);
            ParseEntryType4(storageSpace.SDBBSlabAllocation);

            store = parsedDisks[(int)storeIndex];

            (length, blockTable) = BuildBlockTable();
        }

        public static Dictionary<int, string> GetDisks(Stream stream)
        {
            Dictionary<int, string> disks = [];

            StorageSpace storageSpace = new(stream);

            foreach (byte[] SDBBVolume in storageSpace.SDBBVolumes)
            {
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

                int diskId = Data;
                string diskName = Encoding.Unicode.GetString(virtualDiskName);

                disks.Add(diskId, diskName);
            }

            return disks;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        private (long, Dictionary<long, int>) BuildBlockTable()
        {
            Dictionary<long, int> blockTable = [];

            long blockSize = 0x10000000;

            foreach (DataEntry sdbbEntry in store.sdbbEntryType4)
            {
                int virtualDiskBlockNumber = sdbbEntry.virtual_disk_block_number;
                int physicalDiskBlockNumber = sdbbEntry.physical_disk_block_number;

                blockTable.Add(virtualDiskBlockNumber, physicalDiskBlockNumber);
            }

            long totalBlocks = store.TotalBlocks;

            return (totalBlocks * blockSize, blockTable);
        }

        private int GetBlockDataIndex(long realBlockOffset)
        {
            if (blockTable.ContainsKey(realBlockOffset))
            {
                return blockTable[realBlockOffset];
            }

            return -1; // Invalid
        }

        public override long Position
        {
            get => currentPosition;
            set
            {
                if (currentPosition < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                // Workaround for malformed MBRs
                /*if (currentPosition > Length)
                {
                    throw new EndOfStreamException();
                }*/

                currentPosition = value;
            }
        }

        public override void Flush()
        {
            // Nothing to do here
        }

        private byte[] ImageGetStoreDataBlock(int physicalDiskBlockNumber)
        {
            byte[] buffer = new byte[blockSize];

            long physicalDiskLocation = physicalDiskBlockNumber * blockSize + 0x2000 + 0x4000000;

            stream.Seek(ogSeek + physicalDiskLocation, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("The sum of offset and count is greater than the buffer length.");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // Workaround for malformed MBRs
            if (Position >= Length)
            {
                return count;
            }

            long readBytes = count;

            if (Position + readBytes > Length)
            {
                readBytes = (int)(Length - Position);
            }

            byte[] readBuffer = new byte[readBytes];
            Array.Fill<byte>(readBuffer, 0);

            // Read the buffer from the FFU file.
            // First we have to figure out where do we land here.

            long overflowBlockStartByteCount = Position % blockSize;
            long overflowBlockEndByteCount = (Position + readBytes) % blockSize;

            long startBlockIndex = (Position - overflowBlockStartByteCount) / blockSize;
            long endBlockIndex = (Position + readBytes + (blockSize - overflowBlockEndByteCount)) / blockSize;

            byte[] allReadBlocks = new byte[(endBlockIndex - startBlockIndex + 1) * blockSize];

            for (long currentBlock = startBlockIndex; currentBlock < endBlockIndex; currentBlock++)
            {
                int virtualBlockIndex = GetBlockDataIndex(currentBlock);
                if (virtualBlockIndex != -1)
                {
                    byte[] block = ImageGetStoreDataBlock(virtualBlockIndex);
                    Array.Copy(block, 0, allReadBlocks, (int)((currentBlock - startBlockIndex) * blockSize), blockSize);
                }
            }

            Array.Copy(allReadBlocks, overflowBlockStartByteCount, buffer, offset, readBytes);

            Position += readBytes;

            if (Position == Length)
            {
                // Workaround for malformed MBRs
                //return 0;
            }

            return (int)readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        Position = offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        Position += offset;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        Position = Length + offset;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException(nameof(origin));
                    }
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            stream.Dispose();
        }

        private void ParseEntryType2(List<byte[]> sdbbEntryType2)
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

        private void ParseEntryType3(List<byte[]> sdbbEntryType3)
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

        private void ParseEntryType4(List<byte[]> sdbbEntryType4)
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

                if (!parsedDisks.ContainsKey(virtual_disk_id))
                {
                    parsedDisks.Add(virtual_disk_id, new Disk());
                }

                parsedDisks[virtual_disk_id].sdbbEntryType4.Add(new DataEntry()
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

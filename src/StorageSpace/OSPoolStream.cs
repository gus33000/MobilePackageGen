using System.Text;

namespace StorageSpace
{
    public class OSPoolStream : Stream
    {
        private readonly Stream stream;
        private readonly Dictionary<int, Disk> parsedDisks = [];
        private readonly Disk store;
        private readonly ulong storeIndex;
        private readonly long length;
        private readonly long blockSize = 0x100000;
        private readonly Dictionary<long, int> blockTable;

        private long currentPosition = 0;

        private readonly long ogSeek;

        public OSPoolStream(Stream stream, ulong storeIndex)
        {
            long ogSeek = stream.Position;

            this.stream = stream;
            this.storeIndex = storeIndex;

            using BinaryReader reader = new(stream);

            byte[] spaceDbHeader = reader.ReadBytes(8);
            string spaceDbHeaderStr = Encoding.ASCII.GetString(spaceDbHeader, 0, spaceDbHeader.Length);

            if (spaceDbHeaderStr != "SPACEDB ")
            {
                throw new Exception("Invalid OSPool!");
            }

            stream.Seek(0x18, SeekOrigin.Current);

            Guid storagePoolUUID = new(reader.ReadBytes(16));
            Guid physicalDiskUUID = new(reader.ReadBytes(16));

            int SDBCOffset = 0x1000;

            stream.Seek(ogSeek + SDBCOffset, SeekOrigin.Begin);

            byte[] sdBcHeader = reader.ReadBytes(8);
            string sdBcHeaderStr = Encoding.ASCII.GetString(sdBcHeader, 0, sdBcHeader.Length);

            if (sdBcHeaderStr != "SDBC    ")
            {
                throw new Exception("Invalid SDBC!");
            }

            stream.Seek(8, SeekOrigin.Current);

            Guid sdbcStoragePoolUUID = new(reader.ReadBytes(16));

            if (sdbcStoragePoolUUID != storagePoolUUID)
            {
                throw new Exception("Invalid OSPool! SDBC is not for the given SpaceDB!");
            }

            stream.Seek(4, SeekOrigin.Current);

            uint sdbbEntrySize = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
            uint nextSdbbEntryNumber = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());

            stream.Seek(0x1C, SeekOrigin.Current);

            DateTime sdbbEntryModifiedTime = DateTime.FromFileTime(BitConverter.ToInt64(reader.ReadBytes(8).Reverse().ToArray()));

            stream.Seek(ogSeek + SDBCOffset + 8 * sdbbEntrySize, SeekOrigin.Begin);

            List<byte[]> sdbbEntryType1 = [];
            List<byte[]> sdbbEntryType2 = [];
            List<byte[]> sdbbEntryType3 = [];
            List<byte[]> sdbbEntryType4 = [];

            Dictionary<int, byte[]> entryDataList = [];

            for (int j = 8; j < nextSdbbEntryNumber; j++)
            {
                stream.Seek(8, SeekOrigin.Current);
                int entryIndex = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray());
                stream.Seek(2, SeekOrigin.Current);
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

            for (int j = 8; j < nextSdbbEntryNumber; j++)
            {
                if (!entryDataList.ContainsKey(j))
                {
                    continue;
                }

                byte[] entry = entryDataList[j];

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

            ParseEntryType2(sdbbEntryType2);
            ParseEntryType3(sdbbEntryType3);
            ParseEntryType4(sdbbEntryType4);

            store = parsedDisks[(int)storeIndex];

            (length, blockTable) = BuildBlockTable();

            stream.Seek(ogSeek, SeekOrigin.Begin);
        }

        public static Dictionary<int, string> GetDisks(Stream stream)
        {
            Dictionary<int, string> disks = [];

            long ogSeek = stream.Position;

            using BinaryReader reader = new(stream);

            byte[] spaceDbHeader = reader.ReadBytes(8);
            string spaceDbHeaderStr = Encoding.ASCII.GetString(spaceDbHeader, 0, spaceDbHeader.Length);

            if (spaceDbHeaderStr != "SPACEDB ")
            {
                throw new Exception("Invalid OSPool!");
            }

            stream.Seek(0x18, SeekOrigin.Current);

            Guid storagePoolUUID = new(reader.ReadBytes(16));
            Guid physicalDiskUUID = new(reader.ReadBytes(16));

            int SDBCOffset = 0x1000;

            stream.Seek(ogSeek + SDBCOffset, SeekOrigin.Begin);

            byte[] sdBcHeader = reader.ReadBytes(8);
            string sdBcHeaderStr = Encoding.ASCII.GetString(sdBcHeader, 0, sdBcHeader.Length);

            if (sdBcHeaderStr != "SDBC    ")
            {
                throw new Exception("Invalid SDBC!");
            }

            stream.Seek(8, SeekOrigin.Current);

            Guid sdbcStoragePoolUUID = new(reader.ReadBytes(16));

            if (sdbcStoragePoolUUID != storagePoolUUID)
            {
                throw new Exception("Invalid OSPool! SDBC is not for the given SpaceDB!");
            }

            stream.Seek(4, SeekOrigin.Current);

            uint sdbbEntrySize = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
            uint nextSdbbEntryNumber = BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());

            stream.Seek(0x1C, SeekOrigin.Current);

            DateTime sdbbEntryModifiedTime = DateTime.FromFileTime(BitConverter.ToInt64(reader.ReadBytes(8).Reverse().ToArray()));

            stream.Seek(ogSeek + SDBCOffset + 8 * sdbbEntrySize, SeekOrigin.Begin);

            List<byte[]> sdbbEntryType1 = [];
            List<byte[]> sdbbEntryType2 = [];
            List<byte[]> sdbbEntryType3 = [];
            List<byte[]> sdbbEntryType4 = [];

            Dictionary<int, byte[]> entryDataList = [];

            for (int j = 8; j < nextSdbbEntryNumber; j++)
            {
                stream.Seek(8, SeekOrigin.Current);
                int entryIndex = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray());
                stream.Seek(2, SeekOrigin.Current);
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

            for (int j = 8; j < nextSdbbEntryNumber; j++)
            {
                if (!entryDataList.ContainsKey(j))
                {
                    continue;
                }

                byte[] entry = entryDataList[j];

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

            foreach (byte[] sdbbEntry in sdbbEntryType3)
            {
                int tempOffset = 0;
                int dataRecordLen = sdbbEntry[tempOffset];
                int virtualDiskId = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                tempOffset += 0x02;

                if (sdbbEntry[tempOffset] == 0x01)
                {
                    tempOffset += sdbbEntry[tempOffset] + 1;
                }

                tempOffset += 0x10;

                int virtualDiskNameLength = BitConverter.ToUInt16(sdbbEntry.Skip(tempOffset).Take(0x02).Reverse().ToArray(), 0);
                tempOffset += 0x02;

                byte[] virtualDiskName = new byte[virtualDiskNameLength * 2];
                byte[] tempVirtualDiskName = sdbbEntry.Skip(tempOffset).Take(virtualDiskNameLength * 2).ToArray();
                tempOffset += virtualDiskNameLength * 2;
                for (int j = 0; j < virtualDiskNameLength * 2; j += 2)
                {
                    virtualDiskName[j] = tempVirtualDiskName[j + 1];
                    virtualDiskName[j + 1] = tempVirtualDiskName[j];
                }

                int diskId = virtualDiskId;
                string diskName = Encoding.Unicode.GetString(virtualDiskName);

                disks.Add(diskId, diskName);
            }

            stream.Seek(ogSeek, SeekOrigin.Begin);

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
            foreach (byte[] sdbbEntry in sdbbEntryType3)
            {
                int tempOffset = 0;
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
                tempOffset += 0x02;

                byte[] virtualDiskName = new byte[virtualDiskNameLength * 2];
                byte[] tempVirtualDiskName = sdbbEntry.Skip(tempOffset).Take(virtualDiskNameLength * 2).ToArray();
                tempOffset += virtualDiskNameLength * 2;
                for (int j = 0; j < virtualDiskNameLength * 2; j += 2)
                {
                    virtualDiskName[j] = tempVirtualDiskName[j + 1];
                    virtualDiskName[j + 1] = tempVirtualDiskName[j];
                }

                int diskDescriptionLength = BitConverter.ToUInt16(sdbbEntry.Skip(tempOffset).Take(0x02).Reverse().ToArray(), 0);
                tempOffset += 0x02;

                byte[] diskDescription = new byte[diskDescriptionLength * 2];
                byte[] tempDiskDescription = sdbbEntry.Skip(tempOffset).Take(diskDescriptionLength * 2).ToArray();
                tempOffset += diskDescriptionLength * 2;
                for (int j = 0; j < diskDescriptionLength * 2; j += 2)
                {
                    diskDescription[j] = tempDiskDescription[j + 1];
                    diskDescription[j + 1] = tempDiskDescription[j];
                }

                tempOffset += 3;

                int virtualDiskBlockNumber = 0;

                dataRecordLen = sdbbEntry[tempOffset];
                dataRecordLen -= 3;
                virtualDiskBlockNumber = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray()) / 0x10;

                if (virtualDiskBlockNumber == 0)
                {
                    dataRecordLen = sdbbEntry[tempOffset];
                    virtualDiskBlockNumber = BigEndianToInt(sdbbEntry.Skip(tempOffset + 1).Take(dataRecordLen).ToArray());
                }

                int diskId = virtualDiskId;
                Guid diskUuid = new(virtualDiskUuid);
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
